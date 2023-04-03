module Archer.Tests.Scripts.``When tests execute normally framework should raise``

open Archer.Bow.Lib
open Archer.CoreTypes.Lib
open Archer.CoreTypes.Lib.InternalTypes
open Archer.Tests.Scripts.TestLang

let defaultSeed = 33
let getDefaultSeed () = defaultSeed

let private container = suite.Container ("", "Framework Run Should")

let ``the FrameworkExecutionStarted event`` =
    container.Test ("the FrameWorkExecutionStarted event", fun () ->
        let framework = archer.Framework ()

        let mutable result = "Not Run" |> GeneralFailure |> TestFailure
        framework.FrameworkExecutionStarted.AddHandler (FrameworkDelegate (fun fr _ ->
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
        framework.FrameworkExecutionEnded.AddHandler (FrameworkDelegate (fun fr _ ->
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

// let ``the TestExecutionStarted event`` =
//     container.Test ("the TestExecutionStarted event", fun () ->
//         let framework = archer.Framework ()
//         let test = container.Test ("My Passing Test", fun () -> TestSuccess)
//         framework.AddTests [test]
//
//         let mutable result = "Not Called" |> GeneralFailure |> TestFailure
//         
//         framework.TestExecutionStarted.AddHandler (fun fr _ ->
//                 let r =
//                     if fr = framework then TestSuccess
//                     else
//                         $"expected\n%A{fr}\nto be\n%A{framework}"
//                         |> VerificationFailure
//                         |> TestFailure
//                     
//                 result <- r
//             )
//         
//         getDefaultSeed
//         |> framework.Run
//         |> ignore
//         
//         result
//     )