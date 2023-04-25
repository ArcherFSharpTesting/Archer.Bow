module Archer.Tests.Scripts.``TestStartTearDown Event should``

open Archer
open Archer.Arrows.Helpers
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.FrameworkTypes
open Archer.MicroLang

let private feature = Arrow.NewFeature ()

let ``be raised with the given test`` =
    feature.Test (fun _ ->
        let framework, test = buildBasicFramework ()
        
        let mutable result = newFailure.With.TestExecutionWasNotRunFailure () |> TestFailure
        
        framework.FrameworkLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | FrameworkTestLifeCycle (_, TestStartTeardown, _) -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
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