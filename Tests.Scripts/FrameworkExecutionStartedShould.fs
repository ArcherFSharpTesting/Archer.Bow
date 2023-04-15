module Archer.Tests.Scripts.``FrameworkExecutionStarted Event should``

open Archer.Bow
open Archer
open Archer.CoreTypes.InternalTypes.FrameworkTypes
open Archer.MicroLang

let private defaultSeed = 33
let private getDefaultSeed () = defaultSeed

let private container = suite.Container ()

let ``be raised when framework is run`` =
    container.Test (fun _ ->
        let framework = bow.Framework ()

        let mutable result = "Not Run" |> newFailure.With.OtherTestExecutionFailure |> TestFailure
        
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
    
let ``Test Cases`` = container.Tests