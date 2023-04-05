module Archer.Tests.Scripts.TestingLibrary.``UnitTestExecutor StartTest``

open Archer.Tests.Scripts.TestLang

let private container = suite.Container ("TestingLibrary", "UnitTestExecutor StartTest should")

let ``Test Cases`` = [
    container.Test ("be raised when the test is executed", fun () ->
        let executor = buildDummyExecutor None None
        
        let mutable result = notRunError
        executor.StartTest.AddHandler (fun tst _ ->
            result <- tst |> expectsToBe executor.Parent
        )
        
        executor.Execute ()
        |> ignore
        
        result
    )
]