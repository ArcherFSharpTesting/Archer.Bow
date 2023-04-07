module Archer.Tests.Scripts.``FrameworkExecutionEnded Event``

open Archer.Bow
open Archer.CoreTypes
open Archer.Tests.Scripts.TestLang

let private defaultSeed = 33
let private getDefaultSeed () = defaultSeed

let private container = suite.Container ("", "FrameworkExecutionEnded Event should")

let ``Test Cases`` = [
    container.Test ("be raised when the framework is run", fun () ->
        let framework = bow.Framework ()

        let mutable result = "Not Called" |> GeneralFailure |> TestFailure
        framework.FrameworkEndExecution.AddHandler (fun fr _ ->
            let r =
                if fr = framework then TestSuccess
                else
                    fr
                    |> expectsToBe framework
                    
            result <- r
        )
        framework.Run getDefaultSeed |> ignore
        
        result
    )
]