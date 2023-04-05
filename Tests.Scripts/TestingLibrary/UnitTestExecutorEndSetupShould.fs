module Archer.Tests.Scripts.TestingLibrary.``UnitTestExecutor EndSetup should``

open Archer.Tests.Scripts.TestLang
open Archer.Tests.Scripts.TestLang.Types
open Archer.CoreTypes.Lib

let private container = suite.Container ("TestingLibrary", "UnitTestExecutor EndSetup should")
    
let ``be raised when the test is executed`` =
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
    
let ``carry the result of the Setup Function`` =
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