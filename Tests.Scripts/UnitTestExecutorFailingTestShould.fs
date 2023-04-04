module Archer.Tests.Scripts.``UnitTestExecutor Failing Test``

open Archer.CoreTypes.Lib
open Archer.CoreTypes.Lib.InternalTypes
open Archer.Tests.Scripts.TestLang
open Archer.Tests.Scripts.TestLang.Types

let private container = suite.Container ("Scripting", "UnitTestExecutor Failing Test")
let generateFailure failureType details =
    details |> failureType |> TestFailure
    
let private dummyTest (testAction: (unit -> TestResult) option) (parts: TestPart option) =
    let c = suite.Container (ignoreString (), ignoreString ())
    let test =
        match parts, testAction with
        | None, None -> c.Test (ignoreString (), successfulTest, EmptyPart, ignoreString (), ignoreInt ())
        | None, Some action -> c.Test (ignoreString (), action, EmptyPart, ignoreString (), ignoreInt ())
        | Some part, None -> c.Test (ignoreString (), successfulTest, part, ignoreString (), ignoreInt ())
        | Some part, Some action -> c.Test (ignoreString (), action, part, ignoreString (), ignoreInt ())
        
    test.GetExecutor ()
    
let private notRunError = "Not Run" |> GeneralFailure |> TestFailure
    
let ``Should return failure if the test action returns failure`` =
    container.Test ("Should return failure if the test action returns failure", fun () ->
        let expectedResult = "Things don't add up" |> generateFailure VerificationFailure
        let test = dummyTest (Some (fun () -> expectedResult)) None
        
        let result = test.Execute ()
        
        result
        |> expectsToBe expectedResult
    )
    
let ``Should raise all events even if setup fails`` =
    container.Test ("Should raise all events even if setup fails", fun () ->
        let failure = "Setup Fail" |> generateFailure SetupFailure
        let test = dummyTest None (Some (SetupPart (fun () -> failure)))
        
        let mutable cnt = 0
        let increment _ _ = cnt <- cnt + 1
        
        test.StartExecution.AddHandler increment
        test.StartSetup.AddHandler increment
        test.EndSetup.AddHandler increment
        test.StartTest.AddHandler increment
        test.EndTest.AddHandler increment
        test.StartTearDown.AddHandler increment
        test.EndExecution.AddHandler increment
        
        test.Execute () |> ignore 
        
        cnt
        |> expectsToBe 7
    )