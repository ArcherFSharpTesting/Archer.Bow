[<AutoOpen>]
module Archer.Tests.Scripts.TestHelpers

open Archer.CoreTypes
open Archer.Bow
open Archer.MicroLang.Lang
open Archer.MicroLang.Types

let buildTestFramework (testAction: (unit -> TestResult) option) (parts: TestPart option) =
    let framework = bow.Framework ()
    let test = buildDummyTest testAction parts

    framework.AddTests [test]
    framework, test