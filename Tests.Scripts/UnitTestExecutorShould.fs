module Archer.Tests.Scripts.Scripting.``UnitTestExecutor Should``

open Archer.CoreTypes.Lib.InternalTypes
open Archer.Tests.Scripts.TestLang
open Archer.Tests.Scripts.TestLang.Types

let private container = suite.Container ("Scripting", "UnitTestExecutor Should")

let ``Should have the creating test as its parent`` =
    container.Test("Should have the creating test as its parent", fun () ->
            let test = UnitTest (ignoreString (), ignoreString (), ignoreString (), ignoreInt (), [], successfulTest, EmptyPart) :> ITest
            
            let executor = test.GetExecutor ()
            
            executor.Parent
            |> expectsToBe test
        )