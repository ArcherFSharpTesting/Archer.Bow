/// <summary>
/// Internal module containing the core test execution engine for Archer.Bow.
/// Provides functions for running tests synchronously and asynchronously, managing test ordering,
/// building execution reports, and handling test lifecycle management.
/// This module contains implementation details that support the public Runner API.
/// </summary>
module Archer.Bow.Executor

open System
open System.Threading.Tasks
open Archer
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.RunnerTypes

/// <summary>
/// Global random number generator used for test shuffling and seed generation.
/// Provides a shared source of randomness across the execution engine.
/// </summary>
let globalRandom = Random ()

/// <summary>
/// Filters tests to run only those marked with the "Only" tag, if any exist.
/// If no tests have the "Only" tag, returns all tests unchanged.
/// This is useful for focused testing during development.
/// </summary>
/// <param name="tests">The list of tests to potentially filter</param>
/// <returns>Either the subset of tests marked "Only" or all tests if none are marked</returns>
let ifOnlyFilter (tests: ITest list) =
    let containsOnly (tags: TestTag seq) =
        let onlyTags =
            tags
            |> Seq.filter (fun t -> t = Only)
            
        0 < (onlyTags |> Seq.length)
        
    let possible =
        tests
        |> List.filter (getTags >> containsOnly)
        
    if 0 < possible.Length then possible
    else tests
    
/// <summary>
/// Creates run information metadata for a test execution.
/// Includes runner identification, version, and test context information.
/// </summary>
/// <param name="test">The test executor for which to create run information</param>
/// <returns>RunInfo record containing runner metadata and test information</returns>
let getRunInfo (test: ITestExecutor) =
    let assembly = System.Reflection.Assembly.GetExecutingAssembly ()
    {
        RunnerName = "Archer.Bow"
        RunnerVersion = assembly.GetName().Version
        TestInfo = test.Parent :> ITestInfo 
    }
    
/// <summary>
/// Executes a single test synchronously in the current thread.
/// Handles exceptions by converting them to test failure results.
/// Used for serial test execution where tests must run one after another.
/// </summary>
/// <param name="test">The test executor to run</param>
/// <returns>Tuple of (execution result, test instance)</returns>
let runTestSynchronous (test: ITestExecutor) = 
    let info = getRunInfo test
    
    try
        let result = test.Execute info
        (result, test.Parent)
    with
    | e -> (e |> TestExceptionFailure |> TestFailure |> TestExecutionResult, test.Parent)

/// <summary>
/// Executes a single test asynchronously using F# tasks for parallel execution.
/// Handles exceptions by converting them to test failure results.
/// Used for parallel test execution where multiple tests can run concurrently.
/// </summary>
/// <param name="test">The test executor to run</param>
/// <returns>Task containing a tuple of (execution result, test instance)</returns>
let runTestParallel (test: ITestExecutor) =
    let info = getRunInfo test
    
    task {
        try
            let result = test.Execute info
            return (result, test.Parent)
        with
        | e -> return (e |> TestExceptionFailure |> TestFailure |> TestExecutionResult, test.Parent)
    }
    
/// <summary>
/// Builds a comprehensive test execution report from categorized test results.
/// Organizes results by container path and name, creating a hierarchical structure
/// that groups related tests together for clear reporting.
/// </summary>
/// <param name="startTm">The timestamp when test execution began</param>
/// <param name="endTm">The timestamp when test execution completed</param>
/// <param name="failures">List of failed tests with their failure types</param>
/// <param name="ignored">List of ignored tests with optional reasons and locations</param>
/// <param name="successes">List of successfully executed tests</param>
/// <param name="seed">The seed value used for test ordering</param>
/// <returns>A structured test report with timing, results, and execution metadata</returns>
let buildReport startTm endTm (failures: (TestFailureType * ITest) list, ignored: (string option * CodeLocation * ITest) list, successes: ITest list, seed) =
    let failures =
        failures
        |> List.groupBy (fun (_, test) -> test.ContainerPath)
        |> List.map (fun (path, results) ->
            let reports =
                results
                |> List.groupBy (fun (_, test) -> test.ContainerName)
                |> List.map (fun (container, tests) ->
                    FailContainer (container, [FailedTests tests])
                )
                
            if path |> String.IsNullOrEmpty then reports
            else [FailContainer (path, reports)]
        )
        |> List.concat
        
    let successes =
        successes
        |> List.groupBy (fun t -> t.ContainerPath)
        |> List.map (fun (path, results) ->
                let reports =
                    results
                    |> List.groupBy (fun test -> test.ContainerName)
                    |> List.map (fun (container, tests) ->
                        SuccessContainer (container, [SucceededTests tests])
                    )
                    
                if path |> String.IsNullOrEmpty then reports
                else [SuccessContainer (path, reports)]
        )
        |> List.concat
        
    let ignored =
        ignored
        |> List.groupBy (fun (_, _, test) -> test.ContainerPath)
        |> List.map(fun (path, results) ->
            let reports =
                results
                |> List.groupBy (fun (_, _, test) -> test.ContainerName)
                |> List.map (fun (container, tests) ->
                    IgnoreContainer (container, [IgnoredTests tests]) 
                )
                
            if path |> String.IsNullOrEmpty then reports
            else [IgnoreContainer (path, reports)]
        )
        |> List.concat
        
    {
        Failures = failures
        Successes = successes
        Ignored = ignored
        Seed = seed
        Began = startTm
        End = endTm
    }
    
/// <summary>
/// Shuffles a sequence of items using the Fisher-Yates algorithm with a specific seed.
/// Provides deterministic randomization - the same seed will always produce the same order.
/// Used to randomize test execution order while maintaining reproducibility.
/// </summary>
/// <param name="seed">The seed value for the random number generator</param>
/// <param name="items">The sequence of items to shuffle</param>
/// <returns>A new list with items in randomized order</returns>
let shuffle seed (items: 'a seq) =
    let random = Random seed
    let bucket = System.Collections.Generic.List<'a> items
    
    let rec shuffle acc =
        if bucket.Count <= 0 then
            acc
        else
            let index = random.Next (0, bucket.Count)
            let item = bucket[index]
            bucket.RemoveAt index
            
            item::acc
            |> shuffle
            
    shuffle []
    
/// <summary>
/// Core test execution function that runs tests using a provided execution strategy.
/// Handles shuffling, result categorization, and statistics collection.
/// This is the foundation for both serial and parallel test execution.
/// </summary>
/// <param name="runner">Function that executes a list of test executors and returns results</param>
/// <param name="seed">Seed for deterministic test ordering</param>
/// <param name="tests">Sequence of test executors to run</param>
/// <returns>Tuple of (failures, ignored tests, successes, seed used)</returns>
let runTests (runner: ITestExecutor list -> (TestExecutionResult * ITest) list) seed (tests: ITestExecutor seq) =
    let shuffled = shuffle seed tests
    let results =
        shuffled
        |> runner
        
    let successes =
        results
        |> List.filter (fun (result, _) ->
            match result with
            | TestExecutionResult TestSuccess -> true
            | _ -> false
        )
        |> List.map snd

    let failures =
        results
        |> List.filter (fun (result, _) ->
            match result with
            | TestExecutionResult TestSuccess
            | TestExecutionResult (TestFailure (TestIgnored _))  -> false
            | _ -> true
        )
        |> List.map (fun (testResult, test) ->
            let failure = 
                match testResult with
                | GeneralExecutionFailure generalTestingFailure -> GeneralFailureType generalTestingFailure
                | SetupExecutionFailure setupTearDownFailure -> SetupFailureType setupTearDownFailure
                | TestExecutionResult (TestFailure testFailure) -> TestRunFailureType testFailure
                | TeardownExecutionFailure setupTearDownFailure -> TeardownFailureType setupTearDownFailure
                
            failure, test
        )
        |> List.ofSeq
        
    let ignored =
        results
        |> List.filter (fun (result, _) ->
            match result with
            | TestExecutionResult (TestFailure (TestIgnored _)) -> true
            | _ -> false
        )
        |> List.map (fun (TestExecutionResult (TestFailure (TestIgnored (s, location))), test) -> s, location, test)

    (failures, ignored, successes, seed)
    
/// <summary>
/// Executes tests serially (one after another) in a single thread.
/// Uses synchronous execution to ensure tests run sequentially without concurrency.
/// Recommended for tests that are not thread-safe or need to run in isolation.
/// </summary>
/// <param name="seed">Seed for deterministic test ordering</param>
/// <param name="tests">Sequence of test executors to run serially</param>
/// <returns>Tuple of (failures, ignored tests, successes, seed used)</returns>
let runTestsSerial seed (tests: ITestExecutor seq) =
    let runner (tests: ITestExecutor list) =
        tests |> List.map runTestSynchronous
        
    runTests runner seed tests
    
/// <summary>
/// Executes tests in parallel using F# tasks for concurrent execution.
/// Uses asynchronous execution to run multiple tests simultaneously, improving performance.
/// Recommended for independent tests that can safely run concurrently.
/// </summary>
/// <param name="seed">Seed for deterministic test ordering</param>
/// <param name="tests">Sequence of test executors to run in parallel</param>
/// <returns>Tuple of (failures, ignored tests, successes, seed used)</returns>
let runTestsParallel seed (tests: ITestExecutor seq) =
    let runner (tests: ITestExecutor list) =
        let results = 
            tests
            |> Array.ofList
            |> Array.map runTestParallel
            |> Task.WhenAll
            
        results.Result
        |> List.ofArray
            
    runTests runner seed tests