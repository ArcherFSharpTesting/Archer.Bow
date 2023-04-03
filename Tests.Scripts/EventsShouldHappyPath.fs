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

        let mutable called = false
        framework.FrameworkExecutionStarted.Add (fun _ -> called <- true)
        framework.Run getDefaultSeed |> ignore
        
        called
        |> expectsToBeTrue
    )

let ``the FrameworkExecutionEnded event`` =
    container.Test ("the FrameworkExecutionEnded event", fun () ->
        let framework = archer.Framework ()

        let mutable called = false
        framework.FrameworkExecutionEnded.Add (fun _ -> called <- true)
        framework.Run getDefaultSeed |> ignore
        
        called
        |> expectsToBeTrue
    )