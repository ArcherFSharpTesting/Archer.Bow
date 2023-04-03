namespace Archer.Bow.Lib

open Archer.CoreTypes.Lib
open Archer.CoreTypes.Lib.InternalTypes

type RunResults = {
    Failures: (TestResult * ITest) list
    Successes: ITest list
    Seed: int
}

module Executor =
    let runTest (test: ITestExecutor) =
        let result = test.Execute ()
        result, test.Parent
        
    let runTests getSeed (tests: ITestExecutor seq) =
        let results = tests |> Seq.map runTest
        let successes =
            results
            |> Seq.filter (fst >> (=) TestSuccess)
            |> Seq.map snd
            |> List.ofSeq

        let failures =
            results
            |> Seq.filter (fst >> (=) TestSuccess >> not)
            |> List.ofSeq

        {
            Failures = failures |> List.sortBy (fun (_, test) -> test.TestFullName)
            Successes = successes |> List.sortBy (fun test -> test.TestFullName)
            Seed = getSeed ()
        }