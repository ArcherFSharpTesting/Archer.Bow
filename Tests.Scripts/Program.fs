﻿module Archer.Tests.Scripts.Program
// For more information see https://aka.ms/fsharp-console-apps

open Archer.Bow.Lib
open Archer.Tests.Scripts.Scripting

let framework = archer.Framework ()

[
    // Testing The framework built in the tests
    ``UnitTest should``.``have the test name``
    ``UnitTest should``.``have the container name``
    ``UnitTest should``.``have the container fullname``
    ``UnitTest should``.``have the test fullname``
    ``UnitTest should``.``have tags``
    
    // Testing Archer Framework
    ``Framework Run Should``.``return empty results when it has no tests``
    ``Framework Run Should``.``return empty results with specific seed when it has no tests``
    ``Framework Run Should``.``return a successful result when one test passes``
    ``Framework Run Should``.``return a successful result when two tests pass``
    ``Framework Run Should``.``return failure when a test fails``
    ``Framework Run Should``.``return failure when second test fails``
    ``Framework Run Should``.``return failure when both tests fail``
    
    ``When tests execute normally framework should raise``.``the FrameworkExecutionStarted event``
    ``When tests execute normally framework should raise``.``the FrameworkExecutionEnded event``
    // ``When tests execute normally framework should raise``.``the TestExecutionStarted event``
]
|> framework.AddTests

let results = framework.Run ()
    
printfn $"\nTests Passing: %d{results.Successes |> List.length}, Failing: %d{results.Failures |> List.length}\n"

results.Failures
|> List.iter (fun (result, test) ->
        printfn $"%A{result} <- %s{test.TestFullName} : %d{test.LineNumber}"
    )

printfn "\n\n\n"

exit (results.Failures |> List.length)