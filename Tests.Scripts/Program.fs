﻿module Archer.Tests.Scripts.Program
// For more information see https://aka.ms/fsharp-console-apps

open Archer.Bow.Lib
open Archer.Tests.Scripts.TestingLibrary
open Archer.CoreTypes.Lib

let framework = archer.Framework ()

// These tests test the testing environment used to test the framework
let testDoublesTests =
    [
        ``UnitTest Base Case``.``Test Cases``
        ``UnitTestExecutor Happy Path``.``Test Cases``
        ``UnitTestExecutor With a Failing Test``.``Test Cases``
        ``UnitTestExecutor StartExecution``.``Test Cases``
        ``UnitTestExecutor StartSetup``.``Test Cases``
        ``UnitTestExecutor EndSetup``.``Test Cases``
        ``UnitTestExecutor StartTest``.``Test Cases``
        ``UnitTestExecutor EndTest``.``Test Cases``
        ``UnitTestExecutor StartTearDown``.``Test Cases``
        ``UnitTestExecutor EndExecution``.``Test Cases``
    ]
    |> List.concat

let frameWorkTests =
    [
        ``Framework Run``.``Test Cases``
        ``FrameworkExecutionStarted Event``.``Test Cases``
        ``FrameworkExecutionEnded Event``.``Test Cases``
        ``TestExecutionStarted Event``.``Test Cases``
        ``TestStartSetup Event``.``Test Cases``
        ``TestEndSetup Event``.``Test Cases``
    ]
    |> List.concat
    
[
    testDoublesTests
    frameWorkTests
]
|> List.concat
|> framework.AddTests

let results = framework.Run ()

let ignored =
    results.Failures
    |> List.filter (fun (result, _) ->
        match result with
        | TestFailure (IgnoredFailure _) -> true
        | _ -> false
    )
    |> List.map (fun (result, test) ->
        match result with
        | TestFailure f -> f, test
        | _ -> GeneralFailure "Successful Test matched as failure", test
    )
    
let failures =
    results.Failures
    |> List.filter (fun (result, _) ->
        match result with
        | TestFailure (IgnoredFailure _) -> false
        | _ -> true
    )
    |> List.map (fun (result, test) ->
        match result with
        | TestFailure f -> f, test
        | _ -> GeneralFailure "Successful Test matched as failure", test
    ) 
    
let failureCount = failures |> List.length
    
printfn $"\nTests Passing: %d{results.Successes |> List.length}, Ignored: %d{ignored |> List.length} Failing: %d{failureCount}\n"

failures
|> List.iter (fun (result, test) ->
    printfn $"%A{result} <- %s{test.TestFullName} : %d{test.LineNumber}"
)

printfn ""

ignored
|> List.iter (fun (result, test) ->
    printfn $"%A{result} <- %s{test.TestFullName} : %d{test.LineNumber}"
)

printfn "\n\n\n"

exit failureCount