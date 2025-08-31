[<AutoOpen>]
module Archer.Tests.Scripts.TestHelpers

open Archer
open Archer.Runner
let runnerFactory = RunnerFactory ()
open Archer.MicroLang

let buildTestRunner (testAction: 'a -> RunnerEnvironment -> TestResult) (setup: unit -> Result<'a, SetupTeardownFailure>) (teardown: Result<'a, SetupTeardownFailure> -> TestResult option -> Result<unit, SetupTeardownFailure>) =
    let runner = runnerFactory.Runner ()
    let container = suite.Container (ignoreString (), ignoreString ())
    let test = container.Test (SetupPart setup, testAction, TeardownPart teardown)
    
    (runner.AddTests [test]), test
    
let buildBasicRunner () = buildTestRunner successfulEnvironmentTest successfulUnitSetup successfulTeardown
