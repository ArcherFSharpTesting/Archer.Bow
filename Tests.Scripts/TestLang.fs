[<AutoOpen>]
module Archer.Tests.Scripts.TestLang.Lang

open Archer.CoreTypes.Lib
open Archer.Tests.Scripts.TestLang.Types

let suite = TestContainerBuilder ()

let randomInt _ = System.Random().Next ()
let ignoreInt _ = randomInt ()
let ignoreString _ = $"%d{randomInt ()}%d{randomInt ()}%d{randomInt ()}"
let successfulTest () = TestSuccess

let expectsToBe expected result =
    if expected = result then TestSuccess
    else
        $"expected \"%A{result}\" to be \"%A{expected}\""
        |> VerificationFailure
        |> TestFailure

let verifyWith = expectsToBe
        
let expectsToBeTrue result =
    if result then TestSuccess
    else
        "expected true and got false"
        |> VerificationFailure
        |> TestFailure