module Archer.Tests.Scripts.``When running tests that throw exception framework should``

open System
open Archer
open Archer.Arrows
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.RunnerTypes
open Archer.Bow.Values

let private container = Arrow.NewFeature ()

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
    container.Test (fun _ ->
        let expectedException = Exception "Bad Behavior"
        let badThrowingTestAction _ = raise expectedException
        
        let containerPath = "Exceptions.Abound"
        let containerName = "And Abound"
        let badTest = DummyTest (containerPath, containerName,  "Throws Exception", badThrowingTestAction, { FilePath = "BadFilePath"; FileName = "BadFileName.fs"; LineNumber = 32 })
        
        let framework = bow.Framework ()
        
        framework.AddTests [badTest]
        |> runWithSeed (fun () -> 155)
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
        }
    )

let ``Test Cases`` = container.GetTests ()