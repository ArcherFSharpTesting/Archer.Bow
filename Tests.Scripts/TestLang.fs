[<AutoOpen>]
module Archer.Tests.Scripts.TestLang.Lang

open Archer.CoreTypes
open Archer.Bow.Lib
open Archer.Tests.Scripts.TestLang.Types

let suite = TestContainerBuilder ()

let randomInt _ = System.Random().Next ()
let ignoreInt _ = randomInt ()
let ignoreString _ = $"%d{randomInt ()}%d{randomInt ()}%d{randomInt ()}"

let combineResultIgnoring defaultError a b =
    match a, b with
    | var, _ when var = defaultError -> b
    | _, var when var = defaultError -> a
    | TestSuccess, _ -> b
    | _, TestSuccess -> a
    | TestFailure tfa, TestFailure tfb -> CombinationFailure (tfa, tfb) |> TestFailure

let combineError = combineResultIgnoring TestSuccess

let successfulTest () = TestSuccess

let expectsToBe expected result =
    if expected = result then TestSuccess
    else
        $"expected \"%A{result}\" to be \"%A{expected}\""
        |> VerificationFailure
        |> TestFailure
        
let expectsToBeWithMessage expected message result =
    let r =
        result
        |> expectsToBe expected
        
    match r with
    | TestSuccess -> r
    | TestFailure f -> FailureWithMessage (message, f) |> TestFailure

let verifyWith = expectsToBe
        
let expectsToBeTrue result =
    if result then TestSuccess
    else
        "expected true and got false"
        |> VerificationFailure
        |> TestFailure
        
let buildDummyTest (testAction: (unit -> TestResult) option) (parts: TestPart option) =
    let c = suite.Container (ignoreString (), ignoreString ())
        
    match parts, testAction with
    | None, None -> c.Test (ignoreString (), successfulTest, EmptyPart, ignoreString (), ignoreInt ())
    | None, Some action -> c.Test (ignoreString (), action, EmptyPart, ignoreString (), ignoreInt ())
    | Some part, None -> c.Test (ignoreString (), successfulTest, part, ignoreString (), ignoreInt ())
    | Some part, Some action -> c.Test (ignoreString (), action, part, ignoreString (), ignoreInt ())
    
let buildDummyExecutor (testAction: (unit -> TestResult) option) (parts: TestPart option) =
    let test = buildDummyTest testAction parts
    
    test.GetExecutor ()
    
let buildTestFramework (testAction: (unit -> TestResult) option) (parts: TestPart option) =
    let framework = archer.Framework ()
    let test = buildDummyTest testAction parts

    framework.AddTests [test]
    framework, test
    
let notRunError = "Not Run" |> GeneralFailure |> TestFailure