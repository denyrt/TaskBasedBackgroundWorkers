using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers.Concurrency
{
    /// <summary>
    /// An object that wraps <see cref="SemaphoreSlim"/> allowing auto-release it with <see langword="using"/> scope by <see cref="IDisposable"/> implementation.
    /// </summary>
    /// <remarks>
    /// Beware that despite implementing <see cref="IDisposable"/> the underlying <see cref="SemaphoreSlim"/> is not disposed by that object.
    /// </remarks>
    public readonly struct ConcurrentHandle : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly bool _isEntered;

        /// <summary>
        /// Determines if underlying <see cref="SemaphoreSlim"/>.
        /// </summary>
        /// <remarks>
        /// If timeout of <see cref="EnterBlocking(SemaphoreSlim, TimeSpan, CancellationToken)"/> was reached then value will be <see langword="false"/>.
        /// </remarks>
        public bool IsEntered => _isEntered;

        /// <summary>
        /// Initializes an instance of <see cref="ConcurrentHandle"/>.
        /// </summary>
        /// <param name="semaphore"> Semaphore to handle. </param>
        /// <param name="isEntered"> Result of wait execution. </param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <remarks>
        /// Do not initialize <see cref="ConcurrentHandle"/> manually. Instead recommended to use a pre-defined set of static methods.
        /// Otherwise check next notes:
        /// <list type="bullet">
        /// It is expected to call <c>Wait()</c> method of <paramref name="semaphore"/> before passing it as parameter.
        /// </list>
        /// <list type="bullet">
        /// Value of <paramref name="isEntered"/> must be result of <see cref="SemaphoreSlim"/> wait function if it uses timeout, otherwise <see langword="true"/>.
        /// </list>
        /// </remarks>
        public ConcurrentHandle(SemaphoreSlim semaphore, bool isEntered)
        {
            _semaphore = semaphore ?? throw new ArgumentNullException(nameof(semaphore));
            _isEntered = isEntered;
        }

        /// <summary>
        /// Releases the <see cref="SemaphoreSlim"/> object allowing next caller enter semaphore.
        /// </summary>
        public void Dispose()
        {
            if (_isEntered)
            {
                _semaphore.Release();
            }
        }

        private static readonly TimeSpan Indefinitely = TimeSpan.FromMilliseconds(-1);

        public static ConcurrentHandle EnterBlocking(SemaphoreSlim semaphore)
        {
            return EnterBlocking(semaphore, Indefinitely, CancellationToken.None);
        }

        public static Task<ConcurrentHandle> EnterBlockingAsync(SemaphoreSlim semaphore)
        {
            return EnterBlockingAsync(semaphore, Indefinitely, CancellationToken.None);
        }

        public static ConcurrentHandle EnterBlocking(SemaphoreSlim semaphore, TimeSpan timeout)
        {
            return EnterBlocking(semaphore, timeout, CancellationToken.None);
        }

        public static Task<ConcurrentHandle> EnterBlockingAsync(SemaphoreSlim semaphore, TimeSpan timeout)
        {
            return EnterBlockingAsync(semaphore, timeout, CancellationToken.None);
        }

        public static ConcurrentHandle EnterBlocking(SemaphoreSlim semaphore, CancellationToken token)
        {
            return EnterBlocking(semaphore, Indefinitely, token);
        }

        public static Task<ConcurrentHandle> EnterBlockingAsync(SemaphoreSlim semaphore, CancellationToken token)
        {
            return EnterBlockingAsync(semaphore, Indefinitely, token);
        }

        /// <summary>
        /// Waits for entering to <see cref="SemaphoreSlim"/>.
        /// </summary>
        /// <param name="semaphore"> Semaphore that must be entred. </param>
        /// <param name="timeout"> Amount of time that must be reached before force-stop waiting. </param>
        /// <param name="token"> Token that allows cancel this operation by external cancellation request. </param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns> An instance of <see cref="ConcurrentHandle"/>. </returns>
        public static ConcurrentHandle EnterBlocking(SemaphoreSlim semaphore, TimeSpan timeout, CancellationToken token)
        {
            bool isEntered = semaphore.Wait(timeout, token);
            return new ConcurrentHandle(semaphore, isEntered);
        }

        /// <summary>
        /// Waits for entering to <see cref="SemaphoreSlim"/>.
        /// </summary>
        /// <param name="semaphore"> Semaphore that must be entred. </param>
        /// <param name="timeout"> Amount of time that must be reached before force-stop waiting. </param>
        /// <param name="token"> Token that allows cancel this operation by external cancellation request. </param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns> Task that returns an instance of <see cref="ConcurrentHandle"/>. </returns>
        public static async Task<ConcurrentHandle> EnterBlockingAsync(SemaphoreSlim semaphore, TimeSpan timeout, CancellationToken token)
        {
            bool isEntered = await semaphore.WaitAsync(timeout, token);
            return new ConcurrentHandle(semaphore, isEntered);
        }
    }
}
