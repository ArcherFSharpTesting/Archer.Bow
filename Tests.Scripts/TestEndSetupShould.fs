module Archer.Tests.Scripts.``TestEndSetup Event``

open Archer.Bow.Lib
open Archer.CoreTypes.Lib
open Archer.Tests.Scripts.TestLang

let private defaultSeed = 33
let private getDefaultSeed () = defaultSeed

let private container = suite.Container ("", "TestEndSetup should")

let private buildTestFramework () =
    let framework = archer.Framework ()
    let test = dummyTest None None

    framework.AddTests [test]
    framework, test

let ``Test Cases`` = [
    container.Test ("be raised from the given test when the framework is run", fun () ->
        let framework, test = buildTestFramework ()

        let mutable result = notRunError

        framework.TestEndSetup.AddHandler (fun fr args ->
            let r =
                if fr = framework then TestSuccess
                else
                    $"expected\n%A{fr}\nto be\n%A{framework}"
                    |> VerificationFailure
                    |> TestFailure

            result <- args.Test
            |> expectsToBe test
            |> combineError r
        )

        getDefaultSeed
        |> framework.Run
        |> ignore

        result
    )
    
    container.Test ("should not be raised if FrameworkExecutionStart canceled", fun () ->
        let framework, _test = buildTestFramework ()
        
        let mutable result = TestSuccess
        
        framework.TestEndSetup.AddHandler (fun _ _ ->
            result <- "Event should not have fired" |> VerificationFailure |> TestFailure
        )
        
        framework.FrameworkStartExecution.AddHandler (fun _ args ->
            args.Cancel <- true
        )
        
        getDefaultSeed
        |> framework.Run
        |> ignore
        
        result
    )
    
    container.Test ("should not be raised if TestStartExecution is canceled", fun () ->
        let framework, _test = buildTestFramework ()
        
        let mutable result = TestSuccess
        
        framework.TestEndSetup.AddHandler (fun _ _ ->
            result <- "Event should not have fired" |> VerificationFailure |> TestFailure
        )
        
        framework.TestStartExecution.AddHandler (fun _ args ->
            args.Cancel <- true
        )
        
        getDefaultSeed
        |> framework.Run
        |> ignore
        
        result |> ignore
        "Waiting for tests of test scaffolding"
        |> Some
        |> IgnoredFailure
        |> TestFailure
    )
]