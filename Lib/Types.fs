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
    let mutable test : ITest option = None
    
    member this.Run () =
        this.Run(fun () -> System.Random().Next ())
    member _.Run (getSeed: unit -> int) =
        {
            Failures = []
            Successes = 
                match test with
                | None -> []
                | Some test -> [test]
            Seed = getSeed ()
        }
    member _.AddTest (newTest: ITest) = test <- Some newTest
        
        
type Archer () =
    member _.Framework () = Framework ()