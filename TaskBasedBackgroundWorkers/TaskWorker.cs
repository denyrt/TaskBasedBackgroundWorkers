using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers
{
    /// <summary>
    /// A base type for a thread-safe re-runnable worker that supports stop using its own stop method or via external <see cref="CancellationToken"/>.
    /// It is wrapped around underlying task which behaviour could be adjusted by passing <see cref="TaskFactory"/>, <see cref="TaskScheduler"/> 
    /// and <see cref="TaskCreationOptions"/>. So worker behaviour could be setuped for both long-time execution and short-time execution.
    /// </summary>
    /// <remarks>
    /// A few notes:
    /// <list type="bullet">
    /// Worker could be re-started again if <see cref="IsRunning"/> is <see langword="false"/>.
    /// </list>
    /// <list type="bullet"> 
    /// Underlying task setup and clean-up are concurrent synchronized in way that only a of them could be executed at one moment of time.
    /// Worker stopping using either external <see cref="CancellationToken"/> or method <see cref="Stop"/> uses sync for concurrent calls,
    /// so underlying resources associated with running task could be accessed only from one thread. The same sync is used for start methods.
    /// It is required to prevent invalid behaviour when both start and stop called from diffent threads in one moment of time.
    /// </list>
    /// </remarks>
    /// <typeparam name="TProgress"> Type that represents progress of worker do-work execution. </typeparam>
    public abstract class TaskWorker<TProgress> : IDisposable
    {
        // The underlying task factory. TaskScheduler must be present within.
        private readonly TaskFactory _taskFactory;

        // The underlying creation options.
        private readonly TaskCreationOptions _taskCreationOptions;

        // The synchronization of underlying task setup/clean-up to prevent dirty-read and concurrent changes of its state.
        // Must be applied to: _cts, _task, IsRunning.
        private readonly SemaphoreSlim _taskResourcesSemaphoreSlim;

        // The synchronization of _disposed for thread-safe Dispose().
        private readonly SemaphoreSlim _disposedSemaphoreSlim;

        // The underlying indicator of disposal state.
        private bool _disposed = false;

        // ToDo: Consider throwing ObjectDisposedException where this member is accessed.
        private bool IsDisposedBlocking
        {
            get
            {
                try
                {
                    _disposedSemaphoreSlim.Wait();
                }
                catch
                {
                    return true;
                }

                try
                {
                    return _disposed;
                }
                finally
                {
                    _disposedSemaphoreSlim.Release();
                }
            }
        }

        // The underlying cancellation source.
        private CancellationTokenSource _cts;

        // The underlying task.
        private Task _task;

        /// <summary>
        /// Indicates if worker is still running.
        /// </summary>
        public bool IsRunning
        {
            get => _task != null;
        }

        /// <summary>
        /// Blocking access to <see cref="IsRunning"/>.
        /// </summary>
        /// <remarks>
        /// Synced via <see cref="_taskResourcesSemaphoreSlim"/>.
        /// </remarks>
        private bool IsRunningBlocking
        {
            get
            {
                _taskResourcesSemaphoreSlim.Wait();

                try
                {
                    return IsRunning;
                }
                finally
                {
                    _taskResourcesSemaphoreSlim.Release();
                }
            }
        }

        /// <summary>
        /// Raises when worker is started.
        /// </summary>
        public event EventHandler<TaskWorkerStartedEventArgs> Started;

        /// <summary>
        /// Raises when worker is stopped.
        /// </summary>
        /// <remarks>
        /// The underlying task and its resources are cleaned-up already when this event is raised.
        /// </remarks>
        public event EventHandler<TaskWorkerStoppedEventArgs> Stopped;

        /// <summary>
        /// Raises when task progress is adjusted.
        /// </summary>
        public event EventHandler<TaskWorkerProgressChangedEventArgs<TProgress>> ProgressChanged;

        /// <summary>
        /// Raises when do-work method failed with unxpected exception.
        /// </summary>
        /// <remarks>
        /// The underlying task and its resources are cleaned-up already when this event is raised.
        /// </remarks>
        public event EventHandler<TaskWorkerExceptionEventArgs> ExceptionThrown;

        /// <summary>
        /// Initializes new instance of <see cref="TaskWorker{TProgress}"/> using <see cref="TaskScheduler.Default"/> and <see cref="TaskCreationOptions.None"/> as parameters.
        /// </summary>
        public TaskWorker() : this(TaskScheduler.Default, TaskCreationOptions.None)
        {
        }

        /// <summary>
        /// Initializes new instance of <see cref="TaskWorker{TProgress}"/> using custom <see cref="TaskScheduler"/> and <see cref="TaskCreationOptions"/>.
        /// </summary>
        /// <param name="taskScheduler"> Task scheduler that will be used within inner task factory. </param>
        /// <param name="taskCreationOptions"> Options that will be used to run worker. </param>
        public TaskWorker(TaskScheduler taskScheduler, TaskCreationOptions taskCreationOptions) : this(new TaskFactory(taskScheduler), taskCreationOptions)
        {
        }

        /// <summary>
        /// Initializes new instance of <see cref="TaskWorker{TProgress}"/> with custom <see cref="TaskFactory"/> and <see cref="TaskCreationOptions"/>.
        /// </summary>
        /// <param name="taskFactory"> Task factory that will be used to create tasks for worker. </param>
        /// <param name="taskCreationOptions"> Options that will be used to run worker. </param>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// <paramref name="taskFactory"/> must have not null <see cref="TaskScheduler"/>
        /// </remarks>
        public TaskWorker(TaskFactory taskFactory, TaskCreationOptions taskCreationOptions)
        {
            DebugEnter();

            if (taskFactory.Scheduler == null)
            {
                throw new ArgumentException($"Cannot accept task factory without scheduler.", nameof(taskFactory.Scheduler));
            }

            _taskFactory = taskFactory;
            _taskCreationOptions = taskCreationOptions;
            _taskResourcesSemaphoreSlim = new SemaphoreSlim(1, 1);
            _disposedSemaphoreSlim = new SemaphoreSlim(1, 1);

            DebugExit();
        }

        // Finalizer.
        ~TaskWorker()
        {
            DebugEnter();
            Dispose(false);
            DebugExit();
        }

        /// <summary>
        /// Work that is executed by worker on start.
        /// </summary>
        /// <param name="progress">
        /// A progress that could be used to raise <see cref="ProgressChanged"/>. Its life-time is bound to task-associated resources.
        /// </param>
        /// <param name="cancellationToken"> 
        /// Cancellation token provided by worker.
        /// </param>
        /// <returns> 
        /// A task that represents async operation. 
        /// </returns>
        protected abstract Task DoWorkAsync(IProgress<TProgress> progress, CancellationToken cancellationToken);

        private void OnStarted(TaskWorkerStartedEventArgs e)
        {
            DebugEnter();

            Started?.Invoke(this, e);

            DebugExit();
        }

        private void OnStopped(TaskWorkerStoppedEventArgs e)
        {
            DebugEnter();

            Stopped?.Invoke(this, e);

            DebugExit();
        }

        private void OnProgressChanged(TaskWorkerProgressChangedEventArgs<TProgress> e)
        {
            DebugEnter();

            ProgressChanged?.Invoke(this, e);

            DebugExit();
        }

        private void OnExceptionThrown(TaskWorkerExceptionEventArgs e)
        {
            DebugEnter();

            ExceptionThrown?.Invoke(this, e);

            DebugExit();
        }

        /// <summary>
        /// Starts worker that executes do-work once upon its run.
        /// </summary>
        /// <param name="linkedToken"> 
        /// A bound cancellation token. Cancellation request this token will lead to stop of current worker. 
        /// </param>
        /// <exception cref="ArgumentException"></exception>
        public StartResult Start(CancellationToken linkedToken = default)
        {
            var tokens = new CancellationToken[1] { linkedToken };
            
            try
            {
                return Start(tokens);
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Starts worker that executes do-work once upon its run.
        /// </summary>
        /// <param name="linkedTokens"> 
        /// A bunch of bound cancellation tokens. Cancellation request from one of these tokens will lead to stop of current worker. 
        /// </param>
        /// <exception cref="ArgumentException"></exception>
        public StartResult Start(IEnumerable<CancellationToken> linkedTokens)
        {
            var tokens = linkedTokens.ToArray();

            try
            {
                return Start(tokens);
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Starts worker that executes do-work once upon its run.
        /// </summary>
        /// <param name="linkedTokens"> 
        /// A bunch of bound cancellation tokens. Cancellation request from one of these tokens will lead to stop of current worker. 
        /// </param>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks> Uses blocking-sync for concurrent call. </remarks>
        /// <returns> A state that describes start operation result. </returns>
        public StartResult Start(CancellationToken[] linkedTokens)
        {
            if (linkedTokens.Length == 0)
            {
                throw new ArgumentException("Linked tokens cannot be empty.", nameof(linkedTokens));
            }

            if (IsDisposedBlocking) 
            {
                return StartResult.None;
            }

            _taskResourcesSemaphoreSlim.Wait();

            try
            {
                if (IsRunning)
                {
                    return StartResult.StopRequired;
                }

                var cts = CancellationTokenSource.CreateLinkedTokenSource(linkedTokens);

                CreateAndStartTask(cts);
            }
            finally
            {
                _taskResourcesSemaphoreSlim.Release();
            }

            return StartResult.Ok;
        }

        /// <summary>
        /// Starts worker making it execute do-work once upon its run.
        /// </summary>
        /// <param name="linkedTokens">
        /// A bunch of bound cancellation tokens. Cancellation request from one of these tokens will lead to stop of current worker. 
        /// </param>
        /// <param name="cancellationToken"> 
        /// Cancellation token that could be used to cancel starting of worker. 
        /// </param>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// Uses blocking-sync for concurrent call. Current method not awaits for task-worker execution.
        /// Awaiting here is applied only to synchronization of concurrent calls so cancellation request will terminate blocking-wait.
        /// </remarks>
        /// <returns> A state that describes start operation result. <see cref="StartResult.None"/> returns only when worker is already disposed. </returns>
        public async Task<StartResult> StartAsync(CancellationToken[] linkedTokens, CancellationToken cancellationToken = default)
        {
            if (linkedTokens.Length == 0)
            {
                throw new ArgumentException("Linked tokens cannot be empty.", nameof(linkedTokens));
            }

            if (IsDisposedBlocking)
            {
                return StartResult.None;
            }

            await _taskResourcesSemaphoreSlim.WaitAsync(cancellationToken);

            try
            {
                if (IsRunning)
                {
                    return StartResult.StopRequired;
                }

                var cts = CancellationTokenSource.CreateLinkedTokenSource(linkedTokens);

                CreateAndStartTask(cts);
            }
            finally
            {
                _taskResourcesSemaphoreSlim.Release();
            }

            return StartResult.Ok;
        }

        /// <summary>
        /// Sends cancellation signal that lead to stopping of worker and releasing of resources associeted with running task.
        /// </summary>
        /// <returns> A state that describes stop operation result. <see cref="StopResult.None"/> returns only when worker is already disposed. </returns>
        public StopResult Stop()
        {
            if (IsDisposedBlocking)
            {
                return StopResult.None;
            }

            _taskResourcesSemaphoreSlim.Wait();

            try
            {
                if (!IsRunning)
                {
                    return StopResult.StartRequired;
                }
            }
            finally 
            {
                _taskResourcesSemaphoreSlim.Release(); 
            }

            _cts.Cancel();

            return StopResult.Ok;
        }

        // Setup of task and its associated resources.
        private void CreateAndStartTask(CancellationTokenSource cts)
        {
            _cts = cts;

            var ct = _cts.Token;

            var func = new Action<CancellationToken>(async (token) => await ExecuteDoWorkAsync(token));

            _task = _taskFactory.StartNew(() => func(ct), ct, _taskCreationOptions, _taskFactory.Scheduler);
        }

        // A method that is passed to underlying task on its creation.
        private async Task ExecuteDoWorkAsync(CancellationToken cancellationToken)
        {
            OnStarted(TaskWorkerStartedEventArgs.Empty);

            try
            {
                using (var progress = TaskWorkerProgress<TProgress>.FromHandler(OnProgressChanged))
                {
                    await DoWorkAsync(progress, cancellationToken).ConfigureAwait(false);
                }

                CleanupTaskResources();

                OnStopped(TaskWorkerStoppedEventArgs.Finished);
            }
            catch (TaskCanceledException)
            {
                CleanupTaskResources();

                OnStopped(TaskWorkerStoppedEventArgs.Cancelled);
            }
            catch (Exception ex)
            {
                CleanupTaskResources();

                OnExceptionThrown(new TaskWorkerExceptionEventArgs(ex));

                OnStopped(TaskWorkerStoppedEventArgs.Exception);
            }
        }

        // Clean-up underlying cancellation source.
        private void CleanupCTS()
        {
            DebugEnter();

            if (_cts != null)
            {
                Debug($"Disposing {nameof(_cts)}.");

                _cts.Dispose();
                _cts = null;
            }

            DebugExit();
        }

        // Clean-up underlying task.
        private void CleanupTask()
        {
            DebugEnter();

            if (_task != null)
            {
                Debug($"{nameof(_task)} is not null.");

                if (Helpers.TaskHelper.IsCouldBeDisposed(_task))
                {
                    Debug($"Disposing {nameof(_task)}.");

                    _task.Dispose();
                }

                _task = null;
            }

            DebugExit();
        }

        // Organized clean-up of task associated resources.
        private void CleanupTaskResources()
        {
            DebugEnter();

            try
            {
                _taskResourcesSemaphoreSlim.Wait();

                CleanupCTS();
                CleanupTask();
            }
            finally
            {
                _taskResourcesSemaphoreSlim.Release();
            }

            DebugExit();
        }

        // Organized clean-up of exposed event handlers.
        private void CleanupEvents()
        {
            DebugEnter();

            Started = null;
            Stopped = null;
            ProgressChanged = null;
            ExceptionThrown = null;

            DebugExit();
        }

        // Organized clean-up of other underlying objects.
        private void CleanupInstance()
        {
            DebugEnter();

            _taskResourcesSemaphoreSlim.Dispose();

            DebugExit();
        }

        /// <summary>
        /// Releases unmanaged resources used by the <see cref="TaskWorker{TProgress}"/> and optionally releases the managed resources. 
        /// </summary>
        /// <param name="disposing"> <see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources. </param>
        protected virtual void Dispose(bool disposing)
        {
            string callerName = $"{nameof(Dispose)}({nameof(disposing)}={disposing})";
            
            DebugEnter(callerName);

            try
            {
                _disposedSemaphoreSlim.Wait();

                if (_disposed)
                {
                    Debug("Already disposed.");
                    return;
                }

                if (disposing)
                {
                    if (IsRunningBlocking)
                    {
                        Debug("Worker is running while disposing.");
                        Debug("Performing auto-stop.");
                        Stop();
                    }

                    CleanupEvents();
                    CleanupInstance();
                }

                _disposed = true;
            }
            finally
            {
                _disposedSemaphoreSlim.Release();
                _disposedSemaphoreSlim.Dispose();
            }

            DebugExit(callerName);
        }

        /// <summary>
        /// Releases all resources used by current instance of <see cref="TaskWorker{TProgress}"/>.
        /// </summary>
        /// <remarks>
        /// Recommended to dispose explicit. Also it is additionally requests for worker stopping if it is still running.
        /// </remarks>
        public void Dispose()
        {
            DebugEnter();

            Dispose(true);
            GC.SuppressFinalize(this);

            DebugExit();
        }

        [Conditional("DEBUG")]
        private static void Debug(string message, [CallerMemberName] string callerName = null)
        {
            System.Diagnostics.Debug.WriteLine($"{callerName} | {message}", $"{typeof(TaskWorker<>)}");
        }

        [Conditional("DEBUG")]
        private static void DebugEnter([CallerMemberName] string callerName = null)
        {
            Debug("Enter.", callerName);
        }

        [Conditional("DEBUG")]
        private static void DebugExit([CallerMemberName] string callerName = null)
        {
            Debug("Exit.", callerName);
        }
    }
}
