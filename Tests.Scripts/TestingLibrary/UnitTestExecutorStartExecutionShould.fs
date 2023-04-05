module Archer.Tests.Scripts.TestingLibrary.``UnitTestExecutor StartExecution``

open Archer.Tests.Scripts.TestLang.Types
open Archer.CoreTypes
open Archer.CoreTypes.InternalTypes
open Archer.Tests.Scripts.TestLang

let private container = suite.Container ("TestingLibrary", "UnitTestExecutor StartExecution should")

let ``Test Cases`` = [
    container.Test ("be raised when test is executed", fun () ->
        let executor = dummyExecutor None None
        
        let mutable result = notRunError
        executor.StartExecution.AddHandler (CancelDelegate (fun tst _ ->
                result <- tst |> expectsToBe executor.Parent
            )
        )
        
        executor.Execute () |> ignore
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
        
        executor.StartExecution.Add (fun args ->
            args.Cancel <- true
        )
        
        executor.Execute ()  |> ignore
        
        result
    )
    
    container.Test ("should cause execution to return a CancelError if canceled", fun () ->
        let executor = dummyExecutor None None
        
        executor.StartExecution.Add (fun args ->
            args.Cancel <- true
        )
        
        executor.Execute ()
        |> expectsToBe (TestFailure CancelFailure)
    )
]