module Archer.Tests.Scripts.Scripting.UnitTestExecutor

open Archer.CoreTypes.Lib
open Archer.CoreTypes.Lib.InternalTypes
open Archer.Tests.Scripts.TestLang
open Archer.Tests.Scripts.TestLang.Types

let private container = suite.Container ("Scripting", "UnitTestExecutor Should")
let private dummyTest () = suite.Container(ignoreString (), ignoreString ()).Test(ignoreString (), successfulTest)
let private notRunError = "Not Run" |> GeneralFailure |> TestFailure

let ``Should have the creating test as its parent`` =
    container.Test("Should have the creating test as its parent", fun () ->
            let test = dummyTest ()
            
            let executor = test.GetExecutor ()
            
            executor.Parent
            |> expectsToBe test
        )
    
let ``Should return success if test action returns success`` =
    container.Test ("Should return success if test action returns success", fun () ->
            let test = dummyTest ()
            
            test.GetExecutor().Execute ()
        )
    
let ``Should return failure if the test action returns failure`` =
    container.Test ("Should return failure if the test action returns failure", fun () ->
            let expectedResult = "Things don't add up" |> VerificationFailure |> TestFailure
            let test = UnitTest (ignoreString (), ignoreString (), ignoreString (), ignoreInt (), [], (fun () -> expectedResult), EmptyPart) :> ITest
            
            let result = test.GetExecutor().Execute ()
            
            result
            |> expectsToBe expectedResult
        )
    
let ``Should raise ExecutionStart`` =
    container.Test ("Should raise StartExecution", fun () ->
            let test = dummyTest ()
            
            let executor = test.GetExecutor ()
            
            let mutable result = notRunError
            executor.StartExecution.AddHandler (CancelDelegate (fun tst _ ->
                    result <- tst |> expectsToBe test
                )
            )
            
            executor.Execute () |> ignore
            result
        )
    
let ``Should raise StartSetup`` =
    container.Test ("Should raise StartSetup", fun () ->
            let test = dummyTest ()
            
            let executor = test.GetExecutor ()
            
            let mutable result = notRunError
            executor.StartSetup.AddHandler (fun tst _ ->
                result <- tst |> expectsToBe test
            )
            
            executor.Execute ()
            |> ignore
            
            result
        )
    
let ``Should raise EndSetup`` =
    container.Test ("Should raise EndSetup", fun () ->
        let test = dummyTest ()
        
        let executor = test.GetExecutor ()
        
        let mutable result = notRunError
        executor.EndSetup.AddHandler (fun tst _ ->
            result <- tst |> expectsToBe test
        )
        
        executor.Execute ()
        |> ignore
        
        result
    )
    
let ``Should raise StartTest`` =
    container.Test ("Should raise StartTest", fun () ->
        let test = dummyTest ()
        
        let executor = test.GetExecutor ()
        
        let mutable result = notRunError
        executor.StartTest.AddHandler (fun tst _ ->
            result <- tst |> expectsToBe test
        )
        
        executor.Execute ()
        |> ignore
        
        result
    )
    
let ``Should raise EndTest`` =
    container.Test ("Should raise EndTest", fun () ->
        let test = dummyTest ()
        
        let executor = test.GetExecutor ()
        let mutable result = notRunError 
        executor.EndTest.AddHandler (fun tst _ ->
            result <- tst |> expectsToBe test
        )
        
        executor.Execute ()
        |> ignore
        
        result
    )
    
let ``Should raise StartTearDown`` =
    container.Test ("Should raise StartTearDown", fun () ->
        let test = dummyTest ()
        
        let executor = test.GetExecutor ()
        let mutable result = notRunError
        executor.StartTearDown.AddHandler (fun tst _ ->
            result <- tst |> expectsToBe test
        )
        
        executor.Execute ()
        |> ignore
        
        result
    )
    
let ``Should raise EndExecution`` =
    container.Test ("Should raise EndExecution", fun () ->
        let test = dummyTest ()
        
        let executor = test.GetExecutor ()
        let mutable result = notRunError
        executor.EndExecution.AddHandler (fun tst _ ->
            result <- tst |> expectsToBe test
        )
        
        executor.Execute ()
        |> ignore
        
        result
    )
    
let ``Should raise all events in correct order`` =
    container.Test("Should raise all events in correct order", fun () ->
        let test = dummyTest ()
        let executor = test.GetExecutor ()
        
        let mutable cnt = 0
        let mutable result = notRunError
        
        let combineResult a b =
            match a, b with
            | var, _ when var = notRunError -> b
            | _, var when var = notRunError -> a
            | TestSuccess, _ -> b
            | _, TestSuccess -> a
            | TestFailure tfa, TestFailure tfb -> CombinationFailure (tfa, tfb) |> TestFailure
            
            
        executor.StartExecution.AddHandler (fun _ _ ->
            let r =
                cnt
                |> expectsToBeWithMessage 0 "Start Execution out of order"
                
            cnt <- cnt + 1
            result <- r |> combineResult result
        )
        
        executor.StartSetup.AddHandler (fun _ _ ->
            let r =
                cnt
                |> expectsToBeWithMessage 1 "Start Setup out of order"
                
            cnt <- cnt + 1
            result <- r |> combineResult result
        )
        
        executor.EndSetup.AddHandler (fun _ _ ->
            let r =
                cnt
                |> expectsToBeWithMessage 2 "End Setup out of order"
                
            cnt <- cnt + 1
            result <- r |> combineResult result
        )
        
        executor.StartTest.AddHandler (fun _ _ ->
            let r =
                cnt
                |> expectsToBeWithMessage 3 "Start Test out of order"
                
            cnt <- cnt + 1
            result <- r |> combineResult result
        )
        
        executor.EndTest.AddHandler (fun _ _ ->
            let r =
                cnt
                |> expectsToBeWithMessage 4 "End Test out of order"
                
            cnt <- cnt + 1
            result <- r |> combineResult result
        )
        
        executor.StartTearDown.AddHandler (fun _ _ ->
            let r =
                cnt
                |> expectsToBeWithMessage 5 "Start Tear Down out of order"
                
            cnt <- cnt + 1
            result <- r |> combineResult result
        )
        
        executor.EndExecution.AddHandler (fun _ _ ->
            let r =
                cnt
                |> expectsToBeWithMessage 6 "End Execution out of order"
                
            cnt <- cnt + 1
            result <- r |> combineResult result
        )
            
        executor.Execute () |> ignore
        result
    )