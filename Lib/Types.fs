namespace Archer.Runner

open System.ComponentModel
open Archer
open Archer.Runner.Executor
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.RunnerTypes

/// <summary>
/// The main test runner class responsible for executing tests and managing test lifecycle events.
/// Provides functionality to run tests in parallel or serial execution modes, filter tests,
/// and track execution results with detailed reporting.
/// </summary>
/// <param name="startingTests">The initial list of tests to be managed by this runner instance</param>
type Runner (startingTests: ITest list) as this =
    let mutable tests = startingTests
    let lockObj = obj()
    let mutable cancel = false
    let setCancel value =
        lock lockObj (fun () -> cancel <- value)
        cancel
        
    let readCancel () =
        lock lockObj (fun () -> cancel)
        
    let RunnerLifecycleEvent = Event<RunnerExecutionDelegate, RunnerEventLifecycle> ()
    let handleTestEvents (test: obj) (event: TestEventLifecycle) =
        match test with
        | :? ITest as tst ->
            match event with
            | TestStartExecution cancelEventArgs
            | TestStartSetup cancelEventArgs
            | TestEndSetup (_, cancelEventArgs)
            | TestStart cancelEventArgs as eventType ->
                cancelEventArgs.Cancel <- cancelEventArgs.Cancel || readCancel ()
                RunnerLifecycleEvent.Trigger (this, RunnerTestLifeCycle (tst, eventType, cancelEventArgs))
                setCancel cancelEventArgs.Cancel |> ignore
            | TestEnd _
            | TestStartTeardown
            | TestEndExecution _ as eventType ->
                let cancelEventArgs = readCancel () |> CancelEventArgs
                RunnerLifecycleEvent.Trigger (this, RunnerTestLifeCycle (tst, eventType, cancelEventArgs))
                
        | _ -> ()
        
    /// <summary>
    /// Creates a new Runner instance with an empty test collection.
    /// </summary>
    new () =
        let tests: ITest list = []
        Runner tests
        
    /// <summary>
    /// Runs all tests using a randomly generated seed for test ordering.
    /// </summary>
    /// <returns>A test execution report containing results, timing, and statistics</returns>
    member this.Run () =
        this.Run(fun () -> globalRandom.Next ())
        
    /// <summary>
    /// Runs all tests using a custom seed generation function for deterministic test ordering.
    /// </summary>
    /// <param name="getSeed">Function that generates a seed value for test ordering</param>
    /// <returns>A test execution report containing results, timing, and statistics</returns>
    member this.Run (getSeed: unit -> int) =
        this.Run (ifOnlyFilter, getSeed)
        
    /// <summary>
    /// Runs tests after applying a filter function, using a randomly generated seed for ordering.
    /// </summary>
    /// <param name="filter">Function to filter which tests should be executed</param>
    /// <returns>A test execution report containing results, timing, and statistics</returns>
    member this.Run (filter: ITest list -> ITest list) =
        this.Run (filter, fun () -> globalRandom.Next ())
    
    /// <summary>
    /// Runs tests after applying a filter function with a custom seed for deterministic ordering.
    /// This is the main execution method that handles the complete test lifecycle including
    /// parallel and serial execution modes, event handling, and result reporting.
    /// </summary>
    /// <param name="filter">Function to filter which tests should be executed</param>
    /// <param name="getSeed">Function that generates a seed value for test ordering</param>
    /// <returns>A test execution report containing results, timing, and statistics</returns>
    member this.Run (filter: ITest list -> ITest list, getSeed: unit -> int) =
        let seed = getSeed ()
        let startArgs = CancelEventArgs ()
        RunnerLifecycleEvent.Trigger (this, RunnerStartExecution startArgs)
        
        let hookEvents (executor: ITestExecutor) =
            executor.TestLifecycleEvent.AddHandler handleTestEvents
            executor
            
        let unhookEvents (executor: ITestExecutor) =
            executor.TestLifecycleEvent.RemoveHandler handleTestEvents

        let executors =
            tests
            |> filter
            |> List.map (fun t -> t.GetExecutor () |> hookEvents)

        try
            if startArgs.Cancel then
                let tm = System.DateTime.Now
                buildReport tm tm ([], [], [], seed)
            else
                let startTm = System.DateTime.Now
                let groups =
                    executors
                    |> List.groupBy
                        (fun exe ->
                            exe.Parent.Tags |> Seq.contains Serial
                        )
                let parallelGroup = groups |> List.filter (fst >> not) |> List.map snd |> List.concat |> shuffle seed
                let serialGroup = groups |> List.filter fst |> List.map snd |> List.concat |> shuffle seed
                let results =
                    let pFailures, pIgnored, pSuccesses, _seed = runTestsParallel seed parallelGroup
                    let sFailures, sIgnored, sSuccesses, _seed = runTestsSerial seed serialGroup
                    ([pFailures; sFailures] |> List.concat, [pIgnored; sIgnored] |> List.concat, [pSuccesses; sSuccesses] |> List.concat, seed)
                let endTm = System.DateTime.Now
                
                let report =
                    results
                    |> buildReport startTm endTm
                
                report
        finally
            executors |> List.iter unhookEvents
            RunnerLifecycleEvent.Trigger (this, RunnerEndExecution)
    
    /// <summary>
    /// Adds new tests to the runner's test collection. Validates that all test names are unique
    /// across the entire collection to prevent conflicts during execution.
    /// </summary>
    /// <param name="newTests">The sequence of tests to add to the runner</param>
    /// <returns>The current Runner instance for method chaining</returns>
    /// <exception cref="System.Exception">Thrown when duplicate test names are detected</exception>
    member this.AddTests (newTests: ITest seq) =
        tests <-
            [
                tests
                (newTests |> List.ofSeq)
            ]
            |> List.concat
            |> List.distinct
            
        let names =
            tests
            |> List.map (fun test -> test.ContainerPath, test.ContainerName, test.TestName)
            
        let errors =
            names
            |> List.countBy id
            |> List.filter (fun (_, cnt) -> 1 < cnt)
            |> List.map (fst >> (fun (path, container, test) -> $"%s{path}.%s{container}.%s{test}"))
            
        if 0 < errors.Length then
            failwith $"Each test name must be unique:\n%A{errors}"
        
        this
        
        
    interface IRunner with
        member this.AddTests newTests = this.AddTests newTests
        
        member this.Run () = this.Run ()
        
        member this.Run (getSeed: unit -> int) = this.Run getSeed
        member this.Run (filter: ITest list -> ITest list) = this.Run filter
        member this.Run (filter, getSeed) = this.Run (filter, getSeed)
        
        /// <summary>
        /// Event that fires during various stages of runner and test execution lifecycle.
        /// Subscribe to this event to monitor runner progress and handle test events.
        /// </summary>
        [<CLIEvent>]
        member _.RunnerLifecycleEvent = RunnerLifecycleEvent.Publish

        /// <summary>
        /// Gets all unique test tags from all tests in the runner's collection.
        /// Useful for filtering tests by tags or understanding test categorization.
        /// </summary>
        member _.TestTags =
            tests
            |> List.map (getTags >> Seq.toList)
            |> List.concat
            |> List.distinct
        
/// <summary>
/// Factory class for creating Runner instances. Provides a clean API entry point
/// for consumers of the Archer.Runner testing framework.
/// </summary>
type RunnerFactory () =
    /// <summary>
    /// Creates a new Runner instance that implements the IRunner interface.
    /// This is the primary method for obtaining a test runner.
    /// </summary>
    /// <returns>A new IRunner instance ready to accept tests and execute them</returns>
    member _.Runner () = Runner () :> IRunner