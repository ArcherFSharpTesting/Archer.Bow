module Archer.Tests.Scripts.TestingLibrary.``UnitTestExecutor StartSetup should``

open Archer.Tests.Scripts.TestLang

let private container = suite.Container ("Scripting", "UnitTestExecutor happy path")
    
let ``be raised when test is executed`` =
    container.Test ("Should raise StartSetup", fun () ->
        let executor = dummyExecutor None None
        
        let mutable result = notRunError
        executor.StartSetup.AddHandler (fun tst _ ->
            result <- tst |> expectsToBe executor.Parent
        )
        
        executor.Execute ()
        |> ignore
        
        result
    )