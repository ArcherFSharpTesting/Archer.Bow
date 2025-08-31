module Archer.Tests.Scripts.``TestExecutionStarted Event should``

open Archer
open Archer.Arrows
open Archer.Types.InternalTypes
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

let ``be raised from the given test when runner is run`` =
    feature.Test (fun () ->
        let runner, test = buildTestRunner successfulEnvironmentTest successfulUnitSetup successfulTeardown

        let mutable result = "Not Called" |> newFailure.With.TestOtherExpectationFailure |> TestFailure
        
        runner.RunnerLifecycleEvent
        |> Event.filter (fun args ->
            match args with
            | RunnerTestLifeCycle (_, TestStartExecution _, _) -> true
            | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | RunnerTestLifeCycle(currentTest, _, _) ->
                result <-
                    currentTest
                    |> expects.ToBe test
            | _ -> ()
        )

        getDefaultSeed
        |> runner.Run
        |> ignore

        result
    )
    
let ``Test Cases`` = feature.GetTests ()