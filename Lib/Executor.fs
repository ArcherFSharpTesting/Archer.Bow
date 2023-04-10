namespace Archer.Bow

open System
open Archer
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.FrameworkTypes

module Executor =
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
        
    let buildReport failures successes seed =
        {
            Failures = failures |> List.sortBy (fun (_, test) -> test.ContainerPath, test.ContainerName, test.TestName)
            Successes = successes |> List.sortBy (fun test -> test.ContainerPath, test.ContainerName, test.TestName)
            Seed = seed
        }
        
    let shuffle seed (items: 'a seq) =
        let random = System.Random seed
        let bucket = System.Collections.Generic.List<'a> items
        
        let rec shuffle acc =
            if bucket.Count <= 0 then
                acc
            else
                let index = random.Next (0, bucket.Count)
                let item = bucket.[index]
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
            |> List.filter (fst >> (=) TestSuccess)
            |> List.map snd

        let failures =
            results
            |> List.filter (fst >> (=) TestSuccess >> not)
            |> List.map (fun (TestFailure f, test) -> f, test)
            |> List.ofSeq

        buildReport failures successes seed