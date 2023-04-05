module Archer.Tests.Scripts.``TestExecutionStarted Event``

open Archer.Bow.Lib
open Archer.CoreTypes
open Archer.Tests.Scripts.TestLang

let private defaultSeed = 33
let private getDefaultSeed () = defaultSeed

let private container = suite.Container ("", "TestExecutionStarted Event should")

let ``Test Cases`` = [
    container.Test ("be raised from the given test when framework is run", fun () ->
         let framework, test = buildTestFramework None None

         let mutable result = "Not Called" |> GeneralFailure |> TestFailure
         
         framework.TestStartExecution.AddHandler (fun fr args ->
                 let r =
                     if fr = framework then TestSuccess
                     else
                         $"expected\n%A{fr}\nto be\n%A{framework}"
                         |> VerificationFailure
                         |> TestFailure
                     
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