module Archer.Tests.Scripts.``TestEndSetup Event should``

open Archer
open Archer.Arrows
open Archer.Arrows.Helpers
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.RunnerTypes
open Archer.MicroLang

let private feature = Arrow.NewFeature ()

let ``be raised from the given test when the framework is run`` =
    feature.Test (fun _ ->
        let framework, test = buildTestFramework successfulEnvironmentTest successfulUnitSetup successfulTeardown

        let mutable result = newFailure.With.TestExecutionWasNotRunValidationFailure () |> TestFailure
        
        framework.RunnerLifecycleEvent
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
        |> framework.Run
        |> ignore

        result
    )
    
let ``should not be raised if FrameworkExecutionStart canceled`` =
    feature.Test (fun _ ->
        let framework, _test = buildTestFramework successfulEnvironmentTest successfulUnitSetup successfulTeardown
        
        let mutable result = TestSuccess
        
        framework.RunnerLifecycleEvent
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
        |> framework.Run
        |> ignore
        
        result
    )
    
let ``should carry the result of the EndSetup event`` =
    feature.Test (fun _ ->
        let expectedResult = ("Should blow up", { FilePath = ignoreString (); FileName = ignoreString (); LineNumber = ignoreInt () }) |> GeneralSetupTeardownFailure
        let setup _ = Error expectedResult
        
        let framework, _test = buildTestFramework successfulEnvironmentTest setup successfulTeardown
        
        let mutable result = newFailure.With.TestExecutionWasNotRunValidationFailure () |> TestFailure
        
        framework.RunnerLifecycleEvent
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
        |> framework.Run
        |> ignore
        
        result
    )
    
let ``Test Cases`` = feature.GetTests ()