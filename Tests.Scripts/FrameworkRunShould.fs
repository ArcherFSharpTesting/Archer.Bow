module Archer.Tests.Scripts.``Framework Run``

open System
open Archer.Bow
open Archer
open Archer.MicroLang

let private defaultSeed = 42
let private getDefaultSeed () = defaultSeed

let private container = suite.Container ("", "Framework Run Should")

let ``Test Cases`` = [
    container.Test ("return empty results when it has no tests", fun () ->
        let seed = 5
                
        let framework = bow.Framework ()
        let result = framework.Run (fun () -> seed)
                
        let expected = {
            Failures = []
            Successes = []
            Seed = seed
        }
                
        result |> verifyWith expected
    )
    
    container.Test ("return empty results when it has no tests", fun () ->
        let seed = 258
                
        let framework = bow.Framework ()
        let result = framework.Run (fun () -> seed)
                
        let expected = {
            Failures = []
            Successes = []
            Seed = seed
        }
                
        result |> verifyWith expected
    )
    
    container.Test ("return a successful result when one test passes", fun () ->
        let framework = bow.Framework ()
        let container = suite.Container ("A Test Suite", "with a passing test")
        let test = container.Test ("A Passing Test", fun () -> TestSuccess)

        framework.AddTests [test]
        let result = framework.Run getDefaultSeed
        
        let expected = {
            Failures = []
            Successes = [test]
            Seed = defaultSeed
        }

        result |> verifyWith expected
    )
    
    container.Test ("return a successful result when two tests pass", fun () ->
        let framework = bow.Framework ()
        let container = suite.Container ("A test Suite", "with two passing tests")
        
        let test1 = container.Test ("Fist Passing Test", fun () -> TestSuccess)
        let test2 = container.Test ("Second Passing Test", fun () -> TestSuccess)

        let expected = {
                Failures = []
                Successes = [test1; test2]
                Seed = defaultSeed
            }

        framework.AddTests [test1; test2]
        let result = framework.Run getDefaultSeed

        result |> verifyWith expected
    )
    
    container.Test ("return failure when a test fails", fun () -> 
        let framework = bow.Framework ()
        let container = suite.Container ("A test Suite", "to hold tests")

        let failure = "Boom" |> GeneralFailure
        let testF = container.Test ("First Test Fails", fun () -> failure |> TestFailure)
        let test2 = container.Test ("Second Test Passes", fun () -> TestSuccess)

        let expected = {
                Failures = [failure, testF]
                Successes = [test2]
                Seed = defaultSeed
            }

        framework.AddTests [testF; test2]

        let result = framework.Run getDefaultSeed

        result |> verifyWith expected
    )
    
    container.Test ("return failure when second test fails", fun () -> 
        let framework = bow.Framework ()
        let container = suite.Container ("A test Suite", "to hold tests")

        let failure = "Boom Again" |> GeneralFailure
        let test1 = container.Test ("First Test Passes", fun () -> TestSuccess)
        let testF = container.Test ("Second Test Fails", fun () -> failure |> TestFailure)

        let expected = {
                Failures = [failure, testF]
                Successes = [test1]
                Seed = defaultSeed
            }

        framework.AddTests [test1; testF]

        let result = framework.Run getDefaultSeed

        result |> verifyWith expected
    )
    
    container.Test ("return failure when second test fails", fun () -> 
        let framework = bow.Framework ()
        let container = suite.Container ("A test Suite", "to hold tests")

        let failure1 = "Boom Again" |> GeneralFailure
        let failure2 = notRunExpectation
        let testF = container.Test ("Second Test Fails", fun () -> failure2 |> TestFailure)
        let testF2 = container.Test ("First Test fails", fun () -> failure1 |> TestFailure)

        let expected = {
                Failures = [failure1, testF2; failure2, testF]
                Successes = []
                Seed = defaultSeed
            }

        framework.AddTests [testF2; testF]

        let result = framework.Run getDefaultSeed

        result |> verifyWith expected
    )
    
    container.Test ("shuffle the order of the tests", fun () ->
        let framework = bow.Framework ()
        
        let container = suite.Container ("Framework Run", "shuffle the order of the tests")
        let results = System.Collections.Generic.List<string> ()
        
        framework.AddTests [
            container.Test ("Test A", successfulTest)
            container.Test ("Test B", successfulTest)
            container.Test ("Test C", successfulTest)
        ]
        
        framework.TestStart.Add (fun ars ->
            results.Add ars.Test.TestName
        )
        
        (fun () -> 1073633209)
        |> framework.Run
        |> ignore
        
        results
        |> List.ofSeq
        |> expectsToBe [
            "Test C"
            "Test A"
            "Test B"
        ]
        |> ignore
        
        "Async Breaks this"
        |> Some
        |> IgnoredFailure
        |> TestFailure
    )
    
    container.Test ("shuffle the order of the tests different seed", fun () ->
        let framework = bow.Framework ()
        
        let container = suite.Container ("Framework Run", "shuffle the order of the tests")
        let results = System.Collections.Generic.List<string> ()
        
        framework.AddTests [
            container.Test ("Test A", successfulTest)
            container.Test ("Test B", successfulTest)
            container.Test ("Test C", successfulTest)
        ]
        
        framework.TestStart.Add (fun ars ->
            results.Add ars.Test.TestName
        )
        
        (fun () -> 4006)
        |> framework.Run
        |> ignore
        
        results
        |> List.ofSeq
        |> expectsToBe [
            "Test B"
            "Test C"
            "Test A"
        ]
        |> ignore
        
        "Async Breaks this"
        |> Some
        |> IgnoredFailure
        |> TestFailure
    )
    
    container.Test ("run asynchronously", fun () ->
        let monitor = obj ()
        let mutable isRunning = false
        
        let framework = bow.Framework ()
        let random = Random ()
        
        let mutable result = notRunGeneralFailure
        
        let run _ =
                
            lock monitor (fun _ -> isRunning <- true)
            System.Threading.Thread.Sleep (random.Next (1, 3) * 100)

            result <-
                match result with
                | TestSuccess -> result
                | _ -> isRunning |> expectsToBeTrue

            lock monitor (fun _ -> isRunning <- false)
            
            TestSuccess
            
        
        let t1 = container.Test ("Test A", run)
        let t2 = container.Test ("Test B", run)
        let t3 = container.Test ("Test C", run)
        
        framework.AddTests [
            t1
            t2
            t3
        ]
        
        ()
        |> framework.Run
        |> ignore
        
        result
    )
]