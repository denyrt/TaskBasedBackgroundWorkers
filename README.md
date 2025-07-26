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
| _Resource-Management_ | <input type="checkbox" checked> |
| _Thread-Safety_       | <input type="checkbox" checked> |
| _Life-Time Events_    | <input type="checkbox" checked> |
| _Progress Tracking_   | <input type="checkbox" checked> |
| _`RelayWorker`_       | <input type="checkbox"        > |
| _`WorkerScheldure`_   | <input type="checkbox"        > |
| _Fluent API_          | <input type="checkbox"        > |

### _Resources-Management_

Worker automatically setup, run and clean-up underlying `Task` and its associated resources.

### _Thread-Safety_

Worker is thread-safe in context of management its exection. 

The are main points that are synchronized:

- Starting
- Stopping
- Cancellation by external token

Each of listed scenarios uses concurrent synchronization so only one of them could be performed at one moment of time
that keeps underlying state always valid.

### _Life-Time Events and Progress Tracking_

Worker exposes next events:

- `Started`
- `Stopped`
- `ProgressChanged`
- `ExceptionThrown`
