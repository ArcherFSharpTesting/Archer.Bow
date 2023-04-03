module Archer.Tests.Scripts.Scripting.``UnitTestExecutor Should``

open Archer.CoreTypes.Lib
open Archer.CoreTypes.Lib.InternalTypes
open Archer.Tests.Scripts.TestLang
open Archer.Tests.Scripts.TestLang.Types

let private container = suite.Container ("Scripting", "UnitTestExecutor Should")
let private dummyTest = UnitTest (ignoreString (), ignoreString (), ignoreString (), ignoreInt (), [], successfulTest, EmptyPart) :> ITest
let private notRunError = "Not Run" |> GeneralFailure |> TestFailure

let ``Should have the creating test as its parent`` =
    container.Test("Should have the creating test as its parent", fun () ->
            let test = dummyTest
            
            let executor = test.GetExecutor ()
            
            executor.Parent
            |> expectsToBe test
        )
    
let ``Should return success if test action returns success`` =
    container.Test ("Should return success if test action returns success", fun () ->
            let test = dummyTest
            
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
            let test = dummyTest
            
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
            let test = dummyTest
            
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
        let test = dummyTest
        
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
        let test = dummyTest
        
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
        let test = dummyTest
        
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
        let test = dummyTest
        
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
        let test = dummyTest
        
        let executor = test.GetExecutor ()
        let mutable result = notRunError
        executor.EndExecution.AddHandler (fun tst _ ->
            result <- tst |> expectsToBe test
        )
        
        executor.Execute ()
        |> ignore
        
        result
    )