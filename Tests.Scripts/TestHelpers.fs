[<AutoOpen>]
module Archer.Tests.Scripts.TestHelpers

open Archer
open Archer.Bow
open Archer.MicroLang
open Archer.MicroLang.Types

let buildTestFramework (testAction: (unit -> TestResult) option) (parts: TestPart option) =
    let framework = bow.Framework ()
    let test = buildDummyTest testAction parts

    framework.AddTests [test]
    framework, test