using System.Runtime.CompilerServices;
using Debug = System.Diagnostics.Debug;

namespace DotNext.Net.Cluster.Consensus.Raft.TransportServices.Datagram;

internal sealed class ResignExchange : ClientExchange<bool>
{
    private const string Name = "Resign";

    internal ResignExchange()
        : base(Name)
    {
    }

    public override ValueTask<bool> ProcessInboundMessageAsync(PacketHeaders headers, ReadOnlyMemory<byte> payload, CancellationToken token)
    {
        Debug.Assert(headers.Control == FlowControl.Ack);
        TrySetResult(Unsafe.BitCast<byte, bool>(payload.Span[0]));
        return new(false);
    }

    public override ValueTask<(PacketHeaders Headers, int BytesWritten, bool)> CreateOutboundMessageAsync(Memory<byte> payload, CancellationToken token)
        => new((new PacketHeaders(MessageType.Resign, FlowControl.None), 0, true));
}