module Archer.Tests.Scripts.``TestStartTearDown Event should``

open Archer
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.FrameworkTypes
open Archer.MicroLang

let private container = suite.Container ()

let ``be raised with the given test`` =
    container.Test (fun _ ->
        let framework, test = buildTestFramework None None
        
        let mutable result = expects.GeneralNotRunFailure () |> TestFailure
        
        framework.FrameworkLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | FrameworkTestLifeCycle (_, TestStartTearDown, _) -> true
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
    
let ``Test Cases`` = container.Tests