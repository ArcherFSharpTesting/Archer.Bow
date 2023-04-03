namespace Archer.Bow.Lib

open Archer.CoreTypes.Lib.InternalTypes
open Archer.Bow.Lib.Executor

type Framework () =
    let mutable tests = System.Collections.Generic.List<ITest>()
    
    member this.Run () =
        this.Run(fun () -> System.Random().Next ())
        
    member _.Run (getSeed: unit -> int) =
        runTests getSeed tests
        
    member _.AddTest (newTest: ITest) = 
        tests.Add newTest
        
type Archer () =
    member _.Framework () = Framework ()