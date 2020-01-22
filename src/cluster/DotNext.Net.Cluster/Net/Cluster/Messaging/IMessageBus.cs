using System.Collections.Generic;

namespace DotNext.Net.Cluster.Messaging
{
    /// <summary>
    /// Specifies a cloud of nodes that can communicate with each other through the network.
    /// </summary>
    public interface IMessageBus : ICluster
    {
        /// <summary>
        /// Gets the leader node.
        /// </summary>
        new ISubscriber? Leader { get; }

        IClusterMember? ICluster.Leader => Leader;

        /// <summary>
        /// Represents a collection of nodes in the network.
        /// </summary>
        new IReadOnlyCollection<ISubscriber> Members { get; }

        IReadOnlyCollection<IClusterMember> ICluster.Members => Members;

        /// <summary>
        /// Allows to route messages to the leader 
        /// even if it is changed during transmission.
        /// </summary>
        IOutputChannel LeaderRouter { get; }

        /// <summary>
        /// Adds message handler.
        /// </summary>
        /// <param name="handler">The message handler.</param>
        void AddListener(IInputChannel handler);

        /// <summary>
        /// Removes message handler.
        /// </summary>
        /// <param name="handler">The message handler.</param>
        void RemoveListener(IInputChannel handler);
    }
}