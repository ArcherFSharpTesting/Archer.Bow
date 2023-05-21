module Archer.Tests.Scripts.``RunnerExecutionEnded Event should``

open Archer.Arrows
open Archer.Bow
open Archer
open Archer.CoreTypes.InternalTypes.RunnerTypes
open Archer.MicroLang

let private defaultSeed = 33
let private getDefaultSeed () = defaultSeed

let private feature = Arrow.NewFeature (
    TestTags [
        Category "Runner"
        Category "RunnerLifecycleEvent"
    ]
)

let ``be raised when the runner is run`` =
    feature.Test (fun () ->
        let runner = bow.Runner ()

        let mutable result = "Not Called" |> newFailure.With.TestOtherExpectationFailure |> TestFailure
        
        runner.RunnerLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | RunnerEndExecution -> true
            | _ -> false
        )
        |> Event.add (fun _ ->
            result <- TestSuccess
        )
        
        runner.Run getDefaultSeed |> ignore
        
        result
    )
    
let ``Test Cases`` = feature.GetTests ()