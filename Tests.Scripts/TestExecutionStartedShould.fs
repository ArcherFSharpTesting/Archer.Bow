module Archer.Tests.Scripts.``TestExecutionStarted Event``

open Archer.Bow
open Archer
open Archer.MicroLang

let private defaultSeed = 33
let private getDefaultSeed () = defaultSeed

let private container = suite.Container ("", "TestExecutionStarted Event should")

let ``Test Cases`` = [
    container.Test ("be raised from the given test when framework is run", fun _ ->
         let framework, test = buildTestFramework None None

         let mutable result = "Not Called" |> GeneralFailure |> TestFailure
         
         framework.TestStartExecution.AddHandler (fun fr args ->
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
]