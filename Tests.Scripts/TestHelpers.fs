[<AutoOpen>]
module Archer.Tests.Scripts.TestHelpers

open Archer
open Archer.Bow
open Archer.MicroLang

let buildTestRunner (testAction: 'a -> RunnerEnvironment -> TestResult) (setup: unit -> Result<'a, SetupTeardownFailure>) (teardown: Result<'a, SetupTeardownFailure> -> TestResult option -> Result<unit, SetupTeardownFailure>) =
    let runner = bow.Runner ()
    let container = suite.Container (ignoreString (), ignoreString ())
    let test = container.Test (SetupPart setup, testAction, TeardownPart teardown)
    
    (runner.AddTests [test]), test
    
let buildBasicRunner () = buildTestRunner successfulEnvironmentTest successfulUnitSetup successfulTeardown
