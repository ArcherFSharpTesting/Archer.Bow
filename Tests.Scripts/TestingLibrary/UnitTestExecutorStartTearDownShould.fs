module Archer.Tests.Scripts.TestingLibrary.``UnitTestExecutor StartTearDown``
    
open Archer.Tests.Scripts.TestLang

let private container = suite.Container ("TestLibrary", "UnitTestExecutor StartTearDown should")

let ``Test Cases`` = [
    container.Test ("be raised when the test is executed", fun () ->
        let executor = buildDummyExecutor None None
        
        let mutable result = notRunError
        executor.StartTearDown.AddHandler (fun tst _ ->
            result <- tst |> expectsToBe executor.Parent
        )
        
        executor.Execute ()
        |> ignore
        
        result
    )
]