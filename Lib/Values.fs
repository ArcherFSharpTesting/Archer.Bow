[<AutoOpen>]
module Archer.Bow.Values

open Archer
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
    
let filterByCategories (categories: string list) =
    let filter (tests: ITest list) =
        tests
        |> List.filter (fun test ->
            test.Tags
            |> List.ofSeq
            |> List.map
                (fun tag ->
                    match tag with
                    | Category s ->
                        categories |> List.contains s
                    | _ -> false
                )
            |> List.reduce (||)
        )
        
    filter
    
let filterByCategory category =
    filterByCategories [category]

let bow = Bow ()
