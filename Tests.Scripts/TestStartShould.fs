module Archer.Tests.Scripts.``TestStart Event``

open Archer
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.FrameworkTypes
open Archer.MicroLang

let private container = suite.Container ("", "TestStart Event should")

let ``Test Cases`` = [
    container.Test ("be raised with the given test when the framework is run", fun _ ->
        let framework, test = buildTestFramework None None
        
        let mutable result = notRunGeneralFailure
        
        framework.FrameworkLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | FrameworkTestLifeCycle(_, TestStart _, _) -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | FrameworkTestLifeCycle(currentTest, _, _) ->
                result <-
                    currentTest
                    |> expectsToBe test
            | _ -> ()
        )
        
        ()
        |> framework.Run
        |> ignore
        
        result
    )
    
    container.Test ("should not run the test action when canceled from test arg", fun _ ->
        let mutable result = notRunGeneralFailure
        
        let testAction =
            (fun _ ->
                result <- notRunValidationFailure
                TestSuccess
            )
            |> Some
            
        let framework, _ = buildTestFramework testAction None
        
        framework.FrameworkLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | FrameworkTestLifeCycle(_, TestStart _, _) -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | FrameworkTestLifeCycle(_, TestStart cancelEventArgs, _) ->
                cancelEventArgs.Cancel <- true
            | _ -> ()
        )
        
        result
        |> expectsToBe notRunGeneralFailure
    )
    
    container.Test ("should not run the test action when canceled from framework arg", fun _ ->
        let mutable result = notRunGeneralFailure
        
        let testAction =
            (fun _ ->
                result <- notRunValidationFailure
                TestSuccess
            )
            |> Some
            
        let framework, _ = buildTestFramework testAction None
        
        framework.FrameworkLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | FrameworkTestLifeCycle(_, TestStart _, _) -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | FrameworkTestLifeCycle(_, TestStart _, cancelEventArgs) ->
                cancelEventArgs.Cancel <- true
            | _ -> ()
        )
        
        result
        |> expectsToBe notRunGeneralFailure
    )
]