using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DotNext.Net.Cluster.Consensus.Raft.TransportServices
{
    using IO;
    using IO.Log;

    [ExcludeFromCodeCoverage]
    public abstract class TransportTestSuite : Test
    {
        private sealed class BufferedEntry : BinaryTransferObject, IRaftLogEntry
        {
            internal BufferedEntry(long term, DateTimeOffset timestamp, bool isSnapshot, byte[] content)
                : base(content)
            {
                Term = term;
                Timestamp = timestamp;
                IsSnapshot = isSnapshot;
            }

            public long Term { get; }


            public DateTimeOffset Timestamp { get; }

            public bool IsSnapshot { get; }

        }

        public enum ReceiveEntriesBehavior
        {
            ReceiveAll = 0,
            ReceiveFirst,
            DropAll,
            DropFirst
        }

        private sealed class SimpleServerExchangePool : Assert, ILocalMember, IExchangePool
        {
            internal readonly IList<BufferedEntry> ReceivedEntries = new List<BufferedEntry>();
            internal ReceiveEntriesBehavior Behavior;

            internal SimpleServerExchangePool(bool smallAmountOfMetadata = false)
            {
                var metadata = ImmutableDictionary.CreateBuilder<string, string>();
                if(smallAmountOfMetadata)
                    metadata.Add("a", "b");
                else
                {
                    var rnd = new Random();
                    const string AllowedChars = "abcdefghijklmnopqrstuvwxyz1234567890";
                    for(var i = 0; i < 20; i++)
                        metadata.Add(string.Concat("key", i.ToString()), rnd.NextString(AllowedChars, 20));
                }
                Metadata = metadata.ToImmutableDictionary();
            }

            IPEndPoint ILocalMember.Address => throw new NotImplementedException();

            bool ILocalMember.IsLeader(IRaftClusterMember member) => throw new NotImplementedException();

            Task<bool> ILocalMember.ResignAsync(CancellationToken token) => Task.FromResult(true);

            async Task<Result<bool>> ILocalMember.ReceiveEntriesAsync<TEntry>(EndPoint sender, long senderTerm, ILogEntryProducer<TEntry> entries, long prevLogIndex, long prevLogTerm, long commitIndex, CancellationToken token)
            {
                Equal(42L, senderTerm);
                Equal(1, prevLogIndex);
                Equal(56L, prevLogTerm);
                Equal(10, commitIndex);
                byte[] buffer;
                switch(Behavior)
                {
                    case ReceiveEntriesBehavior.ReceiveAll:
                        while(await entries.MoveNextAsync())
                        {
                            True(entries.Current.Length.HasValue);
                            buffer = await entries.Current.ToByteArrayAsync(token);
                            ReceivedEntries.Add(new BufferedEntry(entries.Current.Term, entries.Current.Timestamp, entries.Current.IsSnapshot, buffer));
                        }
                        break;
                    case ReceiveEntriesBehavior.DropAll:
                        break;
                    case ReceiveEntriesBehavior.ReceiveFirst:
                        True(await entries.MoveNextAsync());
                        buffer = await entries.Current.ToByteArrayAsync(token);
                        ReceivedEntries.Add(new BufferedEntry(entries.Current.Term, entries.Current.Timestamp, entries.Current.IsSnapshot, buffer));
                        break;
                    case ReceiveEntriesBehavior.DropFirst:
                        True(await entries.MoveNextAsync());
                        True(await entries.MoveNextAsync());
                        buffer = await entries.Current.ToByteArrayAsync(token);
                        ReceivedEntries.Add(new BufferedEntry(entries.Current.Term, entries.Current.Timestamp, entries.Current.IsSnapshot, buffer));
                        break;
                }
                
                return new Result<bool>(43L, true);
            }

            async Task<Result<bool>> ILocalMember.ReceiveSnapshotAsync<TSnapshot>(EndPoint sender, long senderTerm, TSnapshot snapshot, long snapshotIndex, CancellationToken token)
            {
                Equal(42L, senderTerm);
                Equal(10, snapshotIndex);
                True(snapshot.IsSnapshot);
                var buffer = await snapshot.ToByteArrayAsync(token);
                ReceivedEntries.Add(new BufferedEntry(snapshot.Term, snapshot.Timestamp, snapshot.IsSnapshot, buffer));
                return new Result<bool>(43L, true);
            }

            Task<Result<bool>> ILocalMember.ReceiveVoteAsync(EndPoint sender, long term, long lastLogIndex, long lastLogTerm, CancellationToken token)
            {
                True(token.CanBeCanceled);
                Equal(42L, term);
                Equal(1L, lastLogIndex);
                Equal(56L, lastLogTerm);
                return Task.FromResult(new Result<bool>(43L, true));
            }

            public bool TryRent(PacketHeaders headers, out IExchange exchange)
            {
                exchange = new ServerExchange(this);
                return true;
            }

            public IReadOnlyDictionary<string, string> Metadata { get; }

            void IExchangePool.Release(IExchange exchange)
                => ((ServerExchange)exchange).Reset();
        }

        private protected delegate IServer ServerFactory(IPEndPoint address, TimeSpan timeout);
        private protected delegate IClient ClientFactory(IPEndPoint address);

        private protected async Task RequestResponseTest(ServerFactory serverFactory, ClientFactory clientFactory)
        {
            var timeout = TimeSpan.FromSeconds(20);
            //prepare server
            var serverAddr = new IPEndPoint(IPAddress.Loopback, 3789);
            using var server = serverFactory(serverAddr, timeout);
            server.Start(new SimpleServerExchangePool());
            //prepare client
            using var client = clientFactory(serverAddr);
            client.Start();
            //Vote request
            CancellationTokenSource timeoutTokenSource;
            Result<bool> result;
            using(timeoutTokenSource = new CancellationTokenSource(timeout))
            {
                var exchange = new VoteExchange(42L, 1L, 56L);
                client.Enqueue(exchange, timeoutTokenSource.Token);
                result = await exchange.Task;
                True(result.Value);
                Equal(43L, result.Term);
            }
            //Resign request
            using(timeoutTokenSource = new CancellationTokenSource(timeout))
            {
                var exchange = new ResignExchange();
                client.Enqueue(exchange, timeoutTokenSource.Token);
                True(await exchange.Task);
            }
            //Heartbeat request
            using(timeoutTokenSource = new CancellationTokenSource(timeout))
            {
                var exchange = new HeartbeatExchange(42L, 1L, 56L, 10L);
                client.Enqueue(exchange, timeoutTokenSource.Token);
                result = await exchange.Task;
                True(result.Value);
                Equal(43L, result.Term);
            }
        }

        private protected async Task StressTestTest(ServerFactory serverFactory, ClientFactory clientFactory)
        {
            var timeout = TimeSpan.FromSeconds(20);
            //prepare server
            var serverAddr = new IPEndPoint(IPAddress.Loopback, 3789);
            using var server = serverFactory(serverAddr, timeout);
            server.Start(new SimpleServerExchangePool());
            //prepare client
            using var client = clientFactory(serverAddr);
            client.Start();
            ICollection<Task<Result<bool>>> tasks = new LinkedList<Task<Result<bool>>>();
            using(var timeoutTokenSource = new CancellationTokenSource(timeout))
            {
                for(var i = 0; i < 100; i++)
                {
                    var exchange = new VoteExchange(42L, 1L, 56L);
                    client.Enqueue(exchange, timeoutTokenSource.Token);
                    tasks.Add(exchange.Task);
                }
                await Task.WhenAll(tasks);
            }
            foreach(var task in tasks)
            {
                True(task.Result.Value);
                Equal(43L, task.Result.Term);
            }
        }

        private protected async Task MetadataRequestResponseTest(ServerFactory serverFactory, ClientFactory clientFactory, bool smallAmountOfMetadata)
        {
            var timeout = TimeSpan.FromSeconds(20);
            //prepare server
            var serverAddr = new IPEndPoint(IPAddress.Loopback, 3789);
            using var server = serverFactory(serverAddr, timeout);
            var exchangePool = new SimpleServerExchangePool(smallAmountOfMetadata);
            server.Start(exchangePool);
            //prepare client
            using var client = clientFactory(serverAddr);
            client.Start();
            var exchange = new MetadataExchange(CancellationToken.None);
            client.Enqueue(exchange, default);
            Equal(exchangePool.Metadata, await exchange.Task);
        }

        private static void Equal(in BufferedEntry x, in BufferedEntry y)
        {
            Equal(x.Term, y.Term);
            Equal(x.Timestamp, y.Timestamp);
            Equal(x.IsSnapshot, y.IsSnapshot);
            True(x.Content.IsSingleSegment);
            True(y.Content.IsSingleSegment);
            True(x.Content.FirstSpan.SequenceEqual(y.Content.FirstSpan));
        }

        private protected async Task SendingLogEntriesTest(ServerFactory serverFactory, ClientFactory clientFactory, int payloadSize, ReceiveEntriesBehavior behavior)
        {
            var timeout = TimeSpan.FromSeconds(20);
            using var timeoutTokenSource = new CancellationTokenSource(timeout);
            //prepare server
            var serverAddr = new IPEndPoint(IPAddress.Loopback, 3789);
            using var server = serverFactory(serverAddr, timeout);
            var exchangePool = new SimpleServerExchangePool(false) { Behavior = behavior };
            server.Start(exchangePool);
            //prepare client
            using var client = clientFactory(serverAddr);
            client.Start();
            var buffer = new byte[533];
            var rnd = new Random();
            rnd.NextBytes(buffer);
            var entry1 = new BufferedEntry(10L, DateTimeOffset.Now, false, buffer);
            buffer = new byte[payloadSize];
            rnd.NextBytes(buffer);
            var entry2 = new BufferedEntry(11L, DateTimeOffset.Now, true, buffer);

            await using var exchange = new EntriesExchange<BufferedEntry, BufferedEntry[]>(42L, new[]{ entry1, entry2 }, 1, 56, 10);
            client.Enqueue(exchange, timeoutTokenSource.Token);
            var result = await exchange.Task;
            Equal(43L, result.Term);
            True(result.Value);
            switch(behavior)
            {
                case ReceiveEntriesBehavior.ReceiveAll:
                    Equal(2, exchangePool.ReceivedEntries.Count);
                    Equal(entry1, exchangePool.ReceivedEntries[0]);
                    Equal(entry2, exchangePool.ReceivedEntries[1]);
                    break;
                case ReceiveEntriesBehavior.ReceiveFirst:
                    Equal(1, exchangePool.ReceivedEntries.Count);
                    Equal(entry1, exchangePool.ReceivedEntries[0]);
                    break;
                case ReceiveEntriesBehavior.DropFirst:
                    Equal(1, exchangePool.ReceivedEntries.Count);
                    Equal(entry2, exchangePool.ReceivedEntries[0]);
                    break;
                case ReceiveEntriesBehavior.DropAll:
                    Empty(exchangePool.ReceivedEntries);
                    break;
            }
        }

        private protected async Task SendingSnapshotTest(ServerFactory serverFactory, ClientFactory clientFactory, int payloadSize)
        {
            var timeout = TimeSpan.FromSeconds(20);
            using var timeoutTokenSource = new CancellationTokenSource(timeout);
            //prepare server
            var serverAddr = new IPEndPoint(IPAddress.Loopback, 3789);
            using var server = serverFactory(serverAddr, timeout);
            var exchangePool = new SimpleServerExchangePool(false);
            server.Start(exchangePool);
            //prepare client
            using var client = clientFactory(serverAddr);
            client.Start();
            var buffer = new byte[payloadSize];
            new Random().NextBytes(buffer);
            var snapshot = new BufferedEntry(10L, DateTimeOffset.Now, true, buffer);
            await using var exchange = new SnapshotExchange(42L, snapshot, 10L);
            client.Enqueue(exchange, timeoutTokenSource.Token);
            var result = await exchange.Task;
            Equal(43L, result.Term);
            True(result.Value);
            NotEmpty(exchangePool.ReceivedEntries);
            Equal(snapshot, exchangePool.ReceivedEntries[0]);
        }
    }
}