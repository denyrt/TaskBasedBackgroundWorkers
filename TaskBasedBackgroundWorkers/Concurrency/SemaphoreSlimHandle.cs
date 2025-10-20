using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers.Concurrency
{
    /// <summary>
    /// An object that wraps <see cref="SemaphoreSlim"/> allowing auto-release it with <see langword="using"/> scope by <see cref="IDisposable"/> implementation.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// Beware that despite implementing <see cref="IDisposable"/> the underlying <see cref="SemaphoreSlim"/> is not disposed by that object.
    /// </list>
    /// </remarks>
    public readonly struct SemaphoreSlimHandle : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly bool _isEntered;

        public bool IsEntered => _isEntered;

        /// <exception cref="ArgumentNullException"></exception>
        public SemaphoreSlimHandle(SemaphoreSlim semaphore, bool isEntered)
        {
            if (semaphore == null)
            {
                throw new ArgumentNullException(nameof(semaphore));
            }

            _semaphore = semaphore;
            _isEntered = isEntered;
        }

        public void Dispose()
        {
            if (_isEntered)
            {
                _semaphore.Release();
            }
        }

        public static SemaphoreSlimHandle EnterImmediately(SemaphoreSlim semaphore)
        {
            return EnterBlocking(semaphore, TimeSpan.Zero);
        }

        public static SemaphoreSlimHandle EnterBlocking(SemaphoreSlim semaphore)
        {
            return EnterBlocking(semaphore, Timeout.InfiniteTimeSpan, CancellationToken.None);
        }

        public static Task<SemaphoreSlimHandle> EnterBlockingAsync(SemaphoreSlim semaphore)
        {
            return EnterBlockingAsync(semaphore, Timeout.InfiniteTimeSpan, CancellationToken.None);
        }

        public static SemaphoreSlimHandle EnterBlocking(
            SemaphoreSlim semaphore,
            TimeSpan timeout)
        {
            return EnterBlocking(semaphore, timeout, CancellationToken.None);
        }

        public static Task<SemaphoreSlimHandle> EnterBlockingAsync(
            SemaphoreSlim semaphore,
            TimeSpan timeout)
        {
            return EnterBlockingAsync(semaphore, timeout, CancellationToken.None);
        }

        public static SemaphoreSlimHandle EnterBlocking(
            SemaphoreSlim semaphore,
            CancellationToken token)
        {
            return EnterBlocking(semaphore, Timeout.InfiniteTimeSpan, token);
        }

        public static Task<SemaphoreSlimHandle> EnterBlockingAsync(
            SemaphoreSlim semaphore,
            CancellationToken token)
        {
            return EnterBlockingAsync(semaphore, Timeout.InfiniteTimeSpan, token);
        }

        public static SemaphoreSlimHandle EnterBlocking(
            SemaphoreSlim semaphore,
            TimeSpan timeout,
            CancellationToken token)
        {
            bool isEntered = semaphore.Wait(timeout, token);

            return new SemaphoreSlimHandle(semaphore, isEntered);
        }

        public static async Task<SemaphoreSlimHandle> EnterBlockingAsync(
            SemaphoreSlim semaphore,
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
