module Archer.Tests.Scripts.``TestEndSetup Event should``

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

let ``be raised from the given test when the runner is run`` =
    feature.Test (fun () ->
        let runner, test = buildTestRunner successfulEnvironmentTest successfulUnitSetup successfulTeardown

        let mutable result = newFailure.With.TestExecutionWasNotRunValidationFailure () |> TestFailure
        
        runner.RunnerLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | RunnerTestLifeCycle(_, TestEndSetup _, _) -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | RunnerTestLifeCycle(currentTest, TestEndSetup _, _) ->
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
    
let ``should not be raised if RunnerExecutionStart canceled`` =
    feature.Test (fun () ->
        let runner, _test = buildTestRunner successfulEnvironmentTest successfulUnitSetup successfulTeardown
        
        let mutable result = TestSuccess
        
        runner.RunnerLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | RunnerTestLifeCycle(_, TestEndSetup _, _)
            | RunnerStartExecution _ -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | RunnerTestLifeCycle(_, TestEndSetup _, _) ->
                result <- expects.NotToBeCalled ()
            | RunnerStartExecution cancelEventArgs ->
                cancelEventArgs.Cancel <- true
            | _ -> ()
        )
        
        ()
        |> runner.Run
        |> ignore
        
        result
    )
    
let ``should carry the result of the EndSetup event`` =
    feature.Test (fun () ->
        let expectedResult = ("Should blow up", { FilePath = ignoreString (); FileName = ignoreString (); LineNumber = ignoreInt () }) |> GeneralSetupTeardownFailure
        let setup _ = Error expectedResult
        
        let runner, _test = buildTestRunner successfulEnvironmentTest setup successfulTeardown
        
        let mutable result = newFailure.With.TestExecutionWasNotRunValidationFailure () |> TestFailure
        
        runner.RunnerLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | RunnerTestLifeCycle(_, TestEndSetup _, _) -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | RunnerTestLifeCycle(_, TestEndSetup (testResult, _), _) ->
                result <-
                    testResult
                    |> Should.BeEqualTo (expectedResult |> SetupFailure)
            | _ -> ()
        )
        
        ()
        |> runner.Run
        |> ignore
        
        result
    )
    
let ``Test Cases`` = feature.GetTests ()