# TaskBasedBackgroundWorkers

A bunch of simple tricks and code-base for implementing custom workers based on `System.Threading.Tasks.Task`. 
Feel free to use some code if you found it usefull.

## `TaskWorker<TProgress>`

A basic worker that is an object wrapper around `Task`, `TaskFactory` (with specified `TaskScheduler`) and `TaskCreationOptions`.

A few main purposes that this object could fulfill:

- Execute re-runnable work both long-running and short-running.
- Avoid a manual management of task and its associated resources.
- Expose life-time events to easy observe worker status.

### _Table of Features_

| Feature               | IsReady                         |
|-----------------------|---------------------------------|
| _Resource-Management_ | <ul><li>- [x] </li></ul>        |
| _Thread-Safety_       | <ul><li>- [x] </li></ul>        |
| _Life-Time Events_    | <ul><li>- [x] </li></ul>        |
| _Progress Tracking_   | <ul><li>- [x] </li></ul>        |
| _`ConcurrentHandle `_ | <ul><li>- [x] </li></ul>        |
| _`RelayWorker`_       | <ul><li>- [ ] </li></ul>        |
| _`WorkerScheldure`_   | <ul><li>- [ ] </li></ul>        |
| _Fluent API_          | <ul><li>- [ ] </li></ul>        |

### _Resources-Management_

Worker automatically setup, run and clean-up underlying `Task` and its associated resources.

### _Thread-Safety_

Worker is thread-safe in context of management its exection. 

The are main points that are synchronized:

- _Starting_
- _Stopping/Cancellation_
- _Disposing_

Each of listed scenarios uses concurrent synchronization so only one of them could be performed at one moment of time
that keeps underlying state always valid.

### _Life-Time Events and Progress Tracking_

Worker exposes next events:

- `Started`
- `Stopped`
- `ProgressChanged`
- `ExceptionThrown`

### _Progress Tracking_

Method `DoWorkAsync` provides `IProgress<T>` to report changes. `ProgressChanged` will be raised for each report.

#### Example

```
    public sealed class CustomWorker : TaskWorker<int>
    {
        public SingleActionWorker(TaskFactory taskFactory) : base(taskFactory)
        {
        }

        protected override async Task DoWorkAsync(IProgress<int> progress, CancellationToken cancellationToken)
        {
            // Triggers `ProgressChanged` event using `1000` as value for event args.
            progress.Report(1000); 
        }
    }
```

_A support for custom `IProgress<T>` not available but considered for later (low-prior task)_

### _ConcurrentHandle_

`ConcurrentHandle` is cool tool that makes code a bit cleaner when working with `SemaphoreSlim`.
It allows use `using` statement instead of `try/finally` that could be very enjoyable when working with a few semaphores.
There are both _sync_ and _async_ API available.

#### Example

```
    
    SemaphoreSlim       semaphore   = SomeObject.Semaphore;
    TimeSpan            timeout     = TimeSpan.FromSeconds(2); // optional
    CancellationToken   token       = CancellationToken.None; // optional

    using (var handle = ConcurrentHandle.EnterBlocking(semaphore, timeout, token))
    {
        // If IsEntered is true then current execution is synchronized with semaphore.
        // IsEntered could be false if timeout parameter specified.
        if (handle.IsEntered) 
        {
            // Do something 
        }
    }

    // Here underlying semaphore is realised if IsEntered is true
```
