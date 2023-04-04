module Archer.Tests.Scripts.``FrameworkExecutionEnded should``

open Archer.Bow.Lib
open Archer.CoreTypes.Lib
open Archer.Tests.Scripts.TestLang

let defaultSeed = 33
let getDefaultSeed () = defaultSeed

let private container = suite.Container ("", "FrameworkExecutionEnded should")

let ``be raised when the framework is run`` =
    container.Test ("be raised when the framework is run", fun () ->
        let framework = archer.Framework ()

        let mutable result = "Not Called" |> GeneralFailure |> TestFailure
        framework.FrameworkEndExecution.AddHandler (fun fr _ ->
            let r =
                if fr = framework then TestSuccess
                else
                    $"expected\n%A{fr}\nto be\n%A{framework}"
                    |> VerificationFailure
                    |> TestFailure
                    
            result <- r
        )
        framework.Run getDefaultSeed |> ignore
        
        result
    )

