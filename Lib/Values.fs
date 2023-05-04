[<AutoOpen>]
module Archer.Bow.Values

open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.RunnerTypes

let run (runner: IRunner) =
    runner.Run ()
    
let runWithSeed (seed: unit -> int) (runner: IRunner) =
    runner.Run seed
    
let filterAndRun (predicate: ITest -> bool) (runner: IRunner) =
    runner.Run predicate

let filterAndRunWith (seed: int) predicate (runner: IRunner) =
    runner.Run (predicate, (fun _ -> seed))

let bow = Bow ()
