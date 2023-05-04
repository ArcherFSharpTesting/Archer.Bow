module Archer.Tests.Scripts.``TestEndExecution Event should``

open Archer
open Archer.Arrows
open Archer.Arrows.Helpers
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.RunnerTypes
open Archer.MicroLang

let private feature = Arrow.NewFeature ()

let ``be raised with the given test`` =
    feature.Test (fun _ ->
        let framework, test = buildTestFramework successfulEnvironmentTest successfulUnitSetup successfulTeardown
        
        let mutable result = newFailure.With.TestExecutionWasNotRunValidationFailure () |> TestFailure
        
        framework.RunnerLifecycleEvent
        |> Event.filter (fun args ->
            match args with
                | RunnerTestLifeCycle(_, TestEndExecution _, _) -> true
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
        
        ()
        |> framework.Run
        |> ignore
        
        result
    )
    
let ``Test Cases`` = feature.GetTests ()