using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers.Concurrency
{
    public static class SemaphoreSlimHandleExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SemaphoreSlimHandle EnterImmediately(this SemaphoreSlim semaphore)
        {
            return EnterBlocking(semaphore, TimeSpan.Zero);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SemaphoreSlimHandle EnterBlocking(this SemaphoreSlim semaphore)
        {
            return EnterBlocking(semaphore, Timeout.InfiniteTimeSpan, CancellationToken.None);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<SemaphoreSlimHandle> EnterBlockingAsync(this SemaphoreSlim semaphore)
        {
            return EnterBlockingAsync(semaphore, Timeout.InfiniteTimeSpan, CancellationToken.None);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SemaphoreSlimHandle EnterBlocking(
            this SemaphoreSlim semaphore,
            TimeSpan timeout)
        {
            return EnterBlocking(semaphore, timeout, CancellationToken.None);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<SemaphoreSlimHandle> EnterBlockingAsync(
            this SemaphoreSlim semaphore,
            TimeSpan timeout)
        {
            return EnterBlockingAsync(semaphore, timeout, CancellationToken.None);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SemaphoreSlimHandle EnterBlocking(
            this SemaphoreSlim semaphore,
            CancellationToken token)
        {
            return EnterBlocking(semaphore, Timeout.InfiniteTimeSpan, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<SemaphoreSlimHandle> EnterBlockingAsync(
            this SemaphoreSlim semaphore,
            CancellationToken token)
        {
            return EnterBlockingAsync(semaphore, Timeout.InfiniteTimeSpan, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SemaphoreSlimHandle EnterBlocking(
            this SemaphoreSlim semaphore,
            TimeSpan timeout,
            CancellationToken token)
        {
            bool isEntered = semaphore.Wait(timeout, token);

            return new SemaphoreSlimHandle(semaphore, isEntered);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<SemaphoreSlimHandle> EnterBlockingAsync(
            this SemaphoreSlim semaphore,
            TimeSpan timeout,
            CancellationToken token)
        {
            bool isEntered = await semaphore
                .WaitAsync(timeout, token)
                .ConfigureAwait(false);

            return new SemaphoreSlimHandle(semaphore, isEntered);
        }
    }
}
