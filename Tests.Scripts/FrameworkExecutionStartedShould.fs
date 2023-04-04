module Archer.Tests.Scripts.``FrameworkExecutionStarted should``

open Archer.Bow.Lib
open Archer.CoreTypes.Lib
open Archer.Tests.Scripts.TestLang

let private defaultSeed = 33
let private getDefaultSeed () = defaultSeed

let private container = suite.Container ("", "FrameworkExecutionStarted should")

let ``be raised when framework is run`` =
    container.Test ("be raised when framework is run", fun () ->
        let framework = archer.Framework ()

        let mutable result = "Not Run" |> GeneralFailure |> TestFailure
        framework.FrameworkStartExecution.AddHandler (FrameworkDelegate (fun fr _ ->
            let r = 
                if fr = framework then TestSuccess
                else
                    $"expected\n%A{fr}\nto be\n%A{framework}"
                    |> VerificationFailure
                    |> TestFailure
                    
            result <- r
        ))
        framework.Run getDefaultSeed |> ignore

        result        
    )