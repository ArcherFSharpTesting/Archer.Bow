namespace Archer.Bow.Lib

open System.Runtime.InteropServices
open Archer.CoreTypes.Lib
open Archer.CoreTypes.Lib.InternalTypes

type RunResults = {
    Failures: (TestResult * ITest) list
    Successes: ITest list
    Seed: int
}

type Framework () =
    let mutable tests = System.Collections.Generic.List<ITest>()
    
    member this.Run () =
        this.Run(fun () -> System.Random().Next ())
    member _.Run (getSeed: unit -> int) =
        let runTest (test: ITest) =
            let ex = test.GetExecutor ()
            let result = ex.Execute ()
            result, test

        let results = tests |> Seq.map (runTest)
        let successes =
            results
            |> Seq.filter (fun (r, _) -> r = TestSuccess)
            |> Seq.map (fun (_, t) -> t)
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
    member _.AddTest (newTest: ITest) = 
        tests.Add newTest
        
        
type Archer () =
    member _.Framework () = Framework ()