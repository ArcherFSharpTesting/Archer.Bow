module Archer.Tests.Scripts.``TestEndSetup Event``

open Archer.CoreTypes
open Archer.MicroLang
open Archer.MicroLang.Types

let private container = suite.Container ("", "TestEndSetup Event should")

let ``Test Cases`` = [
    container.Test ("be raised from the given test when the framework is run", fun () ->
        let framework, test = buildTestFramework None None

        let mutable result = notRunGeneralFailure

        framework.TestEndSetup.AddHandler (fun fr args ->
            let r =
                if fr = framework then TestSuccess
                else
                    fr
                    |> expectsToBe framework

            result <-
                args.Test
                |> expectsToBe test
                |> combineError r
        )

        ()
        |> framework.Run
        |> ignore

        result
    )
    
    container.Test ("should not be raised if FrameworkExecutionStart canceled", fun () ->
        let framework, _test = buildTestFramework None None
        
        let mutable result = TestSuccess
        
        framework.TestEndSetup.AddHandler (fun _ _ ->
            result <- notRunValidationFailure
        )
        
        framework.FrameworkStartExecution.AddHandler (fun _ args ->
            args.Cancel <- true
        )
        
        ()
        |> framework.Run
        |> ignore
        
        result
    )
    
    container.Test ("should carry the result of the EndSetup event", fun () ->
        let expectedResult = "Should blow up" |> SetupFailure |> TestFailure
        let setup =
            (fun () -> expectedResult)
            |> SetupPart
            |> Some
        
        let framework, _test = buildTestFramework None setup
        
        let mutable result = notRunGeneralFailure
        framework.TestEndSetup.Add (fun args ->
            result <-
                args.TestResult
                |> expectsToBe expectedResult
        )
        
        ()
        |> framework.Run
        |> ignore
        
        result
    )
]