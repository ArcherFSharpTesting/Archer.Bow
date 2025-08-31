module Archer.Tests.Scripts.``RunnerExecutionStarted Event should``

open Archer.Arrows
open Archer.Runner
let runnerFactory = RunnerFactory ()
open Archer
open Archer.Types.InternalTypes.RunnerTypes
open Archer.MicroLang

let private defaultSeed = 33
let private getDefaultSeed () = defaultSeed

let private feature = Arrow.NewFeature (
    TestTags [
        Category "Runner"
        Category "RunnerLifecycleEvent"
    ]
)

let ``be raised when runner is run`` =
    feature.Test (fun () ->
    let runner = runnerFactory.Runner ()

        let mutable result = "Not Run" |> newFailure.With.TestOtherExpectationFailure |> TestFailure
        
        runner.RunnerLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | RunnerStartExecution _ -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | RunnerStartExecution _ ->
                result <- TestSuccess
            | _ -> ()
        ) 
        
        runner.Run getDefaultSeed |> ignore

        result        
    )
    
let ``Test Cases`` = feature.GetTests ()