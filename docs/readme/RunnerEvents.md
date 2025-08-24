<!-- (dl
(section-meta
    (title Runner Lifetime Events)
)
) -->

The `Runner` in the Bow library emits several lifecycle events that allow you to hook into and respond to different stages of test execution. These events are useful for custom logging, reporting, or integrating with other systems.


<!-- (dl (# Runner Lifecycle Events)) -->

- **RunnerStartExecution**
  - Triggered before any tests are executed.
  - Carries a `CancelEventArgs` that can be set to cancel the entire test run.

- **RunnerEndExecution**
  - Triggered after all tests have finished executing (regardless of success, failure, or cancellation).

- **RunnerTestLifeCycle**
  - Triggered for each test at various points in its lifecycle.
  - Provides the test instance, the event type, and a `CancelEventArgs` (for cancellable events).


<!-- (dl (# Test Lifecycle Event Types)) -->

- **TestStartExecution**
  - Fired before the test's execution phase begins. Can be cancelled.
- **TestStartSetup**
  - Fired before the test's setup phase. Can be cancelled.
- **TestEndSetup**
  - Fired after the test's setup phase. Can be cancelled.
- **TestStart**
  - Fired before the test body runs. Can be cancelled.
- **TestEnd**
  - Fired after the test body completes.
- **TestStartTeardown**
  - Fired before the test's teardown phase.
- **TestEndExecution**
  - Fired after the test's execution phase completes.


<!-- (dl (# Example: Subscribing to Runner Events)) -->


```fsharp
let runner = Bow().Runner()

runner.RunnerLifecycleEvent.Add(fun (sender, event) ->
  match event with
  | RunnerTestLifeCycle (test, eventType, _) ->
    match eventType with
    | TestEnd ->
      printfn $"Test '{test.TestName}' finished."
    | _ -> ()
  | RunnerStartExecution _ ->
    printfn "Test run starting!"
  | RunnerEndExecution ->
    printfn "Test run finished!"
)
```

---

For more details, see the `Types.fs` source or the Bow library documentation.
