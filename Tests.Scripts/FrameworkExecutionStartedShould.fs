module Archer.Tests.Scripts.``FrameworkExecutionStarted Event``

open Archer.Bow
open Archer
open Archer.CoreTypes.InternalTypes.FrameworkTypes
open Archer.MicroLang

let private defaultSeed = 33
let private getDefaultSeed () = defaultSeed

let private container = suite.Container ("", "FrameworkExecutionStarted Event should")

let ``Test Cases`` = [
    container.Test ("be raised when framework is run", fun _ ->
        let framework = bow.Framework ()

        let mutable result = "Not Run" |> build.AsGeneralTestFailure
        
        framework.FrameworkLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | FrameworkStartExecution _ -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | FrameworkStartExecution _ ->
                result <- TestSuccess
            | _ -> ()
        ) 
        
        framework.Run getDefaultSeed |> ignore

        result        
    )
]