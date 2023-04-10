[<AutoOpen>]
module Archer.Bow.Values

open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.FrameworkTypes

let addTests (tests: ITest seq) (framework: IFramework) =
    framework.AddTests tests
    
let run (framework: IFramework) =
    framework.Run ()
    
let runWithSeed seed (framework:IFramework) =
    framework.Run seed

let bow = Bow ()
