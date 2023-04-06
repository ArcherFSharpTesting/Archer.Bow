module Archer.Tests.Scripts.``TestStart Event``

open Archer.CoreTypes
open Archer.Tests.Scripts.TestLang

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
    
    container.Test ("be raised with the given test when the framework is run", fun () ->
        let framework, test = buildTestFramework None None
        
        let mutable result = notRunError
        
        framework.TestStart.AddHandler (fun _ args ->
            result <-
                args.Test
                |> expectsToBe test
        )
        
        ()
        |> framework.Run
        |> ignore
        
        result
    )
    
    container.Test ("should not run the test action when canceled", fun () ->
        let mutable result = notRunError
        
        let testAction =
            (fun () ->
                result <- "Should not be run" |> VerificationFailure |> TestFailure
                TestSuccess
            )
            |> Some
            
        let framework, _ = buildTestFramework testAction None
        
        framework.TestStart.Add (fun args ->
            args.Cancel <- true
        )
        
        result
        |> expectsToBe notRunError
    )
]