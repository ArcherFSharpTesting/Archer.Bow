module Archer.Tests.Scripts.``UnitTestExecutor Failing Test``

open Archer.CoreTypes.Lib
open Archer.CoreTypes.Lib.InternalTypes
open Archer.Tests.Scripts.TestLang
open Archer.Tests.Scripts.TestLang.Types

let private container = suite.Container ("Scripting", "UnitTestExecutor Failing Test")
let generateFailure failureType details =
    details |> failureType |> TestFailure
    
let private dummyExecutor (testAction: (unit -> TestResult) option) (parts: TestPart option) =
    let test = dummyTest testAction parts
        
    test.GetExecutor ()
    
let ``Should return failure if the test action returns failure`` =
    container.Test ("Should return failure if the test action returns failure", fun () ->
        let expectedResult = "Things don't add up" |> generateFailure VerificationFailure
        let test = dummyExecutor (Some (fun () -> expectedResult)) None
        
        let result = test.Execute ()
        
        result
        |> expectsToBe expectedResult
    )
    
let ``Should raise all events even if setup fails`` =
    container.Test ("Should raise all events even if setup fails", fun () ->
        let failure = "Setup Fail" |> generateFailure SetupFailure
        let test = dummyExecutor None (Some (SetupPart (fun () -> failure)))
        
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
    
let ``Should return failure if setup fails`` =
    container.Test ("Should raise all events even if setup fails", fun () ->
        let failure = "Setup Fail" |> generateFailure SetupFailure
        let test = dummyExecutor None (Some (SetupPart (fun () -> failure)))
        
        let result = test.Execute ()
        
        result
        |> expectsToBe failure
    )
    
let ``Should carry the setup error in future events`` =
    container.Test ("Should raise all events even if setup fails", fun () ->
        let failure = "Setup Fail" |> generateFailure SetupFailure
        let test = dummyExecutor None (Some (SetupPart (fun () -> failure)))
        
        let mutable result = notRunError
        
        let combineResult = combineResultIgnoring notRunError
        
        let testTheResult _ (args: obj) =
            let a =
                match args with
                | :? TestCancelEventArgsWithResults as a -> a.TestResult
                | :? TestEventArgs as b -> b.TestResult
                | _ -> failure

            let r =
                a
                |> expectsToBe failure
                |> combineResult result
                
            result <- r
            
        test.EndSetup.AddHandler testTheResult
        test.EndTest.AddHandler testTheResult
        test.EndExecution.AddHandler testTheResult
        
        test.Execute () |> ignore 
        
        result
    )
    
let ``Should not run test action`` =
    container.Test ("Should raise all events even if setup fails", fun () ->
        let failure = "Setup Fail" |> generateFailure SetupFailure
        let mutable result = TestSuccess
        
        let testAction () =
            result <- "Test should not have run" |> VerificationFailure |> TestFailure
            result
        
        let test = dummyExecutor (Some testAction) (Some (SetupPart (fun () -> failure)))
        
        test.Execute () |> ignore
        
        result
    )