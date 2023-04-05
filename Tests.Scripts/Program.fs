﻿module Archer.Tests.Scripts.Program
// For more information see https://aka.ms/fsharp-console-apps

open Archer.Bow.Lib
open Archer.Tests.Scripts.TestingLibrary

let framework = archer.Framework ()

// These tests test the testing environment used to test the framework
let testDoublesTests =
    [
        UnitTest.``Base Test Cases``
        [
            ``UnitTestExecutor Happy Path``.``Should have the creating test as its parent``
            ``UnitTestExecutor Happy Path``.``Should return success if test action returns success``
            ``UnitTestExecutor Happy Path``.``Should raise all events in correct order``
            
            ``UnitTestExecutor Failing Test``.``Should return failure if the test action returns failure``
            ``UnitTestExecutor Failing Test``.``Should raise all events even if setup fails``
            ``UnitTestExecutor Failing Test``.``Should return failure if setup fails``
            ``UnitTestExecutor Failing Test``.``Should carry the setup error in future events``
            ``UnitTestExecutor Failing Test``.``Should not run test action``
            
            ``UnitTestExecutor StartExecution should``.``be raised when the test is executed``
            
            ``UnitTestExecutor StartSetup should``.``be raised when the test is executed``
            
            ``UnitTestExecutor EndSetup should``.``be raised when the test is executed``
            ``UnitTestExecutor EndSetup should``.``carry the result of the Setup Function``
            
            ``UnitTestExecutor StartTest should``.``be raised when the test is executed``
            
            ``UnitTestExecutor EndTest should``.``be raised when the test is executed``
            
            ``UnitTestExecutor StartTearDown should``.``be raised when the test is executed``
            
            ``UnitTestExecutor EndExecution should``.``be raised when the test is executed``
        ]
    ]
    |> List.concat

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