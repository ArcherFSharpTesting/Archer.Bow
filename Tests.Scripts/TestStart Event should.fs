module Archer.Tests.Scripts.``TestStart Event should``

open Archer
open Archer.Arrows
open Archer.Arrows.Helpers
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.RunnerTypes
open Archer.MicroLang

let private container = Arrow.NewFeature (
    TestTags [
        Category "Runner"
        Category "RunnerLifecycleEvent"
    ]
)

let ``be raised with the given test when the runner is run`` =
    container.Test (fun _ ->
        let runner, test = buildBasicRunner ()
        
        let mutable result = newFailure.With.TestExecutionWasNotRunValidationFailure () |> TestFailure
        
        runner.RunnerLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | RunnerTestLifeCycle(_, TestStart _, _) -> true
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
    
let ``should not run the test action when canceled from test arg`` =
    container.Test (fun _ ->
        let expectedFailure = newFailure.With.TestExecutionWasNotRunValidationFailure () |> TestFailure
        let mutable result = expectedFailure 
        
        let testAction =
            (fun _ _ ->
                result <- expects.NotToBeCalled ()
                TestSuccess
            )
            
        let runner, _ = buildTestRunner testAction successfulUnitSetup successfulTeardown
        
        runner.RunnerLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | RunnerTestLifeCycle(_, TestStart _, _) -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | RunnerTestLifeCycle(_, TestStart cancelEventArgs, _) ->
                cancelEventArgs.Cancel <- true
            | _ -> ()
        )
        
        result
        |> Should.BeEqualTo expectedFailure
    )
    
let ``should not run the test action when canceled from runner arg`` =
    container.Test (fun _ ->
        let expectedFailure = newFailure.With.TestExecutionWasNotRunValidationFailure () |> TestFailure 
        let mutable result = expectedFailure
        
        let testAction =
            (fun _ _ ->
                result <- expects.NotToBeCalled ()
                TestSuccess
            )
            
        let runner, _ = buildTestRunner testAction successfulUnitSetup successfulTeardown
        
        runner.RunnerLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | RunnerTestLifeCycle(_, TestStart _, _) -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | RunnerTestLifeCycle(_, TestStart _, cancelEventArgs) ->
                cancelEventArgs.Cancel <- true
            | _ -> ()
        )
        
        result
        |> expects.ToBe expectedFailure
    )
    
let ``Test Cases`` = container.GetTests ()