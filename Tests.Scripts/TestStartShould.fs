module Archer.Tests.Scripts.``TestStart Event``

open Archer.CoreTypes
open Archer.Tests.Scripts.TestLang
open Archer.Tests.Scripts.TestLang.Types

let private container = suite.Container ("", "TestStart Event should")

let ``Test Cases`` = [
    container.Test ("be raised when the framework is run", fun () ->
        let framework, _test = buildTestFramework None None
        
        let mutable result = notRunError
        
        framework.TestStart.AddHandler (fun fr _args ->
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