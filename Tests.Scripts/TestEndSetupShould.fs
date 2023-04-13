module Archer.Tests.Scripts.``TestEndSetup Event``

open Archer
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.FrameworkTypes
open Archer.MicroLang
open Archer.MicroLang.Types

let private container = suite.Container ("", "TestEndSetup Event should")

let ``Test Cases`` = [
    container.Test ("be raised from the given test when the framework is run", fun _ ->
        let framework, test = buildTestFramework None None

        let mutable result = expects.GeneralNotRunFailure () |> TestFailure
        
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
    
    container.Test ("should not be raised if FrameworkExecutionStart canceled", fun _ ->
        let framework, _test = buildTestFramework None None
        
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
                result <- expects.NotRunValidationFailure () |> TestFailure
            | FrameworkStartExecution cancelEventArgs ->
                cancelEventArgs.Cancel <- true
            | _ -> ()
        )
        
        ()
        |> framework.Run
        |> ignore
        
        result
    )
    
    container.Test ("should carry the result of the EndSetup event", fun _ ->
        let expectedResult = ("Should blow up", { FilePath = ignoreString (); FileName = ignoreString (); LineNumber = ignoreInt () }) |> SetupFailure |> TestFailure
        let setup =
            (fun () -> expectedResult)
            |> SetupPart
            |> Some
        
        let framework, _test = buildTestFramework None setup
        
        let mutable result = expects.GeneralNotRunFailure () |> TestFailure
        
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
                    |> expects.ToBe expectedResult
            | _ -> ()
        )
        
        ()
        |> framework.Run
        |> ignore
        
        result
    )
]