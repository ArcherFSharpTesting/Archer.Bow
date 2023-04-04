module Archer.Tests.Scripts.``UnitTestExecutor Failing Test``

open Archer.CoreTypes.Lib
open Archer.CoreTypes.Lib.InternalTypes
open Archer.Tests.Scripts.TestLang
open Archer.Tests.Scripts.TestLang.Types

let private container = suite.Container ("Scripting", "UnitTestExecutor Failing Test")
let private dummyTest () = suite.Container(ignoreString (), ignoreString ()).Test(ignoreString (), successfulTest)
let private notRunError = "Not Run" |> GeneralFailure |> TestFailure
    
let ``Should return failure if the test action returns failure`` =
    container.Test ("Should return failure if the test action returns failure", fun () ->
        let expectedResult = "Things don't add up" |> VerificationFailure |> TestFailure
        let test = UnitTest (ignoreString (), ignoreString (), ignoreString (), ignoreInt (), [], (fun () -> expectedResult), EmptyPart) :> ITest
        
        let result = test.GetExecutor().Execute ()
        
        result
        |> expectsToBe expectedResult
    )