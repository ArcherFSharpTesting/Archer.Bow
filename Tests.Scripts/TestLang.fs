[<AutoOpen>]
module Archer.Tests.Scripts.TestLang.Lang

open Archer.CoreTypes.Lib
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