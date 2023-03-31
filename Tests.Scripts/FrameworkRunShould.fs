module Archer.Tests.Scripts.``Framework Run Should``

open Archer.Bow.Lib
open Archer.CoreTypes.Lib
open Archer.Tests.Scripts.TestLang
let ``return empty results when it has no tests`` =
    suite.Container ("", "Framework Run Should")
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
                
                if expected = result then TestSuccess
                else
                    $"expected \"%A{result}\" to be \"%A{expected}\""
                    |> VerificationFailure
                    |> TestFailure
            )
        )
    
let ``return empty results with specific seed when it has no tests`` =
    suite.Container ("", "Framework Run Should")
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
                
                if expected = result then TestSuccess
                else
                    $"expected \"%A{result}\" to be \"%A{expected}\""
                    |> VerificationFailure
                    |> TestFailure
            )
        )