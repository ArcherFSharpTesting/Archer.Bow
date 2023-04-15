[<AutoOpen>]
module Archer.Tests.Scripts.TestHelpers

open Archer
open Archer.Bow
open Archer.MicroLang
open Archer.MicroLang.Types

let buildTestFramework (testAction: 'a -> FrameworkEnvironment -> TestResult) (setup: unit -> Result<'a, SetupTeardownFailure>) (teardown: Result<'a, SetupTeardownFailure> -> TestResult option -> Result<unit, SetupTeardownFailure>) =
    let framework = bow.Framework ()
    let container = suite.Container (ignoreString (), ignoreString ())
    let test = container.Test (Setup setup, testAction, Teardown teardown)
    
    (framework.AddTests [test]), test
    
let buildBasicFramework () = buildTestFramework successfulEnvironmentTest successfulUnitSetup successfulTeardown
