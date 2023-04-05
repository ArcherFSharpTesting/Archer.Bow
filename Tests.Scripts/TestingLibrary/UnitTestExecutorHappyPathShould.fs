module Archer.Tests.Scripts.TestingLibrary.``UnitTestExecutor Happy Path``

open Archer.Tests.Scripts.TestLang

let private container = suite.Container ("TestLibrary", "UnitTestExecutor happy path")

let ``Test Cases`` = [
    container.Test("Should have the creating test as its parent", fun () ->
        let executor = dummyExecutor None None
        
        executor.Parent
        |> expectsToBe executor.Parent
    )
    
    container.Test ("Should return success if test action returns success", fun () ->
        let test = dummyExecutor None None
        
        test.Execute ()
    )
    
    container.Test("Should raise all events in correct order", fun () ->
        let executor = dummyExecutor None None
        
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
]