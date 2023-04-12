module Archer.Tests.Scripts.``TestEndExecution Event``

open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.FrameworkTypes
open Archer.MicroLang

let private container = suite.Container ("", "TestEndExecution Event should")

let ``Test Cases`` = [
    container.Test ("be raised with the given test", fun _ ->
        let framework, test = buildTestFramework None None
        
        let mutable result = notRunGeneralFailure
        
        framework.FrameworkLifecycleEvent
        |> Event.filter (fun args ->
            match args with
                | FrameworkTestLifeCycle(_, TestEndExecution _, _) -> true
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
]