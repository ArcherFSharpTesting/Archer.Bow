﻿module Archer.Tests.Scripts.Program
// For more information see https://aka.ms/fsharp-console-apps

open Archer.Bow.Lib
open Archer.Tests.Scripts.TestingLibrary

let framework = archer.Framework ()

// These tests test the testing environment used to test the framework
let testDoublesTests =
    [
        ``UnitTest should``.``have the test name``
        ``UnitTest should``.``have the container name``
        ``UnitTest should``.``have the container fullname``
        ``UnitTest should``.``have the test fullname``
        ``UnitTest should``.``have tags``
        ``UnitTest should``.``have well formed string representation``
        
        ``UnitTestExecutor Happy Path``.``Should have the creating test as its parent``
        ``UnitTestExecutor Happy Path``.``Should return success if test action returns success``
        ``UnitTestExecutor Happy Path``.``Should raise StartSetup``
        ``UnitTestExecutor Happy Path``.``Should raise EndSetup``
        ``UnitTestExecutor Happy Path``.``Should raise StartTest``
        ``UnitTestExecutor Happy Path``.``Should raise EndTest``
        ``UnitTestExecutor Happy Path``.``Should raise StartTearDown``
        ``UnitTestExecutor Happy Path``.``Should raise EndExecution``
        ``UnitTestExecutor Happy Path``.``Should raise all events in correct order``
        
        ``UnitTestExecutor Failing Test``.``Should return failure if the test action returns failure``
        ``UnitTestExecutor Failing Test``.``Should raise all events even if setup fails``
        ``UnitTestExecutor Failing Test``.``Should return failure if setup fails``
        ``UnitTestExecutor Failing Test``.``Should carry the setup error in future events``
        ``UnitTestExecutor Failing Test``.``Should not run test action``
        
        ``UnitTestExecutor StartExecution should``.``be raised when test is executed``
    ]

let frameWorkTests = 
    [
        ``Framework Run Should``.``return empty results when it has no tests``
        ``Framework Run Should``.``return empty results with specific seed when it has no tests``
        ``Framework Run Should``.``return a successful result when one test passes``
        ``Framework Run Should``.``return a successful result when two tests pass``
        ``Framework Run Should``.``return failure when a test fails``
        ``Framework Run Should``.``return failure when second test fails``
        ``Framework Run Should``.``return failure when both tests fail``
        
        ``FrameworkExecutionStarted should``.``be raised when framework is run``
        
        ``FrameworkExecutionEnded should``.``be raised when the framework is run``
        
        ``TestExecutionStarted should``.``be raised from the given test when framework is run``
        
        ``TestStartSetup should``.``be raised from the given test when the framework is run``
        ``TestStartSetup should``.``not be raised if FrameworkExecutionStarted was canceled``
        
        ``TestEndSetup should``.``be raised from the given test when the framework is run``
        ``TestEndSetup should``.``should not be raised if FrameworkExecutionStart canceled``
        // ``TestEndSetup should``.``should not be raised if TestStartExecution is canceled``
    ]   
    
[
    testDoublesTests
    frameWorkTests
]
|> List.concat
|> framework.AddTests

let results = framework.Run ()
    
printfn $"\nTests Passing: %d{results.Successes |> List.length}, Failing: %d{results.Failures |> List.length}\n"

results.Failures
|> List.iter (fun (result, test) ->
        printfn $"%A{result} <- %s{test.TestFullName} : %d{test.LineNumber}"
    )

printfn "\n\n\n"

exit (results.Failures |> List.length)