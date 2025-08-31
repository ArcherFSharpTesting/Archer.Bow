module Archer.Tests.Scripts.``When running tests that throw exception runner should``

open System
open Archer
open Archer.Core
open Archer.Types.InternalTypes
open Archer.Types.InternalTypes.RunnerTypes
open Archer.Runner

let runnerFactory = RunnerFactory ()

let private feature = FeatureFactory.NewFeature (
    TestTags [
        Category "Runner"
        Category "Exception Handling"
    ]
)

type DummyTestExecutor (parent: ITest, action: RunnerEnvironment -> TestExecutionResult) =
    let dummyEvent = Event<TestExecutionDelegate, TestEventLifecycle> ()
    
    interface ITestExecutor with
        member this.Execute environment = action environment
        member this.Parent = parent
        
        [<CLIEvent>]
        member this.TestLifecycleEvent = dummyEvent.Publish

type DummyTest (containerPath: string, containerName: string, testName: string, action: RunnerEnvironment -> TestExecutionResult, location: CodeLocation) =
    interface ITest with
        member _.ContainerName = containerName
        member _.ContainerPath = containerPath
        member this.GetExecutor() = DummyTestExecutor (this, action)
        member _.Location = location
        member _.Tags = []
        member _.TestName = testName
        
let ``Return ExceptionFailure`` =
    feature.Test (fun () ->
        let expectedException = Exception "Bad Behavior"
        let badThrowingTestAction _ = raise expectedException
        
        let containerPath = "Exceptions.Abound"
        let containerName = "And Abound"
        let badTest = DummyTest (containerPath, containerName,  "Throws Exception", badThrowingTestAction, { FilePath = "BadFilePath"; FileName = "BadFileName.fs"; LineNumber = 32 })
        
        let runner = runnerFactory.Runner ()
        
        let result = 
            runner.AddTests [badTest]
            |> runWithSeed (fun () -> 155)
            
        result
        |> Should.BeEqualTo {
            Successes = []
            Failures = [
                FailContainer (
                    containerPath,
                    [
                        FailContainer (
                            containerName,
                            [FailedTests [expectedException |> TestExceptionFailure |> TestRunFailureType, badTest]]
                        )
                    ]
                )
            ]
            Ignored = []
            Seed = 155
            Began = result.Began
            End = result.End 
        }
    )

let ``Test Cases`` = feature.GetTests ()