module Archer.Tests.Scripts.Program
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
    ``UnitTest should``.``have well formed string representation``
    
    ``UnitTestExecutor happy path``.``Should have the creating test as its parent``
    ``UnitTestExecutor happy path``.``Should return success if test action returns success``
    ``UnitTestExecutor happy path``.``Should return failure if the test action returns failure``
    ``UnitTestExecutor happy path``.``Should raise ExecutionStart``
    ``UnitTestExecutor happy path``.``Should raise StartSetup``
    ``UnitTestExecutor happy path``.``Should raise EndSetup``
    ``UnitTestExecutor happy path``.``Should raise StartTest``
    ``UnitTestExecutor happy path``.``Should raise EndTest``
    ``UnitTestExecutor happy path``.``Should raise StartTearDown``
    ``UnitTestExecutor happy path``.``Should raise EndExecution``
    ``UnitTestExecutor happy path``.``Should raise all events in correct order``
    
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