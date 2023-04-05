module Archer.Tests.Scripts.TestingLibrary.``UnitTestExecutor StartSetup``

open Archer.Tests.Scripts.TestLang
open Archer.CoreTypes
open Archer.Tests.Scripts.TestLang.Types

let private container = suite.Container ("TestingLibrary", "UnitTestExecutor StartSetup should")

let ``Test Cases`` = [
    container.Test ("be raised when test is executed", fun () ->
        let executor = dummyExecutor None None
        
        let mutable result = notRunError
        executor.StartSetup.AddHandler (fun tst _ ->
            result <- tst |> expectsToBe executor.Parent
        )
        
        executor.Execute ()
        |> ignore
        
        result
    )
    
    container.Test ("prevent the call of the test setup if canceled", fun () ->
        let mutable result = TestSuccess
        
        let setupPart =
            SetupPart (fun () ->
                result <- "Should not be called" |> VerificationFailure |> TestFailure
                TestSuccess
            )
            |> Some
            
        let executor = dummyExecutor None setupPart
        
        executor.StartSetup.Add (fun args ->
            args.Cancel <- true
        )
        
        executor.Execute ()  |> ignore
        
        result
    )
    
    container.Test ("prevent the call of the test action if canceled", fun () ->
        let mutable result = TestSuccess
        
        let testAction () =
            result <- "Should not be called" |> VerificationFailure |> TestFailure
            TestSuccess
            
        let executor = dummyExecutor (Some testAction) None
        
        executor.StartSetup.Add (fun args ->
            args.Cancel <- true
        )
        
        executor.Execute ()  |> ignore
        
        result
    )
    
    container.Test ("should cause execution to return a CancelError if canceled", fun () ->
        let executor = dummyExecutor None None
        
        executor.StartSetup.Add (fun args ->
            args.Cancel <- true
        )
        
        executor.Execute ()
        |> expectsToBe (TestFailure CancelFailure)
    )
]