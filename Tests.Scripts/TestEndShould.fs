module Archer.Tests.Scripts.``TestEnd Event should``

open Archer
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.FrameworkTypes
open Archer.MicroLang

let private container = suite.Container ()

let ``raise event with given test`` =
    container.Test (fun _ ->
        let framework, test = buildTestFramework None None
        
        let mutable result = expects.GeneralNotRunFailure () |> TestFailure
        
        framework.FrameworkLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | FrameworkTestLifeCycle(_, TestEnd _, _) -> true
            | _ -> false
        )
        |> Event.add (fun arg ->
            match arg with
            | FrameworkTestLifeCycle(currentTest, _, _) ->
                result <-
                    currentTest
                    |> expects.ToBe test
        )
        
        ()
        |> framework.Run
        |> ignore
        
        result
    )
    
let ``Test Cases`` = container.Tests