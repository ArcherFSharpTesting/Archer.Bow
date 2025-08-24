<!-- (dl
(section-meta
    (title Using the Runner)
)
) -->

<!-- (dl (# Creating a Runner)) -->

You can create a runner instance using the `Bow` type:

```fsharp
open Archer.Bow

let runner = Bow().Runner()
```

<!-- (dl (# Adding Tests)) -->

Add tests to the runner (typically discovered or defined elsewhere):

```fsharp
runner.AddTests myTests
```

<!-- (dl (# Running Tests)) -->

Run all tests (with default filtering and random seed):

```fsharp
let report = runner.Run()
```

Run tests with a custom random seed (for deterministic order):

```fsharp
let report = runner.Run(fun () -> 12345)
```

Run tests with a custom filter:

```fsharp
open Archer.Bow.Values

let onlyUnitTests = filterByCategory "Unit"
let report = runner.Run(onlyUnitTests)
```

Run tests with both a custom filter and seed:

```fsharp
let report = runner.Run(onlyUnitTests, fun () -> 12345)
```

<!-- (dl (# Subscribing to Events)) -->

You can subscribe to runner lifecycle events for custom logging or integration:

```fsharp
runner.RunnerLifecycleEvent.Add(fun (sender, event) ->
    match event with
    | RunnerStartExecution _ -> printfn "Starting tests!"
    | RunnerEndExecution -> printfn "All tests finished!"
    | RunnerTestLifeCycle (test, eventType, _) ->
        printfn $"Test {test.TestName} event: {eventType}"
)
```

<!-- (dl (# Interpreting the Report)) -->

The result of `runner.Run` is a report object containing grouped results, execution times, and metadata. You can use this for custom reporting or analysis.

---

For more details, see the Bow library documentation or the `Types.fs` source file.
