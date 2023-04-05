module Archer.Tests.Scripts.TestingLibrary.``UnitTestExecutor StartExecution``

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
]