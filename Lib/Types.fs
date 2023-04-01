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
        let failures =
            if tests.Count < 1 then []
            else
                let test = tests |> Seq.head
                let ex = test.GetExecutor ()
                let result = ex.Execute ()

                match result with
                | TestSuccess -> []
                | failure -> [failure, test]

        let successes =
            if (failures |> List.length) < 1 then
                tests |> List.ofSeq
            else
                tests |> Seq.skip 1 |> List.ofSeq

        {
            Failures = failures
            Successes = successes
            Seed = getSeed ()
        }
    member _.AddTest (newTest: ITest) = 
        tests.Add newTest
        
        
type Archer () =
    member _.Framework () = Framework ()