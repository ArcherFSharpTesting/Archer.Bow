module Archer.Tests.Scripts.``TestExecutionStarted should``

open System.ComponentModel
open Archer.Bow.Lib
open Archer.CoreTypes.Lib
open Archer.Tests.Scripts.TestLang

let private defaultSeed = 33
let private getDefaultSeed () = defaultSeed

let private container = suite.Container ("", "TestExecutionStarted should")

let ``be raised from the given test when framework is run`` =
     container.Test ("be raised from the given test when framework is run", fun () ->
         let framework = archer.Framework ()
         let test = dummyTest None None
         
         framework.AddTests [test]

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
     
let ``not be raised if FrameworkExecutionStarted was canceled`` =
    container.Test ("not be raised if FrameworkExecutionStarted was canceled", fun () ->
        let framework = archer.Framework ()
        let test = dummyTest None None
         
        framework.AddTests [test]
         
        framework.FrameworkStartExecution.AddHandler (fun _ (args: CancelEventArgs) ->
            args.Cancel <- true
        )
        
        let mutable result = TestSuccess
        framework.TestStartExecution.AddHandler (fun _ _ ->
            result <- "Event Raised when it was not supposed to be" |> VerificationFailure |> TestFailure
        )
        
        framework.Run () |> ignore
        
        result
    )