module Archer.Tests.Scripts.``TestStartSetup Event``

open System.ComponentModel
open Archer.Bow
open Archer
open Archer.MicroLang

let private defaultSeed = 33
let private getDefaultSeed () = defaultSeed

let private container = suite.Container ("", "TestStartSetup Event should")

let ``Test Cases`` = [
    container.Test ("be raised from the given test when the framework is run", fun () ->
        let framework, test = buildTestFramework None None

        let mutable result = "Not Called" |> GeneralFailure |> TestFailure

        framework.TestStartSetup.AddHandler (fun fr args ->
             let r =
                 if fr = framework then TestSuccess
                 else
                     fr
                     |> expectsToBe framework
                 
             result <- args.Test
                       |> expectsToBe test
                       |> combineError r
         )

        getDefaultSeed
        |> framework.Run
        |> ignore

        result
    )
    
    container.Test ("not be raised if FrameworkExecutionStarted was canceled", fun () ->
        let framework, _ = buildTestFramework None None
         
        framework.FrameworkStartExecution.AddHandler (fun _ (args: CancelEventArgs) ->
            args.Cancel <- true
        )
        
        let mutable result = TestSuccess
        framework.TestStartExecution.AddHandler (fun _ _ ->
            result <- notRunValidationFailure
        )
        
        framework.Run () |> ignore
        
        result
    )
]