module Archer.Tests.Scripts.``TestStartSetup Event should``

open System.ComponentModel
open Archer.Bow
open Archer
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.FrameworkTypes
open Archer.MicroLang

let private defaultSeed = 33
let private getDefaultSeed () = defaultSeed

let private container = suite.Container ()

let ``be raised from the given test when the framework is run`` =
    container.Test (fun _ ->
        let framework, test = buildTestFramework successfulEnvironmentTest successfulUnitSetup successfulTeardown

        let mutable result = "Not Called" |> expects.AsGeneralFailure |> TestFailure
        
        framework.FrameworkLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | FrameworkTestLifeCycle (_, TestStartSetup _, _) -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | FrameworkTestLifeCycle (currentTest, _, _) ->
                result <-
                    currentTest
                    |> expects.ToBe test
            | _ -> ()
        )

        getDefaultSeed
        |> framework.Run
        |> ignore

        result
    )
    
let ``not be raised if FrameworkExecutionStarted was canceled`` =
    container.Test (fun _ ->
        let framework, _ = buildTestFramework successfulEnvironmentTest successfulUnitSetup successfulTeardown
         
        let mutable result = TestSuccess
        
        framework.FrameworkLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | FrameworkStartExecution _
            | FrameworkTestLifeCycle(_, TestStartExecution _, _) -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | FrameworkStartExecution cancelEventArgs ->
                cancelEventArgs.Cancel <- true
            | FrameworkTestLifeCycle _ ->
                result <- expects.NotRunValidationFailure () |> TestFailure
            | _ -> ()
        )
        
        framework.Run () |> ignore
        
        result
    )
    
let ``Test Cases`` = container.Tests