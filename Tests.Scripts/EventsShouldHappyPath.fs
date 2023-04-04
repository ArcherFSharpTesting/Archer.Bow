module Archer.Tests.Scripts.``When tests execute normally framework should raise``

open Archer.Bow.Lib
open Archer.CoreTypes.Lib
open Archer.Tests.Scripts.TestLang

let defaultSeed = 33
let getDefaultSeed () = defaultSeed

let private container = suite.Container ("", "Framework Run Should")

let ``the FrameworkExecutionStarted event`` =
    container.Test ("the FrameWorkExecutionStarted event", fun () ->
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

let ``the FrameworkExecutionEnded event`` =
    container.Test ("the FrameworkExecutionEnded event", fun () ->
        let framework = archer.Framework ()

        let mutable result = "Not Called" |> GeneralFailure |> TestFailure
        framework.FrameworkEndExecution.AddHandler (FrameworkDelegate (fun fr _ ->
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

let ``the TestExecutionStarted event`` =
     container.Test ("the TestExecutionStarted event", fun () ->
         let framework = archer.Framework ()
         let c = suite.Container ("Framework Run Should", "the TestExecutionStarted event")
         let test = c.Test ("My Passing Test", fun () -> TestSuccess)
         
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

let ``the TestStartSetup event`` =
     container.Test ("the TestStartSetup event", fun () ->
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