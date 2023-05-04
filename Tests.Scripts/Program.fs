module Archer.Tests.Scripts.Program

open Archer
open Archer.Bow
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.RunnerTypes
open Archer.MicroLang.Lang

let reportWhileRunning (framework: IRunner) =
    framework.RunnerLifecycleEvent
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
        | FrameworkEndExecution ->
            printfn "\n"
    )
    
    framework

bow.Framework ()
|> addMany [
    ``Framework Run Should``.``Test Cases``
    ``FrameworkExecutionStarted Event should``.``Test Cases``
    ``FrameworkExecutionEnded Event should``.``Test Cases``
    ``TestExecutionStarted Event should``.``Test Cases``
    ``TestStartSetup Event should``.``Test Cases``
    ``TestEndSetup Event should``.``Test Cases``
    ``TestStart Event should``.``Test Cases``
    ``TestEnd Event should``.``Test Cases``
    ``TestStartTearDown Event should``.``Test Cases``
    ``TestEndExecution Event should``.``Test Cases``
    ``When running tests that throw exception framework should``.``Test Cases``
]
|> reportWhileRunning
|> runAndReport