module Archer.Tests.Scripts.``TestStartTearDown Event should``

open Archer
open Archer.Arrows
open Archer.Arrows.Helpers
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.RunnerTypes
open Archer.MicroLang

let private feature = Arrow.NewFeature (
    TestTags [
        Category "Runner"
        Category "RunnerLifecycleEvent"
    ]
)

let ``be raised with the given test`` =
    feature.Test (fun _ ->
        let runner, test = buildBasicRunner ()
        
        let mutable result = newFailure.With.TestExecutionWasNotRunFailure () |> TestFailure
        
        runner.RunnerLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | RunnerTestLifeCycle (_, TestStartTeardown, _) -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | RunnerTestLifeCycle(currentTest, _, _) ->
                result <-
                    currentTest
                    |> Should.BeEqualTo test
            | _ -> ()
        )
        
        ()
        |> runner.Run
        |> ignore
        
        result
    )
    
let ``Test Cases`` = feature.GetTests ()