﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DotNext.Threading
{
    public sealed class AsyncSharedLockTests : Assert
    {
        [Fact]
        public static async Task WeakLocks()
        {
            using(var sharedLock = new AsyncSharedLock(3))
            {
                True(await sharedLock.TryAcquire(false, TimeSpan.Zero));
                True(await sharedLock.TryAcquire(false, TimeSpan.Zero));
                Equal(1, sharedLock.RemainingCount);
                True(await sharedLock.TryAcquire(false, TimeSpan.Zero));
                Equal(0, sharedLock.RemainingCount);
                False(await sharedLock.TryAcquire(false, TimeSpan.Zero));
                False(await sharedLock.TryAcquire(true, TimeSpan.Zero));
                sharedLock.Release();
                Equal(1, sharedLock.RemainingCount);
                False(await sharedLock.TryAcquire(true, TimeSpan.Zero));
                True(await sharedLock.TryAcquire(false, TimeSpan.Zero));
            }
        }

        [Fact]
        public static async Task StrongLocks()
        {
            using (var sharedLock = new AsyncSharedLock(3))
            {
                True(await sharedLock.TryAcquire(true, TimeSpan.Zero));
                False(await sharedLock.TryAcquire(false, TimeSpan.Zero));
                False(await sharedLock.TryAcquire(true, TimeSpan.Zero));
            }
        }

        private static async void AcquireWeakLockAndRelease(AsyncSharedLock sharedLock, AsyncCountdownEvent acquireEvent)
        {
            await Task.Delay(100);
            await sharedLock.Acquire(false, TimeSpan.Zero);
            acquireEvent.Signal();
            await Task.Delay(100);
            sharedLock.Release();
        }

        [Fact]
        public static async Task WeakToStringLockTransition()
        {
            using(var acquireEvent = new AsyncCountdownEvent(3L))
            using (var sharedLock = new AsyncSharedLock(3))
            {
                AcquireWeakLockAndRelease(sharedLock, acquireEvent);
                AcquireWeakLockAndRelease(sharedLock, acquireEvent);
                AcquireWeakLockAndRelease(sharedLock, acquireEvent);
                await acquireEvent.Wait();
                await sharedLock.Acquire(true, TimeSpan.FromSeconds(1));
                
                Equal(0, sharedLock.RemainingCount);
            }
        }

        private static async void AcquireWeakLock(AsyncSharedLock sharedLock, AsyncCountdownEvent acquireEvent)
        {
            await sharedLock.Acquire(false, CancellationToken.None);
            acquireEvent.Signal();
        }

        [Fact]
        public static async Task StrongToWeakLockTransition()
        {
            using (var acquireEvent = new AsyncCountdownEvent(2L))
            using (var sharedLock = new AsyncSharedLock(3))
            {
                await sharedLock.Acquire(true, TimeSpan.Zero);
                AcquireWeakLock(sharedLock, acquireEvent);
                AcquireWeakLock(sharedLock, acquireEvent);
                sharedLock.Release();
                True(await acquireEvent.Wait(TimeSpan.FromSeconds(1)));
                Equal(1, sharedLock.RemainingCount);
            }
        }
    }
}
