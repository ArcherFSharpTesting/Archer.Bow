module Archer.Tests.Scripts.``TestEndExecution Event should``

open Archer
open Archer.Arrows
open Archer.Arrows.Helpers
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.FrameworkTypes
open Archer.MicroLang

let private feature = Arrow.NewFeature ()

let ``be raised with the given test`` =
    feature.Test (fun _ ->
        let framework, test = buildTestFramework successfulEnvironmentTest successfulUnitSetup successfulTeardown
        
        let mutable result = newFailure.With.TestExecutionWasNotRunValidationFailure () |> TestFailure
        
        framework.FrameworkLifecycleEvent
        |> Event.filter (fun args ->
            match args with
                | FrameworkTestLifeCycle(_, TestEndExecution _, _) -> true
                | _ -> false
        )
        |> Event.add (fun args ->
            match args with
            | FrameworkTestLifeCycle(currentTest, _, _) ->
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