using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers
{
    /// <summary>
    /// A base type for a thread-safe re-runnable worker that supports stop using its own stop method or via external <see cref="CancellationToken"/>.
    /// </summary>
    /// <remarks>
    /// A few notes:
    /// <list type="bullet">
    /// Worker could be re-start again if <see cref="IsRunning"/> is <see langword="false"/>.
    /// </list>
    /// <list type="bullet"> 
    /// All start and stop methods are concurrent synchronized in way that only a of these method could be executed at one moment of time.
    /// </list>
    /// <list type="bullet">
    /// Worker stopping using external <see cref="CancellationToken"/> shares the same synchronized behaivour with start and stop methods.
    /// So external call cancellationTokenSource.Cancel() will not be performed in one moment of time with start and stop methods.
    /// </list>
    /// </remarks>
    /// <typeparam name="TProgress"> Type that represents progress of worker do-work execution. </typeparam>
    public abstract class TaskWorker<TProgress> : IDisposable
    {
        private readonly TaskFactory            _taskFactory;
        private readonly TaskCreationOptions    _taskCreationOptions;
        private readonly SemaphoreSlim          _semaphoreSlim;

        private CancellationTokenSource         _cts;
        private Task                            _task;

        /// <summary>
        /// Indicates if worker is still running.
        /// </summary>
        public bool IsRunning => _task != null;

        /// <summary>
        /// Raises when worker is started.
        /// </summary>
        public event EventHandler<TaskWorkerStartedEventArgs> Started;

        /// <summary>
        /// Raises when worker is stopped.
        /// </summary>
        public event EventHandler<TaskWorkerStoppedEventArgs> Stopped;

        /// <summary>
        /// Raises when task progress is adjusted.
        /// </summary>
        public event EventHandler<TaskWorkerProgressChangedEventArgs<TProgress>> ProgressChanged;

        /// <summary>
        /// Raises when do-work method failed with unxpected exception.
        /// </summary>
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
            if (taskFactory.Scheduler == null)
            {
                throw new ArgumentException($"Cannot accept task factory without scheduler.", nameof(taskFactory.Scheduler));
            }

            _taskFactory = taskFactory;
            _taskCreationOptions = taskCreationOptions;
            _semaphoreSlim = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Work that is executed by worker on start.
        /// </summary>
        /// <param name="cancellationToken"> 
        /// Cancellation token provided by worker.
        /// </param>
        /// <returns> 
        /// A task that represents async operation. 
        /// </returns>
        protected abstract Task DoWorkAsync(CancellationToken cancellationToken);

        /// <summary> 
        /// Raises <see cref="Started"/> event.
        /// </summary>
        /// <param name="e"> Args within event. </param>
        /// <remarks>
        /// Called right before <see cref="DoWorkAsync(CancellationToken)"/>. Inner task is already running at moment of this call and <see cref="IsRunning"/> is <see langword="true"/>.
        /// </remarks>
        protected virtual void OnStarted(TaskWorkerStartedEventArgs e)
        {
            Started?.Invoke(this, e);
        }

        /// <summary> 
        /// Raises <see cref="Stopped"/> event. 
        /// </summary>
        /// <param name="e"> Args within event. </param>
        /// <remarks>
        /// Called when <see cref="DoWorkAsync(CancellationToken)"/> finished execution.
        /// </remarks>
        protected virtual void OnStopped(TaskWorkerStoppedEventArgs e)
        {
            Stopped?.Invoke(this, e);
        }

        /// <summary>
        /// Raises <see cref="ProgressChanged"/> event.
        /// </summary>
        /// <param name="e"> Value of progress. </param>
        protected virtual void OnProgressChanged(TaskWorkerProgressChangedEventArgs<TProgress> e)
        {
            ProgressChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raises <see cref="ExceptionThrown"/> event.
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// Called when <see cref="DoWorkAsync(CancellationToken)"/> fault with unhandled exception.
        /// </remarks>
        protected virtual void OnExceptionThrown(TaskWorkerExceptionEventArgs e)
        {
            ExceptionThrown?.Invoke(this, e);
        }

        public void Start(CancellationToken linkedToken = default)
        {
            var tokens = new CancellationToken[1] { linkedToken };

            try
            {
                Start(tokens);
            }
            catch (InvalidOperationException ex) 
            {
                throw ex;
            }
        }

        public void Start(IEnumerable<CancellationToken> linkedTokens)
        {
            var tokens = linkedTokens.ToArray();

            try
            {
                Start(tokens);
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Starts worker that executes <see cref="DoWorkAsync(CancellationToken)"/> once upon its run.
        /// </summary>
        /// <param name="linkedTokens"> 
        /// A bunch of bound cancellation tokens. Cancellation request from one of these tokens will lead to stop of current worker. 
        /// </param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Start(CancellationToken[] linkedTokens)
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("Stop worker firstly before calling for a start.");
            }

            if (linkedTokens.Length == 0)
            {
                throw new InvalidOperationException($"Array '{linkedTokens}' cannot be empty.");
            }
            
            _semaphoreSlim.Wait();

            try
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(linkedTokens);

                CreateAndStartTask(cts);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
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
        /// <exception cref="InvalidOperationException"></exception>
        /// <remarks>
        /// Current method not waits for task-worker execution. Awaiting here is applied only to synchronization of concurrent calls. 
        /// </remarks>
        public async Task StartAsync(CancellationToken[] linkedTokens, CancellationToken cancellationToken = default)
        {
            if (IsRunning)
            {
                // ToDo: Consider an alernative exception-safe approach with 'IsRunning' check?
                throw new InvalidOperationException("Stop worker firstly before calling for a start.");
            }

            if (linkedTokens.Length == 0)
            {
                throw new InvalidOperationException($"Array '{linkedTokens}' cannot be empty.");
            }

            await _semaphoreSlim.WaitAsync(cancellationToken);

            try
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(linkedTokens);

                CreateAndStartTask(cts);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        /// <summary>
        /// Sends cancellation signal that lead to stopping of worker and releasing of resources associeted with running task.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Stop()
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException("Cannot stop not running task.");
            }

            _semaphoreSlim.Wait();

            try
            {
                _cts.Cancel();
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        /// <summary>
        /// Sends cancellation signal that lead to stopping of worker and releasing of resources associeted with running task.
        /// </summary>
        /// <param name="ct"> Cancellation token. </param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <remarks>
        /// Current method not waits for task-worker execution. Awaiting here is applied only to synchronization of concurrent calls. 
        /// </remarks>
        public async Task StopAsync(CancellationToken ct = default)
        {
            if (!IsRunning)
            {
                // ToDo: Consider an alernative exception-safe approach with 'IsRunning' check?
                throw new InvalidOperationException("Cannot stop not running task.");
            }

            await _semaphoreSlim.WaitAsync(ct);

            _cts.Cancel();

            try
            {
                _cts.Cancel();
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private void CreateAndStartTask(CancellationTokenSource cts)
        {
            _cts = cts;

            var ct = _cts.Token;
            var func = new Action<CancellationToken>(async (token) => await ExecuteDoWorkAsync(token));

            _task = _taskFactory.StartNew(() => func(ct), ct, _taskCreationOptions, _taskFactory.Scheduler);
        }

        private async Task ExecuteDoWorkAsync(CancellationToken cancellationToken)
        {
            OnStarted(TaskWorkerStartedEventArgs.Empty);

            try
            {
                try
                {
                    await DoWorkAsync(cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    CleanupTaskResources();
                }
                
                OnStopped(TaskWorkerStoppedEventArgs.Finished);
            }
            catch (TaskCanceledException)
            {
                OnStopped(TaskWorkerStoppedEventArgs.Cancelled);
            }
            catch (Exception ex)
            {
                OnExceptionThrown(new TaskWorkerExceptionEventArgs(ex));
                OnStopped(TaskWorkerStoppedEventArgs.Exception);
            }
        }

        private void CleanupCTS()
        {
            if (_cts != null)
            {
                _cts.Dispose();
                _cts = null;
            }
        }

        private void CleanupTask()
        {
            if (_task != null)
            {
                if (Helpers.TaskHelper.IsCouldBeDisposed(_task))
                {
                    _task.Dispose();
                }

                _task = null;
            }
        }

        private void CleanupTaskResources()
        {
            CleanupCTS();
            CleanupTask();
        }

        private void CleanupEvents()
        {
            Started = null;
            Stopped = null;
            ProgressChanged = null;
            ExceptionThrown = null;
        }

        private void CleanupInstance()
        {
            _semaphoreSlim.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CleanupTaskResources();
                CleanupEvents();
                CleanupInstance();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
