module Archer.Tests.Scripts.Program
// For more information see https://aka.ms/fsharp-console-apps

open Archer.Bow.Lib
open Archer.Tests.Scripts.TestingLibrary

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