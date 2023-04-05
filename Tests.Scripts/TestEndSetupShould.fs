module Archer.Tests.Scripts.``TestEndSetup Event``

open Archer.Bow.Lib
open Archer.CoreTypes
open Archer.Tests.Scripts.TestLang
open Archer.Tests.Scripts.TestLang.Types

let private defaultSeed = 33
let private getDefaultSeed () = defaultSeed

let private container = suite.Container ("", "TestEndSetup should")

let ``Test Cases`` = [
    container.Test ("be raised from the given test when the framework is run", fun () ->
        let framework, test = buildTestFramework None None

        let mutable result = notRunError

        framework.TestEndSetup.AddHandler (fun fr args ->
            let r =
                if fr = framework then TestSuccess
                else
                    $"expected\n%A{fr}\nto be\n%A{framework}"
                    |> VerificationFailure
                    |> TestFailure

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
            result <- "Event should not have fired" |> VerificationFailure |> TestFailure
        )
        
        framework.FrameworkStartExecution.AddHandler (fun _ args ->
            args.Cancel <- true
        )
        
        ()
        |> framework.Run
        |> ignore
        
        result
    )
    
    container.Test ("should carry the result of the setup method", fun () ->
        let expectedResult = "Should blow up" |> SetupFailure |> TestFailure
        let setup =
            (fun () -> expectedResult)
            |> SetupPart
            |> Some
        
        let framework, _test = buildTestFramework None setup
        
        let mutable result = notRunError
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