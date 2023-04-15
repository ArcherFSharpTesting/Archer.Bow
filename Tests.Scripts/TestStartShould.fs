module Archer.Tests.Scripts.``TestStart Event should``

open Archer
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.FrameworkTypes
open Archer.MicroLang

let private container = suite.Container ()

let ``be raised with the given test when the framework is run`` =
    container.Test (fun _ ->
        let framework, test = buildBasicFramework ()
        
        let mutable result = expects.GeneralNotRunFailure () |> TestFailure
        
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
                    |> expects.ToBe test
            | _ -> ()
        )
        
        ()
        |> framework.Run
        |> ignore
        
        result
    )
    
let ``should not run the test action when canceled from test arg`` =
    container.Test (fun _ ->
        let expectedFailure = expects.GeneralNotRunFailure () |> TestFailure
        let mutable result = expectedFailure 
        
        let testAction =
            (fun _ _ ->
                result <- expects.NotRunValidationFailure () |> TestFailure
                TestSuccess
            )
            
        let framework, _ = buildTestFramework testAction successfulUnitSetup successfulTeardown
        
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
        |> expects.ToBe expectedFailure
    )
    
let ``should not run the test action when canceled from framework arg`` =
    container.Test (fun _ ->
        let expectedFailure = expects.GeneralNotRunFailure () |> TestFailure 
        let mutable result = expectedFailure
        
        let testAction =
            (fun _ _ ->
                result <- expects.NotRunValidationFailure () |> TestFailure
                TestSuccess
            )
            
        let framework, _ = buildTestFramework testAction successfulUnitSetup successfulTeardown
        
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
        |> expects.ToBe expectedFailure
    )
    
let ``Test Cases`` = container.Tests