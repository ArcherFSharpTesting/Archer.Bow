module Archer.Tests.Scripts.``TestEndSetup Event should``

open Archer
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.FrameworkTypes
open Archer.MicroLang

let private container = suite.Container ()

let ``be raised from the given test when the framework is run`` =
    container.Test (fun _ ->
        let framework, test = buildTestFramework successfulEnvironmentTest successfulUnitSetup successfulTeardown

        let mutable result = newFailure.With.TestExecutionWasNotRunValidationFailure () |> TestFailure
        
        framework.FrameworkLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | FrameworkTestLifeCycle(_, TestEndSetup _, _) -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | FrameworkTestLifeCycle(currentTest, TestEndSetup _, _) ->
                result <-
                    currentTest
                    |> expects.ToBe test
            | _ -> ()
        )

        ()
        |> framework.Run
        |> ignore

        result
    )
    
let ``should not be raised if FrameworkExecutionStart canceled`` =
    container.Test (fun _ ->
        let framework, _test = buildTestFramework successfulEnvironmentTest successfulUnitSetup successfulTeardown
        
        let mutable result = TestSuccess
        
        framework.FrameworkLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | FrameworkTestLifeCycle(_, TestEndSetup _, _)
            | FrameworkStartExecution _ -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | FrameworkTestLifeCycle(_, TestEndSetup _, _) ->
                result <- expects.NotToBeCalled ()
            | FrameworkStartExecution cancelEventArgs ->
                cancelEventArgs.Cancel <- true
            | _ -> ()
        )
        
        ()
        |> framework.Run
        |> ignore
        
        result
    )
    
let ``should carry the result of the EndSetup event`` =
    container.Test (fun _ ->
        let expectedResult = ("Should blow up", { FilePath = ignoreString (); FileName = ignoreString (); LineNumber = ignoreInt () }) |> GeneralSetupTeardownFailure
        let setup _ = Error expectedResult
        
        let framework, _test = buildTestFramework successfulEnvironmentTest setup successfulTeardown
        
        let mutable result = newFailure.With.TestExecutionWasNotRunValidationFailure () |> TestFailure
        
        framework.FrameworkLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | FrameworkTestLifeCycle(_, TestEndSetup _, _) -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | FrameworkTestLifeCycle(_, TestEndSetup (testResult, _), _) ->
                result <-
                    testResult
                    |> expects.ToBe (expectedResult |> SetupFailure)
            | _ -> ()
        )
        
        ()
        |> framework.Run
        |> ignore
        
        result
    )
    
let ``Test Cases`` = container.Tests