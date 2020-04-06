using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DotNext.Net.Cluster.Consensus.Raft.Udp
{
    using static Runtime.Intrinsics;

    internal partial class ServerExchange
    {
        private void BeginVote(ReadOnlyMemory<byte> payload, EndPoint member, CancellationToken token)
        {
            VoteExchange.Parse(payload.Span, out var term, out var lastLogIndex, out var lastLogTerm);
            task = server.ReceiveVoteAsync(member, term, lastLogIndex, lastLogTerm, token);
        }

        private async ValueTask<(PacketHeaders, int, bool)> EndVote(Memory<byte> payload)
        {
            var result = await Cast<Task<Result<bool>>>(Interlocked.Exchange(ref task, null)).ConfigureAwait(false);
            return (new PacketHeaders(MessageType.Vote, FlowControl.Ack), IExchange.WriteResult(result, payload.Span), false);
        }
    }
}