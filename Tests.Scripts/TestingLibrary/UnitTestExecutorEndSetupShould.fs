module Archer.Tests.Scripts.TestingLibrary.``UnitTestExecutor EndSetup should``

open Archer.Tests.Scripts.TestLang

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