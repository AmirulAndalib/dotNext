using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DotNext.Threading
{
    /// <summary>
    /// Provides a set of methods to acquire different types of asynchronous lock.
    /// </summary>
    public static class AsyncLockAcquisition
    {
        private static readonly UserDataSlot<AsyncReaderWriterLock> ReaderWriterLock = UserDataSlot<AsyncReaderWriterLock>.Allocate();
        private static readonly UserDataSlot<AsyncExclusiveLock> ExclusiveLock = UserDataSlot<AsyncExclusiveLock>.Allocate();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static AsyncReaderWriterLock GetReaderWriterLock<T>(this T obj)
            where T : class
        {
            switch (obj)
            {
                case null:
                    throw new ArgumentNullException(nameof(obj));
                case AsyncReaderWriterLock rwl:
                    return rwl;
                case ReaderWriterLockSlim _:
                case AsyncExclusiveLock _:
                case SemaphoreSlim _:
                case WaitHandle _:
                case ReaderWriterLock _:
                    throw new ArgumentException(ExceptionMessages.UnsupportedLockAcquisition, nameof(obj));
                default:
                    return obj.GetUserData().GetOrSet(ReaderWriterLock, () => new AsyncReaderWriterLock());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static AsyncLock GetExclusiveLock<T>(this T obj)
            where T : class
        {
            AsyncLock @lock;
            switch (obj)
            {
                case null:
                    throw new ArgumentNullException(nameof(obj));
                case AsyncSharedLock shared:
                    @lock = AsyncLock.Exclusive(shared);
                    break;
                case AsyncExclusiveLock exclusive:
                    @lock = AsyncLock.Exclusive(exclusive);
                    break;
                case SemaphoreSlim semaphore:
                    @lock = AsyncLock.Semaphore(semaphore);
                    break;
                case AsyncReaderWriterLock rwl:
                    @lock = AsyncLock.WriteLock(rwl);
                    break;
                case ReaderWriterLockSlim _:
                case WaitHandle _:
                case ReaderWriterLock _:
                    throw new ArgumentException(ExceptionMessages.UnsupportedLockAcquisition, nameof(obj));
                default:
                    @lock = AsyncLock.Exclusive(obj.GetUserData()
                        .GetOrSet(ExclusiveLock, () => new AsyncExclusiveLock()));
                    break;
            }

            return @lock;
        }

        /// <summary>
        /// Acquires exclusive lock associated with the given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be locked.</typeparam>
        /// <param name="obj">The object to be locked.</param>
        /// <param name="timeout">The interval to wait for the lock.</param>
        /// <returns>The acquired lock holder.</returns>
        /// <exception cref="TimeoutException">The lock cannot be acquired during the specified amount of time.</exception>
        public static Task<AsyncLock.Holder> AcquireLockAsync<T>(this T obj, TimeSpan timeout) where T : class => obj.GetExclusiveLock().Acquire(timeout);

        /// <summary>
        /// Acquires exclusive lock associated with the given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be locked.</typeparam>
        /// <param name="obj">The object to be locked.</param>
        /// <param name="token">The token that can be used to abort acquisition operation.</param>
        /// <returns>The acquired lock holder.</returns>
        public static Task<AsyncLock.Holder> AcquireLockAsync<T>(this T obj, CancellationToken token) where T : class => obj.GetExclusiveLock().Acquire(token);

        /// <summary>
        /// Acquires reader lock associated with the given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be locked.</typeparam>
        /// <param name="obj">The object to be locked.</param>
        /// <param name="timeout">The interval to wait for the lock.</param>
        /// <returns>The acquired lock holder.</returns>
        /// <exception cref="TimeoutException">The lock cannot be acquired during the specified amount of time.</exception>
        public static Task<AsyncLock.Holder> AcquireReadLockAsync<T>(this T obj, TimeSpan timeout) where T : class =>
            AsyncLock.ReadLock(obj.GetReaderWriterLock(), false).Acquire(timeout);

        /// <summary>
        /// Acquires reader lock associated with the given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be locked.</typeparam>
        /// <param name="obj">The object to be locked.</param>
        /// <param name="token">The token that can be used to abort acquisition operation.</param>
        /// <returns>The acquired lock holder.</returns>
        public static Task<AsyncLock.Holder> AcquireReadLockAsync<T>(this T obj, CancellationToken token)
            where T : class => AsyncLock.ReadLock(obj.GetReaderWriterLock(), false).Acquire(token);

        /// <summary>
        /// Acquires writer lock associated with the given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be locked.</typeparam>
        /// <param name="obj">The object to be locked.</param>
        /// <param name="timeout">The interval to wait for the lock.</param>
        /// <returns>The acquired lock holder.</returns>
        /// <exception cref="TimeoutException">The lock cannot be acquired during the specified amount of time.</exception>
        public static Task<AsyncLock.Holder> AcquireWriteLockAsync<T>(this T obj, TimeSpan timeout) where T : class =>
            AsyncLock.WriteLock(obj.GetReaderWriterLock()).Acquire(timeout);

        /// <summary>
        /// Acquires reader lock associated with the given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be locked.</typeparam>
        /// <param name="obj">The object to be locked.</param>
        /// <param name="token">The token that can be used to abort acquisition operation.</param>
        /// <returns>The acquired lock holder.</returns>
        public static Task<AsyncLock.Holder> AcquireWriteLockAsync<T>(this T obj, CancellationToken token)
            where T : class => AsyncLock.WriteLock(obj.GetReaderWriterLock()).Acquire(token);

        /// <summary>
        /// Acquires upgradeable lock associated with the given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be locked.</typeparam>
        /// <param name="obj">The object to be locked.</param>
        /// <param name="timeout">The interval to wait for the lock.</param>
        /// <returns>The acquired lock holder.</returns>
        /// <exception cref="TimeoutException">The lock cannot be acquired during the specified amount of time.</exception>
        public static Task<AsyncLock.Holder> AcquireUpgradeableReadLockAsync<T>(this T obj, TimeSpan timeout)
            where T : class => AsyncLock.ReadLock(obj.GetReaderWriterLock(), true).Acquire(timeout);

        /// <summary>
        /// Acquires upgradeable lock associated with the given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be locked.</typeparam>
        /// <param name="obj">The object to be locked.</param>
        /// <param name="token">The token that can be used to abort acquisition operation.</param>
        /// <returns>The acquired lock holder.</returns>
        public static Task<AsyncLock.Holder> AcquireUpgradeableReadLockAsync<T>(this T obj, CancellationToken token)
            where T : class => AsyncLock.ReadLock(obj.GetReaderWriterLock(), true).Acquire(token);
    }
}