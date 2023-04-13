module Archer.Tests.Scripts.``TestStartTearDown Event``

open Archer
open Archer.CoreTypes.InternalTypes.FrameworkTypes
open Archer.MicroLang

let private container = suite.Container ("", "TestEnd Event should")

let ``Test Cases`` = [
    container.Test ("be raised with the given test", fun _ ->
        let framework, test = buildTestFramework None None
        
        let mutable result = expects.GeneralNotRunFailure () |> TestFailure
        
        framework.FrameworkLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | FrameworkTestLifeCycle(_, TestStartTearDown, _) -> true
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
]