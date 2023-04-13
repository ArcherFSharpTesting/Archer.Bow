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
            let result = test.Execute info
            return (result, test.Parent)
        }
        
    let buildReport (failures: (TestingFailure * ITest) list, ignored: (string option * CodeLocation * ITest) list, successes: ITest list, seed) =
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
                | TestSuccess -> true
                | _ -> false
            )
            |> List.map snd

        let failures =
            results
            |> List.filter (fun (result, _) ->
                match result with
                | TestFailure _ -> true
                | _ -> false
            )
            |> List.map (fun (TestFailure testingFailure, test) ->
                    testingFailure, test
                )
            |> List.ofSeq
            
        let ignored =
            results
            |> List.filter (fun (result, _) ->
                match result with
                | Ignored _ -> true
                | _ -> false
            )
            |> List.map (fun (Ignored (s, location), test) -> s, location, test)

        (failures, ignored, successes, seed)