module Archer.Tests.Scripts.``Framework Run Should``

open Archer.Bow.Lib
open Archer.CoreTypes.Lib
open Archer.Tests.Scripts.TestLang

let defaultSeed = 42
let getDefaultSeed () = defaultSeed

let private container = suite.Container ("", "Framework Run Should")

let verifyWith expected result =
    if expected = result then TestSuccess
    else
        $"expected \"%A{result}\" to be \"%A{expected}\""
        |> VerificationFailure
        |> TestFailure

let ``return empty results when it has no tests`` =
    container
    |> newTest (fun container ->
            container.Test ("return empty results when it has no tests", fun () ->
                let seed = 5
                
                let framework = archer.Framework ()
                let result = framework.Run (fun () -> seed)
                
                let expected = {
                    Failures = []
                    Successes = []
                    Seed = seed
                }
                
                result |> verifyWith expected
            )
        )
    
let ``return empty results with specific seed when it has no tests`` =
    container
    |> newTest (fun container ->
            container.Test ("return empty results when it has no tests", fun () ->
                let seed = 258
                
                let framework = archer.Framework ()
                let result = framework.Run (fun () -> seed)
                
                let expected = {
                    Failures = []
                    Successes = []
                    Seed = seed
                }
                
                result |> verifyWith expected
            )
        )

let ``return a successful result when one test passes`` =
    container
    |> newTest (fun container -> container.Test ("return a successful result when one test passes", fun () ->
            let framework = archer.Framework ()
            let container = suite.Container ("A Test Suite", "with a passing test")
            let test = container.Test ("A Passing Test", fun () -> TestSuccess)

            framework.AddTest test
            let result = framework.Run (getDefaultSeed)
        
            let expected = {
                Failures = []
                Successes = [test]
                Seed = defaultSeed
            }

            result |> verifyWith expected
        )
    )

let ``return a successful result when two tests pass`` =
    container.Test ("return a successful result when two tests pass", fun () ->
        let framework = archer.Framework ()
        let container = suite.Container ("A test Suite", "with two passing tests")
        
        let test1 = container.Test ("Fist Passing Test", fun () -> TestSuccess)
        let test2 = container.Test ("Second Passing Test", fun () -> TestSuccess)

        let expected = {
                Failures = []
                Successes = [test1; test2]
                Seed = defaultSeed
            }

        framework.AddTest test1
        framework.AddTest test2
        let result = framework.Run (getDefaultSeed)

        result |> verifyWith expected
    )