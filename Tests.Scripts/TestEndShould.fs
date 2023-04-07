module Archer.Tests.Scripts.``TestEnd Event``

open Archer.MicroLang.Lang

let private container = suite.Container ("", "TestEnd Event should")

let ``Test Cases`` = [
    container.Test ("be raised when the framework is run", fun () ->
        let framework, _test = buildTestFramework None None
        
        let mutable result = notRunGeneralFailure
        
        framework.TestEnd.AddHandler (fun fr _args ->
            result <-
                fr
                |> expectsToBe framework
        )
        
        ()
        |> framework.Run
        |> ignore
        
        result
    )
    
    container.Test ("raise event with given test", fun () ->
        let framework, test = buildTestFramework None None
        
        let mutable result = notRunGeneralFailure
        
        framework.TestEnd.AddHandler (fun fr args ->
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