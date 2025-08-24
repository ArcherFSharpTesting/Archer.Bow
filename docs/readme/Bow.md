# Bow Library Overview

Bow is the core test execution library for the Archer F# testing framework. It provides the main engine for running, filtering, and reporting on tests written using Archer. Bow is designed to be flexible, supporting both serial and parallel test execution, test filtering, and event-driven test lifecycle management.

## Key Features

- **Test Execution**: Supports running tests synchronously (serial) or asynchronously (parallel), with deterministic shuffling using a random seed.
- **Filtering**: Allows filtering tests by tags, categories, or custom logic before execution.
- **Lifecycle Events**: Emits events for key points in the test lifecycle (start, end, setup, teardown, etc.), enabling custom hooks and reporting.
- **Reporting**: Aggregates results into detailed reports, including successes, failures, ignored tests, and execution metadata.
- **Extensibility**: Exposes types and helpers for integrating with custom runners or test discovery mechanisms.

## Main Types & Modules

- `Bow`: Entry point for creating a test runner (`IRunner`).
- `Runner`: Manages test execution, filtering, and event handling.
- `Executor`: Contains core logic for running tests, shuffling, and building reports.
- `Values`: Provides helper functions for common test runner operations and filtering by category/tag.

## Example Usage

```fsharp
open Archer.Bow

let runner = Bow().Runner()
// Add tests to the runner (from Archer test definitions)
// runner.AddTests myTests
let report = runner.Run()
```

## Filtering Tests

You can filter tests by category or custom logic:

```fsharp
open Archer.Bow.Values

let onlyUnitTests = filterByCategory "Unit"
let report = filterAndRun onlyUnitTests runner
```

## Parallel and Serial Execution

Tests tagged with `Serial` are run serially; others are run in parallel for performance. Execution order is randomized by default but can be controlled with a seed.

## Events and Reporting

The runner emits events for test lifecycle stages, allowing integration with custom logging or reporting tools. The final report includes grouped results and execution metadata.

---

For more details, see the main README or source files in the `Lib/` directory.
