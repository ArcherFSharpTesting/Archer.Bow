module Archer.Tests.Scripts.``Framework Run Should``

open System
open Archer.Arrows
open Archer.Bow
open Archer
open Archer.MicroLang
open Archer.CoreTypes.InternalTypes.FrameworkTypes

let private defaultSeed = 42
let private getDefaultSeed () = defaultSeed

let private feature = Arrow.NewFeature ()

let ``return empty results when it has no tests`` =
    feature.Test (fun _ ->
        let seed = 5
                
        let framework = bow.Framework ()
        let result = framework.Run (fun () -> seed)
                
        let expected = {
            Failures = []
            Successes = []
            Ignored = [] 
            Seed = seed
        }
                
        result |> Should.BeEqualTo expected
    )
    
let ``return empty results with different seed when it has no tests and provided a different seed`` =
    feature.Test (fun _ ->
        let seed = 258
                
        let framework = bow.Framework ()
        let result = framework.Run (fun () -> seed)
                
        let expected = {
            Failures = []
            Successes = []
            Ignored = [] 
            Seed = seed
        }
                
        result |> Should.BeEqualTo expected
    )
    
let ``return a successful result when one test passes`` =
    feature.Test (fun _ ->
        let framework = bow.Framework ()
        let containerPath = "A Test Suite"
        let containerName = "with a passing test"
        let container = suite.Container (containerPath, containerName)
        let test = container.Test ("A Passing Test", fun _ -> TestSuccess)

        
        let result =
            framework.AddTests [test]
            |> runWithSeed getDefaultSeed
        
        let expected = {
            Failures = []
            Successes = [
                SuccessContainer (containerPath, [
                    SuccessContainer (containerName, [SucceededTests [test]])
                ])
            ]
            Ignored = [] 
            Seed = defaultSeed
        }

        result |> Should.BeEqualTo expected
    )
    
let ``return a successful result when two tests pass`` =
    feature.Test (fun _ ->
        let framework = bow.Framework ()
        let containerPath = "A test Suite"
        let containerName = "with two passing tests"
        let container = suite.Container (containerPath, containerName)
        
        let test1 = container.Test ("Fist Passing Test", fun _ -> TestSuccess)
        let test2 = container.Test ("Second Passing Test", fun _ -> TestSuccess)

        let expected = {
                Failures = []
                Successes = [
                    SuccessContainer (containerPath, [
                        SuccessContainer (containerName, [SucceededTests [test1; test2]])
                    ])
                ]
                Ignored = [] 
                Seed = defaultSeed
            }

        
        let result =
            framework.AddTests [test1; test2]
            |> runWithSeed getDefaultSeed

        result |> Should.BeEqualTo expected
    )
    
let ``return failure when a test fails`` =
    feature.Test ("", fun _ -> 
        let framework = bow.Framework ()
        let containerPath = "A test Suite"
        let containerName = "to hold tests"
        let container = suite.Container (containerPath, containerName)

        let failure = "Boom" |> newFailure.With.TestOtherExpectationFailure
        let testF = container.Test ("First Test Fails", fun _ -> failure |> TestFailure)
        let test2 = container.Test ("Second Test Passes", fun _ -> TestSuccess)

        let expected = {
                Failures = [
                    FailContainer (containerPath, [
                        FailContainer (containerName, [FailedTests [failure |> TestRunFailureType, testF]])
                    ])
                ]
                Successes = [
                    SuccessContainer (containerPath, [
                        SuccessContainer (containerName, [SucceededTests [test2]])
                    ])
                ]
                Ignored = [] 
                Seed = defaultSeed
            }

        let result =
            framework.AddTests [testF; test2]
            |> runWithSeed getDefaultSeed

        result |> Should.BeEqualTo expected
    )
    
let ``return failure when second test fails`` =
    feature.Test (fun _ -> 
        let framework = bow.Framework ()
        let containerPath = "A test Suite"
        let containerName = "to hold tests"
        let container = suite.Container (containerPath, containerName)

        let failure = "Boom Again" |> newFailure.With.TestOtherExpectationFailure
        let test1 = container.Test ("First Test Passes", fun _ -> TestSuccess)
        let testF = container.Test ("Second Test Fails", fun _ -> failure |> TestFailure)

        let expected = {
                Failures = [
                    FailContainer (containerPath, [
                        FailContainer (containerName, [FailedTests [failure |> TestRunFailureType, testF]])
                    ])
                ]
                Successes = [
                    SuccessContainer (containerPath, [
                        SuccessContainer (containerName, [SucceededTests [test1]])
                    ])
                ]
                Ignored = [] 
                Seed = defaultSeed
            }

        let result =
            framework.AddTests [test1; testF]
            |> runWithSeed getDefaultSeed

        result |> Should.BeEqualTo expected
    )
    
let ``return failure all second test fail`` =
    feature.Test (fun _ -> 
        let framework = bow.Framework ()
        let containerPath = "A test Suite"
        let containerName = "to hold tests"
        let container = suite.Container (containerPath, containerName)

        let failure1 = "Boom Again" |> newFailure.With.TestOtherExpectationFailure
        let failure2 = newFailure.With.TestExecutionWasNotRunValidationFailure ()
        let testF = container.Test ("Second Test Fails", fun _ -> failure2 |> TestFailure)
        let testF2 = container.Test ("First Test fails", fun _ -> failure1 |> TestFailure)

        let expected = {
                Failures = [
                    FailContainer (containerPath, [
                        FailContainer (containerName,  [FailedTests [failure1 |> TestRunFailureType, testF2; failure2 |> TestRunFailureType, testF]])
                    ])
                ]
                Successes = []
                Ignored = [] 
                Seed = defaultSeed
            }

        let result =
            framework.AddTests [testF2; testF]
            |> runWithSeed getDefaultSeed

        result |> Should.BeEqualTo expected
    )
    
let ``run asynchronously`` =
    feature.Test (fun _ ->
        let monitor = obj ()
        let mutable isRunning = false
        
        let framework = bow.Framework ()
        let random = Random ()
        
        let mutable result = newFailure.With.TestExecutionWasNotRunValidationFailure () |> TestFailure
        
        let run _ =
                
            lock monitor (fun _ -> isRunning <- true)
            System.Threading.Thread.Sleep (random.Next (1, 3) * 100)

            result <-
                match result with
                | TestSuccess -> result
                | _ ->
                    isRunning |> expects.ToBeTrue

            lock monitor (fun _ -> isRunning <- false)
            
            TestSuccess
            
        
        let t1 = feature.Test ("Test A", run)
        let t2 = feature.Test ("Test B", run)
        let t3 = feature.Test ("Test C", run)
        
        framework.AddTests [
            t1
            t2
            t3
        ]
        |> run
        |> ignore
        
        result
    )
    
let ``run a test with the correct framework name`` =
    feature.Test(fun _ env ->
        env.FrameworkEnvironment.FrameworkName
        |> expects.ToBe "Archer.Bow"
    )
    
let ``run a test with the correct framework version`` =
    feature.Test(fun _ env ->
        let typeBow = bow.GetType ()
        let version = typeBow.Assembly.GetName().Version
        
        env.FrameworkEnvironment.FrameworkVersion
        |> expects.ToBe version
    )
    
let ``run a test with the correct test info`` =
    feature.Test(fun _ ->
        let containerPath = "The Path"
        let containerName = "My new container"
        let testName = "Testing the test info"
        let c = suite.Container (containerPath, containerName)
        
        let test = c.Test (testName, fun e ->
            let info = e.TestInfo
            
            let pathResult =
                info.ContainerPath
                |> expects.ToBe containerPath
                
            let containerNameResult =
                info.ContainerName
                |> expects.ToBe containerName
                
            let testNameResult =
                info.TestName
                |> expects.ToBe testName
                
            pathResult
            |> andResult containerNameResult
            |> andResult testNameResult
        )
        
        let framework = bow.Framework ()
        
        
        let result =
            framework.AddTests [test]
            |> run
        
        let a = 
            result.Failures
            |> expects.ToBe [] 
            |> withMessage "Failures"
            
        let b =
            result.Successes
            |> expects.ToBe [
                SuccessContainer (
                    containerPath,
                    [SuccessContainer (containerName, [SucceededTests [test]])]
                )
            ]
            
        a
        |> andResult b
    )
    
let ``Test Cases`` = feature.GetTests ()