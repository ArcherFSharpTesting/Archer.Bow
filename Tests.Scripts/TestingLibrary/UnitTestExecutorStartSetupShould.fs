module Archer.Tests.Scripts.TestingLibrary.``UnitTestExecutor StartSetup should``

open Archer.Tests.Scripts.TestLang

let private container = suite.Container ("TestingLibrary", "UnitTestExecutor StartSetup should")
    
let ``be raised when the test is executed`` =
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