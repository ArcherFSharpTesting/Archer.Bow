[<AutoOpen>]
module Archer.Bow.Values

open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.FrameworkTypes

let run (framework: IFramework) =
    framework.Run ()
    
let runWithSeed (seed: unit -> int) (framework: IFramework) =
    framework.Run seed
    
let filterAndRun (predicate: ITest -> bool) (framework: IFramework) =
    framework.Run predicate

let filterAndRunWith (seed: int) predicate (framework: IFramework) =
    framework.Run (predicate, (fun _ -> seed))

let bow = Bow ()
