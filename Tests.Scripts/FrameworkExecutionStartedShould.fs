module Archer.Tests.Scripts.``FrameworkExecutionStarted Event``

open Archer.Bow
open Archer.CoreTypes
open Archer.MicroLang

let private defaultSeed = 33
let private getDefaultSeed () = defaultSeed

let private container = suite.Container ("", "FrameworkExecutionStarted Event should")

let ``Test Cases`` = [
    container.Test ("be raised when framework is run", fun () ->
        let framework = bow.Framework ()

        let mutable result = "Not Run" |> GeneralFailure |> TestFailure
        framework.FrameworkStartExecution.AddHandler (fun fr _ ->
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