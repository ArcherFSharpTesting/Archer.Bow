module Archer.Tests.Scripts.``TestEnd Event should``

open Archer
open Archer.Core
open Archer.Core.Helpers
open Archer.Types.InternalTypes
open Archer.Types.InternalTypes.RunnerTypes
open Archer.MicroLang

let private feature = FeatureFactory.NewFeature (
    TestTags [
        Category "Runner"
        Category "RunnerLifecycleEvent"
    ]
)

let ``raise event with given test`` =
    feature.Test (fun () ->
        let runner, test = buildBasicRunner ()
        
        let mutable result = newFailure.With.TestExecutionWasNotRunValidationFailure () |> TestFailure
        
        runner.RunnerLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | RunnerTestLifeCycle(_, TestEnd _, _) -> true
            | _ -> false
        )
        |> Event.add (fun arg ->
            match arg with
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