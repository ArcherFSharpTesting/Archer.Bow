module Archer.Tests.Scripts.TestingLibrary.``UnitTestExecutor EndExecution``
    
open Archer.Tests.Scripts.TestLang

let private container = suite.Container ("TestLibrary", "UnitTestExecutor EndExecution should")

let ``Test Cases`` = [
    container.Test ("be raised when the test is executed", fun () ->
        let executor = dummyExecutor None None
        
        let mutable result = notRunError
        executor.EndExecution.AddHandler (fun tst _ ->
            result <- tst |> expectsToBe executor.Parent
        )
        
        executor.Execute ()
        |> ignore
        
        result
    )
]