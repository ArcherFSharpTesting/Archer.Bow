module Archer.Tests.Scripts.``TestStartTearDown Event``

open Archer.Tests.Scripts.TestLang

let private container = suite.Container ("", "TestEnd Event should")

let ``Test Cases`` = [
    container.Test ("be raised when the framework is run", fun () ->
        let framework, _test = buildTestFramework None None
        
        let mutable result = notRunError
        framework.TestStartTearDown.AddHandler (fun fr test ->
            result <-
                fr
                |> expectsToBe framework
        )
        
        ()
        |> framework.Run
        |> ignore
        
        result
    )
]