using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;

namespace DotNext.Net.Cluster.Consensus.Raft.Udp
{
    internal sealed class ResignExchange : ClientExchange<bool>
    {
        public override ValueTask<bool> ProcessInbountMessageAsync(PacketHeaders headers, ReadOnlyMemory<byte> payload, EndPoint endpoint, CancellationToken token)
        {
            Debug.Assert(headers.Control == FlowControl.Ack);
            TrySetResult(ValueTypeExtensions.ToBoolean(payload.Span[0]));
            return new ValueTask<bool>(false);
        }

        public override ValueTask<(PacketHeaders Headers, int BytesWritten, bool)> CreateOutboundMessageAsync(Memory<byte> payload, CancellationToken token)
            => new ValueTask<(PacketHeaders, int, bool)>((new PacketHeaders(MessageType.Resign, FlowControl.None), 0, true));
    }
}