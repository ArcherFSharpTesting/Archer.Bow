module Archer.Tests.Scripts.``TestStartTearDown Event``

open Archer.MicroLang

let private container = suite.Container ("", "TestEnd Event should")

let ``Test Cases`` = [
    container.Test ("be raised when the framework is run", fun _ ->
        let framework, _test = buildTestFramework None None
        
        let mutable result = notRunGeneralFailure
        framework.TestStartTearDown.AddHandler (fun fr _args ->
            result <-
                fr
                |> expectsToBe framework
        )
        
        ()
        |> framework.Run
        |> ignore
        
        result
    )
    
    container.Test ("be raised with the given test", fun _ ->
        let framework, test = buildTestFramework None None
        
        let mutable result = notRunGeneralFailure
        framework.TestStartTearDown.AddHandler (fun _fr args ->
            result <-
                args.Test
                |> expectsToBe test
        )
        
        ()
        |> framework.Run
        |> ignore
        
        result
    )
]