using System;

namespace DotNext.Net.Cluster.Consensus.Raft
{
    /// <summary>
    /// Contains a set of callbacks that can be used to report
    /// runtime metrics generated by Raft cluster node.
    /// </summary>
    public struct MetricsCollector
    {
        /// <summary>
        /// The callback used to report broadcast time.
        /// </summary>
        /// <remarks>
        /// Broadcast time is the time spent accessing the cluster nodes caused by Leader states.
        /// </remarks>
        public Action<TimeSpan> BroadcastTimeCallback;
    }
}