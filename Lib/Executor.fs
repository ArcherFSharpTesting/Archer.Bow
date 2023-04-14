namespace Archer.Bow

open System
open Archer
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.FrameworkTypes

module Executor =
    let globalRandom = Random ()
    let runTest version (test: ITestExecutor) =
        let info = {
            FrameworkName = "Archer.Bow"
            FrameworkVersion = version
            TestInfo = test.Parent :> ITestInfo 
        }
        
        async {
            try
                let result = test.Execute info
                return (result, test.Parent)
            with
            | e -> return (e |> TestExceptionFailure |> TestFailure |> TestResult, test.Parent)
        }
        
    let buildReport (failures: (TestFailureType * ITest) list, ignored: (string option * CodeLocation * ITest) list, successes: ITest list, seed) =
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
    
    let runTests seed (tests: ITestExecutor seq) =
        let assembly = System.Reflection.Assembly.GetExecutingAssembly ()
        let version = assembly.GetName().Version
        
        let shuffled = shuffle seed tests
        let results =
            shuffled
            |> List.map (runTest version)
            |> Async.Parallel
            |> Async.RunSynchronously
            |> List.ofArray
            
        let successes =
            results
            |> List.filter (fun (result, _) ->
                match result with
                | TestResult TestSuccess -> true
                | _ -> false
            )
            |> List.map snd

        let failures =
            results
            |> List.filter (fun (result, _) ->
                match result with
                | TestResult TestSuccess
                | TestResult (TestIgnored _)  -> false
                | _ -> true
            )
            |> List.map (fun (testResult, test) ->
                let failure = 
                    match testResult with
                    | GeneralFailure generalTestingFailure -> GeneralFailureType generalTestingFailure
                    | SetupFailure setupTearDownFailure -> SetupFailureType setupTearDownFailure
                    | TestResult (TestFailure testFailure) -> TestRunFailureType testFailure
                    | TeardownFailure setupTearDownFailure -> TeardownFailureType setupTearDownFailure
                    
                failure, test
            )
            |> List.ofSeq
            
        let ignored =
            results
            |> List.filter (fun (result, _) ->
                match result with
                | TestResult (TestIgnored _) -> true
                | _ -> false
            )
            |> List.map (fun (TestResult (TestIgnored (s, location)), test) -> s, location, test)

        (failures, ignored, successes, seed)