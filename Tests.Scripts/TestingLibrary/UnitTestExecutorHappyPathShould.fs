module Archer.Tests.Scripts.TestingLibrary.``UnitTestExecutor Happy Path``

open Archer.CoreTypes.Lib
open Archer.CoreTypes.Lib.InternalTypes
open Archer.Tests.Scripts.TestLang

let private container = suite.Container ("Scripting", "UnitTestExecutor happy path")
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
        
        let combineResult = combineResultIgnoring notRunError
            
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