module Archer.Tests.Scripts.Program
// For more information see https://aka.ms/fsharp-console-apps

open Archer
open Archer.Bow
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.FrameworkTypes
open Archer.MicroLang.Lang

let reportWhileRunning (framework: IFramework) =
    framework.FrameworkLifecycleEvent
    |> Event.add (fun args ->
        match args with
        | FrameworkStartExecution _ ->
            printfn ""
        | FrameworkTestLifeCycle (test, testEventLifecycle, _) ->
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
|> addManyTests [
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