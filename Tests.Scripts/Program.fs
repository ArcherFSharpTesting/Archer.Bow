module Archer.Tests.Scripts.Program

open Archer
open Archer.Runner
let runnerFactory = RunnerFactory ()
open Archer.Types.InternalTypes
open Archer.Types.InternalTypes.RunnerTypes
open Archer.Logger.Summaries
open Archer.MicroLang.Lang

let reportWhileRunning (runner: IRunner) =
    runner.RunnerLifecycleEvent
    |> Event.add (fun args ->
        match args with
        | RunnerStartExecution _ ->
            printfn ""
        | RunnerTestLifeCycle (test, testEventLifecycle, _) ->
            match testEventLifecycle with
            | TestEndExecution testExecutionResult ->
                match testExecutionResult with
                | TestExecutionResult TestSuccess -> ()
                | result ->
                    let transformedResult = defaultTestExecutionResultSummaryTransformer result test
                    printfn $"%s{transformedResult}"
                
            | _ -> ()
        | RunnerEndExecution ->
            printfn "\n"
    )
    
    runner

runnerFactory.Runner ()
|> addMany [
    ``Runner Run Should``.``Test Cases``
    ``RunnerExecutionStarted Event should``.``Test Cases``
    ``RunnerExecutionEnded Event should``.``Test Cases``
    ``TestExecutionStarted Event should``.``Test Cases``
    ``TestStartSetup Event should``.``Test Cases``
    ``TestEndSetup Event should``.``Test Cases``
    ``TestStart Event should``.``Test Cases``
    ``TestEnd Event should``.``Test Cases``
    ``TestStartTearDown Event should``.``Test Cases``
    ``TestEndExecution Event should``.``Test Cases``
    ``When running tests that throw exception runner should``.``Test Cases``
]
|> reportWhileRunning
|> runAndReport