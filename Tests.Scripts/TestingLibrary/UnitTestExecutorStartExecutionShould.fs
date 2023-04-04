module Archer.Tests.Scripts.TestingLibrary.``UnitTestExecutor StartExecution should``

open Archer.CoreTypes.Lib.InternalTypes
open Archer.Tests.Scripts.TestLang

let private container = suite.Container ("Scripting", "UnitTestExecutor StartExecution should")

let ``be raised when test is executed`` =
    container.Test ("Should raise StartExecution", fun () ->
        let executor = dummyExecutor None None
        
        let mutable result = notRunError
        executor.StartExecution.AddHandler (CancelDelegate (fun tst _ ->
                result <- tst |> expectsToBe executor.Parent
            )
        )
        
        executor.Execute () |> ignore
        result
    )