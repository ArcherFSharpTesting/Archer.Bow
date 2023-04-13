module Archer.Tests.Scripts.``TestExecutionStarted Event``

open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.FrameworkTypes
open Archer.MicroLang

let private defaultSeed = 33
let private getDefaultSeed () = defaultSeed

let private container = suite.Container ("", "TestExecutionStarted Event should")

let ``Test Cases`` = [
    container.Test ("be raised from the given test when framework is run", fun _ ->
        let framework, test = buildTestFramework None None

        let mutable result = "Not Called" |> build.AsGeneralTestFailure
        
        framework.FrameworkLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | FrameworkTestLifeCycle (_, TestExecutionStarted _, _) -> true
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

        getDefaultSeed
        |> framework.Run
        |> ignore

        result
    )
]