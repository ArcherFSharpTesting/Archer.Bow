module Archer.Tests.Scripts.``UnitTestExecutor Failing Test``

open Archer.CoreTypes.Lib
open Archer.CoreTypes.Lib.InternalTypes
open Archer.Tests.Scripts.TestLang
open Archer.Tests.Scripts.TestLang.Types

let private container = suite.Container ("Scripting", "UnitTestExecutor Failing Test")
let private dummyTest (testAction: (unit -> TestResult) option) (parts: TestPart option) =
    let c = suite.Container (ignoreString (), ignoreString ())
    match parts, testAction with
    | None, None -> c.Test (ignoreString (), successfulTest, EmptyPart, ignoreString (), ignoreInt ())
    | None, Some action -> c.Test (ignoreString (), action, EmptyPart, ignoreString (), ignoreInt ())
    | Some part, None -> c.Test (ignoreString (), successfulTest, part, ignoreString (), ignoreInt ())
    | Some part, Some action -> c.Test (ignoreString (), action, part, ignoreString (), ignoreInt ())
    
let private notRunError = "Not Run" |> GeneralFailure |> TestFailure
    
let ``Should return failure if the test action returns failure`` =
    container.Test ("Should return failure if the test action returns failure", fun () ->
        let expectedResult = "Things don't add up" |> VerificationFailure |> TestFailure
        let test = dummyTest (Some (fun () -> expectedResult)) None
        
        let result = test.GetExecutor().Execute ()
        
        result
        |> expectsToBe expectedResult
    )