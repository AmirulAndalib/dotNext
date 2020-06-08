﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static System.Threading.Timeout;

namespace DotNext.Threading
{
    using Runtime.CompilerServices;

    /// <summary>
    /// Represents a lock that can be acquired in exclusive or weak mode.
    /// </summary>
    /// <remarks>
    /// This lock represents the combination of semaphore and reader-writer
    /// lock. The caller can acquire weak locks simultaneously which count
    /// is limited by the concurrency level passed into the constructor. However, the
    /// only one caller can acquire the lock exclusively.
    /// </remarks>
    public class AsyncSharedLock : QueuedSynchronizer, IAsyncDisposable
    {
        private const long ExclusiveMode = -1L;

        private sealed class StrongLockNode : WaitNode
        {
            internal StrongLockNode()
                : base()
            {
            }

            internal StrongLockNode(WaitNode previous)
                : base(previous)
            {
            }
        }

        [StructLayout(LayoutKind.Auto)]
        private struct State : ILockManager<StrongLockNode>, ILockManager<WaitNode>
        {
            internal readonly long ConcurrencyLevel;
            internal long RemainingLocks;   // -1 means that the lock is acquired in exclusive mode

            internal State(long concurrencyLevel) => ConcurrencyLevel = RemainingLocks = concurrencyLevel;

            internal long IncrementLocks() => RemainingLocks = RemainingLocks == ExclusiveMode ? ConcurrencyLevel : RemainingLocks + 1L;

            internal bool IsEmpty => RemainingLocks == ConcurrencyLevel;

            bool ILockManager<StrongLockNode>.TryAcquire()
            {
                if (RemainingLocks < ConcurrencyLevel)
                    return false;
                RemainingLocks = ExclusiveMode;
                return true;
            }

            StrongLockNode ILockManager<StrongLockNode>.CreateNode(WaitNode? tail) => tail is null ? new StrongLockNode() : new StrongLockNode(tail);

            bool ILockManager<WaitNode>.TryAcquire()
            {
                if (RemainingLocks <= 0L)
                    return false;
                RemainingLocks -= 1L;
                return true;
            }

            WaitNode ILockManager<WaitNode>.CreateNode(WaitNode? tail) => tail is null ? new WaitNode() : new WaitNode(tail);
        }

        private static readonly Func<AsyncSharedLock, bool> IsLockHeldPredicate = DelegateHelpers.CreateOpenDelegate<Func<AsyncSharedLock, bool>>(l => l.IsLockHeld);
        private readonly Box<State> state;

        /// <summary>
        /// Initializes a new shared lock.
        /// </summary>
        /// <param name="concurrencyLevel">The number of unique callers that can obtain shared lock simultaneously.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="concurrencyLevel"/> is less than 1.</exception>
        public AsyncSharedLock(long concurrencyLevel)
        {
            if (concurrencyLevel < 1L)
                throw new ArgumentOutOfRangeException(nameof(concurrencyLevel));
            state = new Box<State>(new State(concurrencyLevel));
        }

        /// <summary>
        /// Gets the number of shared locks that can be acquired.
        /// </summary>
        public long RemainingCount => Math.Max(state.Value.RemainingLocks, 0L);

        /// <summary>
        /// Gets the maximum number of locks that can be obtained simultaneously.
        /// </summary>
        public long ConcurrencyLevel => state.Value.ConcurrencyLevel;

        /// <summary>
        /// Indicates that the lock is acquired in exclusive or shared mode.
        /// </summary>
        public bool IsLockHeld => state.Value.RemainingLocks < ConcurrencyLevel;

        /// <summary>
        /// Attempts to obtain lock synchronously without blocking caller thread.
        /// </summary>
        /// <param name="strongLock"><see langword="true"/> to acquire strong(exclusive) lock; <see langword="false"/> to acquire weak lock.</param>
        /// <returns><see langword="true"/> if the caller entered the lock; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ObjectDisposedException">This object has been disposed.</exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool TryAcquire(bool strongLock)
        {
            ThrowIfDisposed();
            return strongLock ? TryAcquire<StrongLockNode, State>(ref state.Value) : TryAcquire<WaitNode, State>(ref state.Value);
        }

        /// <summary>
        /// Attempts to enter the lock asynchronously, with an optional time-out.
        /// </summary>
        /// <param name="strongLock"><see langword="true"/> to acquire strong(exclusive) lock; <see langword="false"/> to acquire weak lock.</param>
        /// <param name="timeout">The interval to wait for the lock.</param>
        /// <param name="token">The token that can be used to abort lock acquisition.</param>
        /// <returns><see langword="true"/> if the caller entered the lock; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Time-out value is negative.</exception>
        /// <exception cref="ObjectDisposedException">This object has been disposed.</exception>
        public Task<bool> TryAcquireAsync(bool strongLock, TimeSpan timeout, CancellationToken token)
            => strongLock ? WaitAsync<StrongLockNode, State>(ref state.Value, timeout, token) : WaitAsync<WaitNode, State>(ref state.Value, timeout, token);

        /// <summary>
        /// Attempts to enter the lock asynchronously, with an optional time-out.
        /// </summary>
        /// <param name="strongLock"><see langword="true"/> to acquire strong(exclusive) lock; <see langword="false"/> to acquire weak lock.</param>
        /// <param name="timeout">The interval to wait for the lock.</param>
        /// <returns><see langword="true"/> if the caller entered the lock; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Time-out value is negative.</exception>
        /// <exception cref="ObjectDisposedException">This object has been disposed.</exception>
        public Task<bool> TryAcquireAsync(bool strongLock, TimeSpan timeout) => TryAcquireAsync(strongLock, timeout, CancellationToken.None);

        /// <summary>
        /// Entres the lock asynchronously.
        /// </summary>
        /// <param name="strongLock"><see langword="true"/> to acquire strong(exclusive) lock; <see langword="false"/> to acquire weak lock.</param>
        /// <param name="timeout">The interval to wait for the lock.</param>
        /// <param name="token">The token that can be used to abort lock acquisition.</param>
        /// <returns>The task representing lock acquisition operation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Time-out value is negative.</exception>
        /// <exception cref="ObjectDisposedException">This object has been disposed.</exception>
        /// <exception cref="TimeoutException">The lock cannot be acquired during the specified amount of time.</exception>
        public Task AcquireAsync(bool strongLock, TimeSpan timeout, CancellationToken token = default) => TryAcquireAsync(strongLock, timeout, token).CheckOnTimeout();

        /// <summary>
        /// Entres the lock asynchronously.
        /// </summary>
        /// <param name="strongLock"><see langword="true"/> to acquire strong(exclusive) lock; <see langword="false"/> to acquire weak lock.</param>
        /// <param name="token">The token that can be used to abort lock acquisition.</param>
        /// <returns>The task representing lock acquisition operation.</returns>
        /// <exception cref="ObjectDisposedException">This object has been disposed.</exception>
        public Task AcquireAsync(bool strongLock, CancellationToken token) => TryAcquireAsync(strongLock, InfiniteTimeSpan, token);

        private void ResumePendingCallers()
        {
            ref var stateHolder = ref state.Value;
            for (WaitNode? current = head, next; !(current is null || current is StrongLockNode) && stateHolder.RemainingLocks > 0L; stateHolder.RemainingLocks--, current = next)
            {
                next = current.Next;
                RemoveNode(current);
                current.Complete();
            }
        }

        /// <summary>
        /// Releases the acquired weak lock or downgrade exclusive lock to the weak lock.
        /// </summary>
        /// <exception cref="SynchronizationLockException">The caller has not entered the lock.</exception>
        /// <exception cref="ObjectDisposedException">This object has been disposed.</exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Downgrade()
        {
            ThrowIfDisposed();
            ref var stateHolder = ref state.Value;
            if (stateHolder.IsEmpty) // nothing to release
                throw new SynchronizationLockException(ExceptionMessages.NotInWriteLock);

            if (stateHolder.RemainingLocks == ExclusiveMode)
            {
                stateHolder.RemainingLocks = ConcurrencyLevel - 1;
                ResumePendingCallers();
            }
            else if (stateHolder.IncrementLocks() == ConcurrencyLevel && !ProcessDisposeQueue())
            {
                if (head is StrongLockNode exclusiveNode)
                {
                    RemoveNode(exclusiveNode);
                    exclusiveNode.Complete();
                    stateHolder.RemainingLocks = ExclusiveMode;
                }
                else
                {
                    ResumePendingCallers();
                }
            }
        }

        /// <summary>
        /// Release the acquired lock.
        /// </summary>
        /// <exception cref="SynchronizationLockException">The caller has not entered the lock.</exception>
        /// <exception cref="ObjectDisposedException">This object has been disposed.</exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Release()
        {
            ThrowIfDisposed();
            ref var stateHolder = ref state.Value;
            if (stateHolder.IsEmpty) // nothing to release
                throw new SynchronizationLockException(ExceptionMessages.NotInWriteLock);
            if (ProcessDisposeQueue())
                return;

            if (stateHolder.IncrementLocks() == ConcurrencyLevel && head is StrongLockNode exclusiveNode)
            {
                RemoveNode(exclusiveNode);
                exclusiveNode.Complete();
                stateHolder.RemainingLocks = ExclusiveMode;
            }
            else
            {
                ResumePendingCallers();
            }
        }

        /// <summary>
        /// Disposes this lock asynchronously and gracefully.
        /// </summary>
        /// <remarks>
        /// If this lock is not acquired then the method just completes synchronously.
        /// Otherwise, it waits for calling of <see cref="Release"/> method.
        /// </remarks>
        /// <returns>The task representing graceful shutdown of this lock.</returns>
        public ValueTask DisposeAsync()
            => IsDisposed ? new ValueTask() : DisposeAsync(IsLockHeldPredicate.Bind(this));
    }
}
