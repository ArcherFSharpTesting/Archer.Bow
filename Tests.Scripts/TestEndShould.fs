module Archer.Tests.Scripts.``TestEnd Event should``

open Archer
open Archer.Arrows
open Archer.Arrows.Helpers
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.FrameworkTypes
open Archer.MicroLang

let private feature = Arrow.NewFeature ()

let ``raise event with given test`` =
    feature.Test (fun _ ->
        let framework, test = buildBasicFramework ()
        
        let mutable result = newFailure.With.TestExecutionWasNotRunValidationFailure () |> TestFailure
        
        framework.FrameworkLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | FrameworkTestLifeCycle(_, TestEnd _, _) -> true
            | _ -> false
        )
        |> Event.add (fun arg ->
            match arg with
            | FrameworkTestLifeCycle(currentTest, _, _) ->
                result <-
                    currentTest
                    |> Should.BeEqualTo test
            | _ -> ()
        )
        
        ()
        |> framework.Run
        |> ignore
        
        result
    )
    
let ``Test Cases`` = feature.GetTests ()