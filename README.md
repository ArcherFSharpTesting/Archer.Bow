<!-- GENERATED DOCUMENT DO NOT EDIT! -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->

<!-- Compiled with doculisp https://www.npmjs.com/package/doculisp -->

# Bow Execution Library for the Archer Test Framework #

1. Overview: [Bow Library Overview](#bow-library-overview)
2. Feature: [Using the Runner](#using-the-runner)
3. Feature: [Runner Lifetime Events](#runner-lifetime-events)
4. Feature: [Making Test Running Easier](#making-test-running-easier)

## Bow Library Overview ##

Bow is the core test execution library for the Archer F# testing framework. It provides the main engine for running, filtering, and reporting on tests written using Archer. Bow is designed to be flexible, supporting both serial and parallel test execution, test filtering, and event-driven test lifecycle management.

### Key Features ###

- **Test Execution**: Supports running tests synchronously (serial) or asynchronously (parallel), with deterministic shuffling using a random seed.
- **Filtering**: Allows filtering tests by tags, categories, or custom logic before execution.
- **Lifecycle Events**: Emits events for key points in the test lifecycle (start, end, setup, teardown, etc.), enabling custom hooks and reporting.
- **Reporting**: Aggregates results into detailed reports, including successes, failures, ignored tests, and execution metadata.
- **Extensibility**: Exposes types and helpers for integrating with custom runners or test discovery mechanisms.

### Main Types & Modules ###

- `Bow`: Entry point for creating a test runner (`IRunner`).
- `Runner`: Manages test execution, filtering, and event handling.
- `Executor`: Contains core logic for running tests, shuffling, and building reports.
- `Values`: Provides helper functions for common test runner operations and filtering by category/tag.

### Example Usage ###

```fsharp
open Archer.Bow

let runner = Bow().Runner()
// Add tests to the runner (from Archer test definitions)
// runner.AddTests myTests
let report = runner.Run()
```

### Filtering Tests ###

You can filter tests by category or custom logic:

```fsharp
open Archer.Bow.Values

let onlyUnitTests = filterByCategory "Unit"
let report = filterAndRun onlyUnitTests runner
```

### Parallel and Serial Execution ###

Tests tagged with `Serial` are run serially; others are run in parallel for performance. Execution order is randomized by default but can be controlled with a seed.

### Events and Reporting ###

The runner emits events for test lifecycle stages, allowing integration with custom logging or reporting tools. The final report includes grouped results and execution metadata.

---

For more details, see the main README or source files in the `Lib/` directory.

## Using the Runner ##

The `Runner` in the Bow library is responsible for executing tests, handling test lifecycle events, and providing flexible filtering and execution options. Below are usage patterns and examples to help you get started.

### Creating a Runner ###

You can create a runner instance using the `Bow` type:

```fsharp
open Archer.Bow

let runner = Bow().Runner()
```

### Adding Tests ###

Add tests to the runner (typically discovered or defined elsewhere):

```fsharp
runner.AddTests myTests
```

### Running Tests ###

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

### Subscribing to Events ###

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

### Interpreting the Report ###

The result of `runner.Run` is a report object containing grouped results, execution times, and metadata. You can use this for custom reporting or analysis.

---

For more details, see the Bow library documentation or the `Types.fs` source file.

## Runner Lifetime Events ##

The `Runner` in the Bow library emits several lifecycle events that allow you to hook into and respond to different stages of test execution. These events are useful for custom logging, reporting, or integrating with other systems.

### Runner Lifecycle Events ###

- **RunnerStartExecution**
  - Triggered before any tests are executed.
  - Carries a `CancelEventArgs` that can be set to cancel the entire test run.

- **RunnerEndExecution**
  - Triggered after all tests have finished executing (regardless of success, failure, or cancellation).

- **RunnerTestLifeCycle**
  - Triggered for each test at various points in its lifecycle.
  - Provides the test instance, the event type, and a `CancelEventArgs` (for cancellable events).

### Test Lifecycle Event Types ###

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

### Example: Subscribing to Runner Events ###

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

## Making Test Running Easier ##

The Bow library provides a set of helper functions to simplify running tests with the runner. These helpers make it easier to filter, seed, and execute tests in a variety of ways, and are available by default.

### Helper Functions Overview ###

Below are the main helpers provided:

- **run**: Runs all tests using the default runner configuration.
- **runWithSeed**: Runs all tests with a custom random seed for deterministic execution order.
- **filterAndRun**: Runs tests after applying a custom filter function.
- **filterAndRunWith**: Runs tests after applying a filter and using a specific seed.
- **filterByCategories**: Returns a filter function that selects tests by a list of category names.
- **filterByCategory**: Returns a filter function for a single category.
- **bow**: Provides a default instance of the Bow runner.

### Usage Examples ###

Here are some usage examples:

```fsharp
// Run all tests
defaultRunner |> run

// Run all tests with a specific seed
defaultRunner |> runWithSeed (fun () -> 42)

// Run only tests in the "Unit" category
defaultRunner |> filterAndRun (filterByCategory "Unit")

// Run only tests in "Unit" or "Integration" categories with a specific seed
defaultRunner |> filterAndRunWith 123 (filterByCategories ["Unit"; "Integration"])
```

---

For more details, see the `Values.fs` source file or the Bow library documentation.

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->
<!-- GENERATED DOCUMENT DO NOT EDIT! -->