module Archer.Tests.Scripts.``FrameworkExecutionEnded Event``

open Archer.Bow
open Archer
open Archer.CoreTypes.InternalTypes.FrameworkTypes
open Archer.MicroLang

let private defaultSeed = 33
let private getDefaultSeed () = defaultSeed

let private container = suite.Container ("", "FrameworkExecutionEnded Event should")

let ``Test Cases`` = [
    container.Test ("be raised when the framework is run", fun _ ->
        let framework = bow.Framework ()

        let mutable result = "Not Called" |> build.AsGeneralTestFailure
        
        framework.FrameworkLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | FrameworkEndExecution -> true
            | _ -> false
        )
        |> Event.add (fun _ ->
            result <- TestSuccess
        )
        
        framework.Run getDefaultSeed |> ignore
        
        result
    )
]