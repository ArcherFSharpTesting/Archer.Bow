/// <summary>
/// Auto-opened module containing convenience functions and values for working with the Archer.Runner test runner.
/// These functions provide a functional programming style API that simplifies common test execution scenarios.
/// All functions in this module are automatically available without explicit module qualification.
/// </summary>
[<AutoOpen>]
module Archer.Runner.Values

open Archer
open Archer.Types.InternalTypes
open Archer.Types.InternalTypes.RunnerTypes

/// <summary>
/// Executes all tests in the runner using a randomly generated seed for test ordering.
/// This is the simplest way to run tests when you don't need control over execution order or filtering.
/// </summary>
/// <param name="runner">The test runner instance containing the tests to execute</param>
/// <returns>A test execution report with results, timing, and statistics</returns>
let run (runner: IRunner) =
    runner.Run ()
    
/// <summary>
/// Executes all tests in the runner using a custom seed generation function for deterministic test ordering.
/// Use this when you need reproducible test runs or want to control the randomization seed.
/// </summary>
/// <param name="seed">Function that generates the seed value for test ordering</param>
/// <param name="runner">The test runner instance containing the tests to execute</param>
/// <returns>A test execution report with results, timing, and statistics</returns>
let runWithSeed (seed: unit -> int) (runner: IRunner) =
    runner.Run seed
    
/// <summary>
/// Executes tests after applying a filter function, using a randomly generated seed for test ordering.
/// This allows you to run only a subset of tests based on custom criteria while maintaining randomized execution order.
/// </summary>
/// <param name="filter">Function that takes the full test list and returns the filtered tests to execute</param>
/// <param name="runner">The test runner instance containing the tests to filter and execute</param>
/// <returns>A test execution report with results, timing, and statistics for the filtered tests</returns>
let filterAndRun (filter: ITest list -> ITest list) (runner: IRunner) =
    runner.Run filter

/// <summary>
/// Executes tests after applying a filter function with a specific seed for deterministic, reproducible test runs.
/// This provides maximum control over both which tests run and their execution order.
/// </summary>
/// <param name="seed">The specific seed value to use for test ordering (ensures reproducible runs)</param>
/// <param name="filter">Function that takes the full test list and returns the filtered tests to execute</param>
/// <param name="runner">The test runner instance containing the tests to filter and execute</param>
/// <returns>A test execution report with results, timing, and statistics for the filtered tests</returns>
let filterAndRunWith (seed: int) filter (runner: IRunner) =
    runner.Run (filter, (fun _ -> seed))
    
/// <summary>
/// Creates a filter function that selects tests belonging to any of the specified categories.
/// Tests are included if they have at least one Category tag that matches any of the provided category names.
/// Use this to run tests from multiple related categories in a single execution.
/// </summary>
/// <param name="categories">List of category names to include in the filter</param>
/// <returns>A filter function that can be used with filterAndRun or filterAndRunWith</returns>
/// <example>
/// <code>
/// let uiAndIntegrationTests = filterByCategories ["UI"; "Integration"]
/// runner |> filterAndRun uiAndIntegrationTests
/// </code>
/// </example>
let filterByCategories (categories: string list) =
    let filter (tests: ITest list) =
        tests
        |> List.filter (fun test ->
            test.Tags
            |> List.ofSeq
            |> List.map
                (fun tag ->
                    match tag with
                    | Category s ->
                        categories |> List.contains s
                    | _ -> false
                )
            |> List.reduce (||)
        )
        
    filter
    
/// <summary>
/// Creates a filter function that selects tests belonging to a specific category.
/// This is a convenience function equivalent to calling filterByCategories with a single-item list.
/// Tests are included if they have a Category tag matching the provided category name.
/// </summary>
/// <param name="category">The category name to filter by</param>
/// <returns>A filter function that can be used with filterAndRun or filterAndRunWith</returns>
/// <example>
/// <code>
/// let unitTests = filterByCategory "Unit"
/// runner |> filterAndRun unitTests
/// </code>
/// </example>
let filterByCategory category =
    filterByCategories [category]

/// <summary>
/// A global instance of the RunnerFactory class, ready to create test runners.
/// This provides convenient access to runner creation without needing to instantiate RunnerFactory() manually.
/// Use this when you need a quick way to create runners in scripts or simple test scenarios.
/// </summary>
/// <example>
/// <code>
/// let runner = runnerFactory.Runner()
/// runner.AddTests [test1; test2; test3] |> ignore
/// runner |> run
/// </code>
/// </example>
let runnerFactory = RunnerFactory ()
