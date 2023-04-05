module Archer.Tests.Scripts.TestingLibrary.``UnitTestExecutor EndSetup``

open Archer.Tests.Scripts.TestLang
open Archer.Tests.Scripts.TestLang.Types
open Archer.CoreTypes

let private container = suite.Container ("TestingLibrary", "UnitTestExecutor EndSetup should")

let ``Test Cases`` = [
    container.Test ("be raised when the test is executed", fun () ->
        let executor = dummyExecutor None None
        
        let mutable result = notRunError
        executor.EndSetup.AddHandler (fun tst _ ->
            result <- tst |> expectsToBe executor.Parent
        )
        
        executor.Execute ()
        |> ignore
        
        result
    )
    
    container.Test ("carry the result of the StartSetup event", fun () ->
        let expectedFailure = "Failures abound" |> SetupFailure |> TestFailure
        let setupPart = SetupPart (fun () -> expectedFailure) |> Some
        let executor = dummyExecutor None setupPart
        
        let mutable result = notRunError
        
        executor.EndSetup.Add (fun args ->
            result <- args.TestResult
        )
        
        executor.Execute () |> ignore
        
        result
        |> expectsToBe expectedFailure
    )
    
    container.Test ("prevent the call of the test action if canceled", fun () ->
        let mutable result = TestSuccess
        
        let testAction () =
            result <- "Should not be called" |> VerificationFailure |> TestFailure
            TestSuccess
            
        let executor = dummyExecutor (Some testAction) None
        
        executor.EndSetup.Add (fun args ->
            args.Cancel <- true
        )
        
        executor.Execute ()  |> ignore
        
        result
    )
    
    container.Test ("should cause execution to return a CancelError if canceled", fun () ->
        let executor = dummyExecutor None None
        
        executor.EndSetup.Add (fun args ->
            args.Cancel <- true
        )
        
        executor.Execute ()
        |> expectsToBe (TestFailure CancelFailure)
    )
]