[<AutoOpen>]
module Archer.Bow.Values

open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.RunnerTypes

let run (runner: IRunner) =
    runner.Run ()
    
let runWithSeed (seed: unit -> int) (runner: IRunner) =
    runner.Run seed
    
let filterAndRun (filter: ITest list -> ITest list) (runner: IRunner) =
    runner.Run filter

let filterAndRunWith (seed: int) filter (runner: IRunner) =
    runner.Run (filter, (fun _ -> seed))

let bow = Bow ()
