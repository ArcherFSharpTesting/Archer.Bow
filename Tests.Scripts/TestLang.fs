[<AutoOpen>]
module Archer.Tests.Scripts.TestLang.Lang

open Archer.CoreTypes.Lib
open Archer.Tests.Scripts.TestLang.Types

let suite = TestContainerBuilder ()

let verifyWith expected result =
    if expected = result then TestSuccess
    else
        $"expected \"%A{result}\" to be \"%A{expected}\""
        |> VerificationFailure
        |> TestFailure
        
let expectsToBeTrue result =
    if result then TestSuccess
    else
        "expected true and got false"
        |> VerificationFailure
        |> TestFailure