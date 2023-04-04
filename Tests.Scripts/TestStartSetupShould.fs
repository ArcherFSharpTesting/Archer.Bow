module Archer.Tests.Scripts.``TestStartSetup should``

open Archer.Bow.Lib
open Archer.CoreTypes.Lib
open Archer.Tests.Scripts.TestLang

let private defaultSeed = 33
let private getDefaultSeed () = defaultSeed

let private container = suite.Container ("", "TestStartSetup should")

let ``be raised from the given test when the framework is run`` =
     container.Test ("be raised from the given test when the framework is run", fun () ->
         let framework = archer.Framework ()
         let c = suite.Container ("Framework Run Should", "the TestExecutionStarted event")
         let test = c.Test ("My Passing Test", fun () -> TestSuccess)
         
         framework.AddTests [test]

         let mutable result = "Not Called" |> GeneralFailure |> TestFailure
         
         framework.TestStartSetup.AddHandler (fun fr args ->
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