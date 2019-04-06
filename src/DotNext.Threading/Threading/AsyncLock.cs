using System;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.CompilerServices.Unsafe;

namespace DotNext.Threading
{
    using Generic;
    using Tasks;
    
    /// <summary>
    /// Unified representation of asynchronous exclusive lock, semaphore lock, read lock, write lock or upgradable read lock.
    /// </summary>
    /// <remarks>
    /// Lock acquisition is asynchronous operation. Note that non-blocking asynchronous locks are not intersected with
    /// their blocking alternatives except semaphore. It means that exclusive lock obtained in blocking manner doesn't
    /// prevent acquisition of asynchronous lock obtained in non-blocking manner.
    /// </remarks>
    /// <seealso cref="Lock"/>
    public struct AsyncLock : IDisposable, IEquatable<AsyncLock>
    {
        internal enum Type : byte
        {
            None = 0,
            Exclusive,
            ReadLock,
            UpgradableReadLock,
            WriteLock,
            Semaphore
        }

        /// <summary>
        /// Represents acquired asynchronous lock.
        /// </summary>
        /// <remarks>
        /// The lock can be released by calling <see cref="Dispose()"/>.
        /// </remarks>
        public struct Holder : IDisposable 
        {
            internal static readonly Func<Task<Holder>, Holder> TimeoutChecker = task =>
            {
                var holder = task.Result;
                holder.ThrowIfEmpty<TimeoutException>();
                return holder;
            };
            private readonly object lockedObject;
            private readonly Type type;

            internal Holder(object lockedObject, Type type)
            {
                this.lockedObject = lockedObject;
                this.type = type;
            }

            internal void ThrowIfEmpty<E>()
                where E: Exception, new()
            {
                if(lockedObject is null)
                    throw new E();
            }

            /// <summary>
            /// Releases the acquired lock.
            /// </summary>
            /// <remarks>
            /// This object is not reusable after calling of this method.
            /// </remarks>
            public void Dispose()
            {
                switch (type)
                {
                    case Type.Exclusive:
                        As<AsyncExclusiveLock>(lockedObject).Release();
                        break;
                    case Type.ReadLock:
                        As<AsyncReaderWriterLock>(lockedObject).ExitReadLock();
                        break;
                    case Type.WriteLock:
                        As<AsyncReaderWriterLock>(lockedObject).ExitWriteLock();
                        break;
                    case Type.UpgradableReadLock:
                        As<AsyncReaderWriterLock>(lockedObject).ExitUpgradableReadLock();
                        break;
                    case Type.Semaphore:
                        As<SemaphoreSlim>(lockedObject).Release(1);
                        break;
                }
                this = default;
            }

            /// <summary>
            /// Indicates that the object holds successfully acquired lock.
            /// </summary>
            /// <param name="holder">The lock holder.</param>
            /// <returns><see langword="true"/>, if the object holds successfully acqured lock; otherwise, <see langword="false"/>.</returns>
            public static bool operator true(in Holder holder) => !(holder.lockedObject is null);

            /// <summary>
            /// Indicates that the object doesn't hold the lock.
            /// </summary>
            /// <param name="holder">The lock holder.</param>
            /// <returns><see langword="false"/>, if the object holds successfully acqured lock; otherwise, <see langword="true"/>.</returns>
            public static bool operator false(in Holder holder) => holder.lockedObject is null;
        }

        private readonly object lockedObject;
        private readonly Type type;
        private readonly bool owner;

        private AsyncLock(object lockedObject, Type type, bool owner)
        {
            this.lockedObject = lockedObject;
            this.type = type;
            this.owner = owner;
        }

        /// <summary>
        /// Creates exclusive asynchronous lock but doesn't acquire it.
        /// </summary>
        /// <remarks>
        /// Constructed lock owns the exclusive lock instance.
        /// </remarks>
        /// <returns>Exclusive asynchronous lock.</returns>
        /// <seealso cref="AsyncExclusiveLock"/>
        public static AsyncLock Exclusive() => new AsyncLock(new AsyncExclusiveLock(), Type.Exclusive, true);

        /// <summary>
        /// Wraps exclusive lock into the unified representation of asynchronous lock.
        /// </summary>
        /// <param name="lock">The lock object to be wrapped.</param>
        /// <returns>Exclusive asynchronous lock.</returns>
        public static AsyncLock Exclusive(AsyncExclusiveLock @lock) => new AsyncLock(@lock ?? throw new ArgumentNullException(nameof(@lock)), Type.Exclusive, false);

        /// <summary>
        /// Wraps semaphore instance into the unified representation of the lock.
        /// </summary>
        /// <param name="semaphore">The semaphore to wrap into lock object.</param>
        /// <returns>The lock representing semaphore.</returns>
        public static AsyncLock Semaphore(SemaphoreSlim semaphore) => new AsyncLock(semaphore ?? throw new ArgumentNullException(nameof(semaphore)), Type.Semaphore, false);
        
        /// <summary>
        /// Creates semaphore-based lock but doesn't acquire the lock.
        /// </summary>
        /// <remarks>
        /// Constructed lock owns the semaphore instance.
        /// </remarks>
        /// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently.</param>
        /// <param name="maxCount">The maximum number of requests for the semaphore that can be granted concurrently.</param>
        /// <returns>The lock representing semaphore.</returns>
        public static AsyncLock Semaphore(int initialCount, int maxCount) => new AsyncLock(new SemaphoreSlim(initialCount, maxCount), Type.Semaphore, true);

        /// <summary>
        /// Creates read lock but doesn't acquire it.
        /// </summary>
        /// <param name="rwLock">Read/write lock source.</param>
        /// <param name="upgradable"><see langword="true"/> to create upgradable read lock wrapper.</param>
        /// <returns>Reader lock.</returns>
        public static AsyncLock ReadLock(AsyncReaderWriterLock rwLock, bool upgradable) 
            => new AsyncLock(rwLock ?? throw new ArgumentNullException(nameof(rwLock)), upgradable ? Type.UpgradableReadLock : Type.ReadLock, false);
        
        /// <summary>
        /// Creates write lock but doesn't acquire it.
        /// </summary>
        /// <param name="rwLock">Read/write lock source.</param>
        /// <returns>Write-only lock.</returns>
        public static AsyncLock WriteLock(AsyncReaderWriterLock rwLock) 
            => new AsyncLock(rwLock ?? throw new ArgumentNullException(nameof(rwLock)), Type.WriteLock, false);

        /// <summary>
        /// Acquires the lock asynchronously.
        /// </summary>
        /// <param name="token">The token that can be used to abort acquisition operation.</param>
        /// <returns>The task returning the acquired lock holder.</returns>
        public Task<Holder> Acquire(CancellationToken token) => TryAcquire(TimeSpan.MaxValue, token).ContinueWith(Holder.TimeoutChecker, AsyncLockHelpers.CheckOnTimeoutOptions);

        /// <summary>
        /// Acquires the lock asynchronously.
        /// </summary>
        /// <param name="timeout">The interval to wait for the lock.</param>
        /// <returns>The task returning the acquired lock holder.</returns>
        /// <exception cref="TimeoutException">The lock cannot be acquired during the specified amount of time.</exception>
        public Task<Holder> Acquire(TimeSpan timeout) => TryAcquire(timeout).ContinueWith(Holder.TimeoutChecker, AsyncLockHelpers.CheckOnTimeoutOptions);
        
        private Task<bool> TryAcquireCore(TimeSpan timeout, CancellationToken token)
        {
            switch(type)
            {
                case Type.Exclusive:
                    return As<AsyncExclusiveLock>(lockedObject).TryAcquire(timeout, token);
                case Type.ReadLock:
                    return As<AsyncReaderWriterLock>(lockedObject).TryEnterReadLock(timeout, token);
                case Type.UpgradableReadLock:
                    return As<AsyncReaderWriterLock>(lockedObject).TryEnterUpgradableReadLock(timeout, token);
                case Type.WriteLock:
                    return As<AsyncReaderWriterLock>(lockedObject).TryEnterWriteLock(timeout, token);
                case Type.Semaphore:
                    return As<SemaphoreSlim>(lockedObject).WaitAsync(timeout, token);
                default:
                    return CompletedTask<bool, BooleanConst.False>.Task;
            }
        }

        /// <summary>
        /// Tries to acquire the lock asynchronously.
        /// </summary>
        /// <param name="timeout">The interval to wait for the lock.</param>
        /// <returns>The task returning the acquired lock holder; or empty lock holder if lock has not been acquired.</returns>
        public Task<Holder> TryAcquire(TimeSpan timeout) => TryAcquire(timeout, default);

        /// <summary>
        /// Tries to acquire the lock asynchronously.
        /// </summary>
        /// <param name="timeout">The interval to wait for the lock.</param>
        /// <param name="token">The token that can be used to abort acquisition operation.</param>
        /// <returns>The task returning the acquired lock holder; or empty lock holder if lock has not been acquired.</returns>
        public async Task<Holder> TryAcquire(TimeSpan timeout, CancellationToken token)
            => await TryAcquireCore(timeout, token) ? new Holder(lockedObject, type) : default;

        /// <summary>
        /// Destroy this lock and dispose underlying lock object if it is owned by the given lock.
        /// </summary>
        /// <remarks>
        /// If the given lock is an owner of the underlying lock object then this method will call <see cref="IDisposable.Dispose()"/> on it;
        /// otherwise, the underlying lock object will not be destroyed.
        /// As a result, this lock is not usable after calling of this method.
        /// </remarks>
        public void Dispose()
        {
            if (owner && lockedObject is IDisposable disposable)
                disposable.Dispose();
            this = default;
        }

        /// <summary>
        /// Determines whether this lock object is the same as other lock.
        /// </summary>
        /// <param name="other">Other lock to compare.</param>
        /// <returns><see langword="true"/> if this lock is the same as the specified lock; otherwise, <see langword="false"/>.</returns>
        public bool Equals(AsyncLock other) => type == other.type && ReferenceEquals(lockedObject, other.lockedObject) && owner == other.owner;

        /// <summary>
        /// Determines whether this lock object is the same as other lock.
        /// </summary>
        /// <param name="other">Other lock to compare.</param>
        /// <returns><see langword="true"/> if this lock is the same as the specified lock; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object other) => other is AsyncLock @lock && Equals(@lock);

        /// <summary>
        /// Computes hash code of this lock.
        /// </summary>
        /// <returns>The hash code of this lock.</returns>
        public override int GetHashCode()
        {
            if (lockedObject is null)
                return 0;
            var hashCode = -549183179;
            hashCode = hashCode * -1521134295 + lockedObject.GetHashCode();
            hashCode = hashCode * -1521134295 + type.GetHashCode();
            hashCode = hashCode * -1521134295 + owner.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Returns actual type of this lock in the form of the string.
        /// </summary>
        /// <returns>The actual type of this lock.</returns>
        public override string ToString() => type.ToString();

        /// <summary>
        /// Determines whether two locks are the same.
        /// </summary>
        /// <param name="first">The first lock to compare.</param>
        /// <param name="second">The second lock to compare.</param>
        /// <returns><see langword="true"/>, if both are the same; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(in AsyncLock first, in AsyncLock second) => ReferenceEquals(first.lockedObject, second.lockedObject) && first.type == second.type && first.owner == second.owner;

        /// <summary>
        /// Determines whether two locks are not the same.
        /// </summary>
        /// <param name="first">The first lock to compare.</param>
        /// <param name="second">The second lock to compare.</param>
        /// <returns><see langword="true"/>, if both are not the same; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(in AsyncLock first, in AsyncLock second) => !ReferenceEquals(first.lockedObject, second.lockedObject) || first.type != second.type || first.owner != second.owner;
    }
}