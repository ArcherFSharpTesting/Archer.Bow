module Archer.Tests.Scripts.Program

open Archer
open Archer.Bow
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.RunnerTypes
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
                let successMsg =
                    match testExecutionResult with
                    | TestExecutionResult TestSuccess -> "Success"
                    | _ -> "Fail"
                    
                let report = $"%A{test} : (%s{successMsg})"
                printfn $"%s{report}"
            | _ -> ()
        | RunnerEndExecution ->
            printfn "\n"
    )
    
    runner

bow.Runner ()
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