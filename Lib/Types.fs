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
    member this.Run () =
        this.Run(fun () -> System.Random().Next ())
    member _.Run (getSeed: unit -> int) =
        {
            Failures = []
            Successes = []
            Seed = getSeed ()
        }
        
type Archer () =
    member _.Framework () = Framework ()