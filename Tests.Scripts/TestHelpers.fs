[<AutoOpen>]
module Archer.Tests.Scripts.TestHelpers

open Archer
open Archer.Bow
open Archer.MicroLang

let buildTestFramework (testAction: 'a -> RunnerEnvironment -> TestResult) (setup: unit -> Result<'a, SetupTeardownFailure>) (teardown: Result<'a, SetupTeardownFailure> -> TestResult option -> Result<unit, SetupTeardownFailure>) =
    let framework = bow.Framework ()
    let container = suite.Container (ignoreString (), ignoreString ())
    let test = container.Test (SetupPart setup, testAction, TeardownPart teardown)
    
    (framework.AddTests [test]), test
    
let buildBasicFramework () = buildTestFramework successfulEnvironmentTest successfulUnitSetup successfulTeardown
