module Archer.Tests.Scripts.``Runner Run Should``

open System
open Archer.Arrows
open Archer.Bow
open Archer
open Archer.CoreTypes.InternalTypes
open Archer.MicroLang
open Archer.CoreTypes.InternalTypes.RunnerTypes

let private defaultSeed = 42
let private getDefaultSeed () = defaultSeed

let private feature = Arrow.NewFeature (
    TestTags [
        Category "Runner"
    ]
)

let ``return empty results when it has no tests`` =
    feature.Test (fun () ->
        let seed = 5
                
        let runner = bow.Runner ()
        let result = runner.Run (fun () -> seed)
                
        let expected = {
            Failures = []
            Successes = []
            Ignored = [] 
            Seed = seed
            Began = result.Began
            End = result.End
        }
                
        result |> Should.BeEqualTo expected
    )
    
let ``return empty results with different seed when it has no tests and provided a different seed`` =
    feature.Test (fun () ->
        let seed = 258
                
        let runner = bow.Runner ()
        let result = runner.Run (fun () -> seed)
                
        let expected = {
            Failures = []
            Successes = []
            Ignored = [] 
            Seed = seed
            Began = result.Began
            End = result.End
        }
                
        result |> Should.BeEqualTo expected
    )
    
let ``return a successful result when one test passes`` =
    feature.Test (fun () ->
        let runner = bow.Runner ()
        let containerPath = "A Test Suite"
        let containerName = "with a passing test"
        let testFeature = Arrow.NewFeature (containerPath, containerName)
        let test = testFeature.Test ("A Passing Test", fun () -> TestSuccess)

        
        let result =
            runner.AddTests [test]
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
            Began = result.Began
            End = result.End
        }

        result |> Should.BeEqualTo expected
    )
    
let ``return a successful result when two tests pass`` =
    feature.Test (fun () ->
        let runner = bow.Runner ()
        let containerPath = "A test Suite"
        let containerName = "with two passing tests"
        let testFeature = Arrow.NewFeature (containerPath, containerName)
        
        let test1 = testFeature.Test ("Fist Passing Test", fun () -> TestSuccess)
        let test2 = testFeature.Test ("Second Passing Test", fun () -> TestSuccess)
        
        let result =
            runner.AddTests [test1; test2]
            |> runWithSeed getDefaultSeed

        let expected = {
                Failures = []
                Successes = [
                    SuccessContainer (containerPath, [
                        SuccessContainer (containerName, [SucceededTests [test1; test2]])
                    ])
                ]
                Ignored = [] 
                Seed = defaultSeed
                Began = result.Began
                End = result.End
            }
        
        result |> Should.BeEqualTo expected
    )
    
let ``return failure when a test fails`` =
    feature.Test (fun () -> 
        let runner = bow.Runner ()
        let containerPath = "A test Suite"
        let containerName = "to hold tests"
        let container = suite.Container (containerPath, containerName)

        let failure = "Boom" |> newFailure.With.TestOtherExpectationFailure
        let testF = container.Test ("First Test Fails", (fun _ -> failure |> TestFailure))
        let test2 = container.Test ("Second Test Passes", (fun _ -> TestSuccess))

        let result =
            runner.AddTests [testF; test2]
            |> runWithSeed getDefaultSeed

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
                Began = result.Began
                End = result.End
            }

        result |> Should.BeEqualTo expected
    )
    
let ``return failure when second test fails`` =
    feature.Test (fun () -> 
        let runner = bow.Runner ()
        let containerPath = "A test Suite"
        let containerName = "to hold tests"
        let container = suite.Container (containerPath, containerName)

        let failure = "Boom Again" |> newFailure.With.TestOtherExpectationFailure
        let test1 = container.Test ("First Test Passes", fun _ -> TestSuccess)
        let testF = container.Test ("Second Test Fails", fun _ -> failure |> TestFailure)

        let result =
            runner.AddTests [test1; testF]
            |> runWithSeed getDefaultSeed

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
                Began = result.Began
                End = result.End
            }

        result |> Should.BeEqualTo expected
    )
    
let ``return failure all second test fail`` =
    feature.Test (fun () -> 
        let runner = bow.Runner ()
        let containerPath = "A test Suite"
        let containerName = "to hold tests"
        let testFeature = Arrow.NewFeature (containerPath, containerName)

        let failure1 = "Boom Again" |> newFailure.With.TestOtherExpectationFailure
        let failure2 = newFailure.With.TestExecutionWasNotRunValidationFailure ()
        let testF = testFeature.Test ("Second Test Fails", fun () -> failure2 |> TestFailure)
        let testF2 = testFeature.Test ("First Test fails", fun () -> failure1 |> TestFailure)

        let result =
            runner.AddTests [testF2; testF]
            |> runWithSeed getDefaultSeed

        let expected = {
                Failures = [
                    FailContainer (containerPath, [
                        FailContainer (containerName,  [FailedTests [failure1 |> TestRunFailureType, testF2; failure2 |> TestRunFailureType, testF]])
                    ])
                ]
                Successes = []
                Ignored = [] 
                Seed = defaultSeed
                Began = result.Began
                End = result.End
            }

        result |> Should.BeEqualTo expected
    )
    
let ``run asynchronously`` =
    feature.Test (fun () ->
        let monitor = obj ()
        let mutable isRunning = false
        
        let runner = bow.Runner ()
        let random = Random ()
        
        let mutable result = newFailure.With.TestExecutionWasNotRunValidationFailure () |> TestFailure
        
        let testFunction () =
                
            lock monitor (fun () -> isRunning <- true)
            System.Threading.Thread.Sleep (random.Next (1, 3) * 100)

            result <-
                match result with
                | TestSuccess -> result
                | _ ->
                    isRunning |> expects.ToBeTrue

            lock monitor (fun () -> isRunning <- false)
            
            TestSuccess
            
        let testFeature = Arrow.NewFeature ()
        let t1 = testFeature.Test ("Test A", testFunction)
        let t2 = testFeature.Test ("Test B", testFunction)
        let t3 = testFeature.Test ("Test C", testFunction)
        
        runner.AddTests [
            t1
            t2
            t3
        ]
        |> run
        |> ignore
        
        result
    )
    
let ``run a test with the correct runner name`` =
    feature.Test(fun () env ->
        env.RunEnvironment.RunnerName
        |> expects.ToBe "Archer.Bow"
    )
    
let ``run a test with the correct runner version`` =
    feature.Test(fun () env ->
        let typeBow = bow.GetType ()
        let version = typeBow.Assembly.GetName().Version
        
        env.RunEnvironment.RunnerVersion
        |> expects.ToBe version
    )
    
let ``run a test with the correct test info`` =
    feature.Test(fun () ->
        let containerPath = "The Path"
        let containerName = "My new container"
        let testName = "Testing the test info"
        let c = Arrow.NewFeature (containerPath, containerName)
        
        let test = c.Test (testName, fun () e ->
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
        
        let runner = bow.Runner ()
        
        let result =
            runner.AddTests [test]
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
    
let ``run only tests marked with Only Tag if no filter is given`` =
    feature.Test (fun () ->
        let runner = bow.Runner ()
        
        let testFeature = Arrow.NewFeature ("a", "b")
        
        let _a = testFeature.Test ("Successful test A", fun () -> TestSuccess)
        let b = testFeature.Test ("Successful test B", TestTags [Only],fun () -> TestSuccess)
        let _c = testFeature.Test ("Successful test C", fun () -> TestSuccess)
        let d = testFeature.Test ("Successful test D", TestTags [Only],fun () -> TestSuccess)
        
        testFeature.GetTests () |> runner.AddTests  |> ignore
        let result = runner.Run()
        
        let contains (target: ITest) (containers: TestSuccessContainer list) =
            let rec contains (containers: TestSuccessContainer list) =
                match containers with
                | [] -> false
                | head::tail ->
                    match head with
                    | EmptySuccesses -> contains tail
                    | SucceededTests tests ->
                        let a = tests |> List.contains target
                        if a then a
                        else contains tail
                    | SuccessContainer (_, testSuccessContainers) ->
                        let a = contains testSuccessContainers
                        if a then a
                        else contains tail
                        
            contains containers
            
        let getNumberOfTests (containers: TestSuccessContainer list) =
            let rec getLength (containers: TestSuccessContainer list) acc =
                match containers with
                | [] -> acc
                | head::tail ->
                    match head with
                    | EmptySuccesses -> acc |> getLength tail
                    | SucceededTests tests -> (tests.Length + acc) |> getLength tail
                    | SuccessContainer (_, items) ->
                        let a = getLength items acc
                        a |> getLength tail
                        
            getLength containers 0
        
        result.Successes
        |> Should.PassAllOf [
            getNumberOfTests >> Should.BeEqualTo 2 >> withMessage "Not correct number of results"
            contains b >> Should.BeTrue >> withMessage $"Missing %s{b.ToString ()}"
            contains d >> Should.BeTrue >> withMessage $"Missing %s{d.ToString ()}"
        ]
    )
    
let ``Test Cases`` = feature.GetTests ()