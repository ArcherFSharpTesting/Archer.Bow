module Archer.Bow.Executor

open System
open System.Threading.Tasks
open Archer
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.RunnerTypes

let globalRandom = Random ()

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
    
let getRunInfo (test: ITestExecutor) =
    let assembly = System.Reflection.Assembly.GetExecutingAssembly ()
    {
        RunnerName = "Archer.Bow"
        RunnerVersion = assembly.GetName().Version
        TestInfo = test.Parent :> ITestInfo 
    }
    
let runTestSynchronous (test: ITestExecutor) = 
    let info = getRunInfo test
    
    try
        let result = test.Execute info
        (result, test.Parent)
    with
    | e -> (e |> TestExceptionFailure |> TestFailure |> TestExecutionResult, test.Parent)

let runTestParallel (test: ITestExecutor) =
    let info = getRunInfo test
    
    task {
        try
            let result = test.Execute info
            return (result, test.Parent)
        with
        | e -> return (e |> TestExceptionFailure |> TestFailure |> TestExecutionResult, test.Parent)
    }
    
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
    
let runTestsSerial seed (tests: ITestExecutor seq) =
    let runner (tests: ITestExecutor list) =
        tests |> List.map runTestSynchronous
        
    runTests runner seed tests
    
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