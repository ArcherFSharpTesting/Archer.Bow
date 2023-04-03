namespace Archer.Bow.Lib

open Archer.CoreTypes.Lib
open Archer.CoreTypes.Lib.InternalTypes

type RunResults = {
    Failures: (TestResult * ITest) list
    Successes: ITest list
    Seed: int
}

module Executor =
    let runTest (test: ITest) =
        let ex = test.GetExecutor ()
        let result = ex.Execute ()
        result, test
        
    let runTests getSeed (tests: ITest seq) =
        let results = tests |> Seq.map runTest
        let successes =
            results
            |> Seq.filter (fun (r, _) -> r = TestSuccess)
            |> Seq.map snd
            |> List.ofSeq

        let failures =
            results
            |> Seq.filter (fun (r, _) -> r |> (=) TestSuccess |> not)
            |> List.ofSeq

        {
            Failures = failures
            Successes = successes
            Seed = getSeed ()
        }