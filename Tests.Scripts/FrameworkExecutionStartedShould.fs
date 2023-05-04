module Archer.Tests.Scripts.``FrameworkExecutionStarted Event should``

open Archer.Arrows
open Archer.Bow
open Archer
open Archer.CoreTypes.InternalTypes.RunnerTypes
open Archer.MicroLang

let private defaultSeed = 33
let private getDefaultSeed () = defaultSeed

let private feature = Arrow.NewFeature ()

let ``be raised when framework is run`` =
    feature.Test (fun _ ->
        let framework = bow.Framework ()

        let mutable result = "Not Run" |> newFailure.With.TestOtherExpectationFailure |> TestFailure
        
        framework.RunnerLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | RunnerStartExecution _ -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | RunnerStartExecution _ ->
                result <- TestSuccess
            | _ -> ()
        ) 
        
        framework.Run getDefaultSeed |> ignore

        result        
    )
    
let ``Test Cases`` = feature.GetTests ()