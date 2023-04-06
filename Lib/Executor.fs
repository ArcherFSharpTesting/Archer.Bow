namespace Archer.Bow

open Archer.CoreTypes
open Archer.CoreTypes.InternalTypes

type RunResults = {
    Failures: (Failure * ITest) list
    Successes: ITest list
    Seed: int
}

module Executor =
    let runTest (test: ITestExecutor) =
        let result = test.Execute ()
        result, test.Parent
        
    let buildReport failures successes seed =
        {
            Failures = failures |> List.sortBy (fun (_, test) -> test.TestFullName)
            Successes = successes |> List.sortBy (fun test -> test.TestFullName)
            Seed = seed
        }
    
        
    let runTests seed (tests: ITestExecutor seq) =
        let results = tests |> Seq.map runTest
        let successes =
            results
            |> Seq.filter (fst >> (=) TestSuccess)
            |> Seq.map snd
            |> List.ofSeq

        let failures =
            results
            |> Seq.filter (fst >> (=) TestSuccess >> not)
            |> Seq.map (fun (TestFailure f, test) -> f, test)
            |> List.ofSeq

        buildReport failures successes seed