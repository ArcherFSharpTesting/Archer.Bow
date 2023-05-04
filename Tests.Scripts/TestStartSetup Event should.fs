module Archer.Tests.Scripts.``TestStartSetup Event should``

open Archer.Arrows
open Archer
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.RunnerTypes
open Archer.MicroLang

let private defaultSeed = 33
let private getDefaultSeed () = defaultSeed

let private feature = Arrow.NewFeature ()

let ``be raised from the given test when the runner is run`` =
    feature.Test (fun _ ->
        let runner, test = buildTestRunner successfulEnvironmentTest successfulUnitSetup successfulTeardown

        let mutable result = "Not Called" |> newFailure.With.TestOtherExpectationFailure |> TestFailure
        
        runner.RunnerLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | RunnerTestLifeCycle (_, TestStartSetup _, _) -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | RunnerTestLifeCycle (currentTest, _, _) ->
                result <-
                    currentTest
                    |> expects.ToBe test
            | _ -> ()
        )

        getDefaultSeed
        |> runner.Run
        |> ignore

        result
    )
    
let ``not be raised if RunnerExecutionStarted was canceled`` =
    feature.Test (fun _ ->
        let runner, _ = buildTestRunner successfulEnvironmentTest successfulUnitSetup successfulTeardown
         
        let mutable result = TestSuccess
        
        runner.RunnerLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | RunnerStartExecution _
            | RunnerTestLifeCycle(_, TestStartExecution _, _) -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | RunnerStartExecution cancelEventArgs ->
                cancelEventArgs.Cancel <- true
            | RunnerTestLifeCycle _ ->
                result <- expects.NotToBeCalled ()
            | _ -> ()
        )
        
        runner.Run () |> ignore
        
        result
    )
    
let ``Test Cases`` = feature.GetTests ()