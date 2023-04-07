module Archer.Tests.Scripts.Program
// For more information see https://aka.ms/fsharp-console-apps

open Archer.Bow
open Archer.CoreTypes

let framework = bow.Framework ()

let frameWorkTests =
    [
        ``Framework Run``.``Test Cases``
        ``FrameworkExecutionStarted Event``.``Test Cases``
        ``FrameworkExecutionEnded Event``.``Test Cases``
        ``TestExecutionStarted Event``.``Test Cases``
        ``TestStartSetup Event``.``Test Cases``
        ``TestEndSetup Event``.``Test Cases``
        ``TestStart Event``.``Test Cases``
        ``TestEnd Event``.``Test Cases``
        ``TestStartTearDown Event``.``Test Cases``
        ``TestEndExecution Event``.``Test Cases``
    ]
    |> List.concat
    
frameWorkTests
|> framework.AddTests

let startTime = System.DateTime.Now
printfn $"Started at %s{startTime.ToShortTimeString ()}"
let results = framework.Run ()

let endTime = System.DateTime.Now
printfn $"Ended at %s{endTime.ToShortTimeString ()}"

let ignored =
    results.Failures
    |> List.filter (fun (result, _) ->
        match result with
        | IgnoredFailure _
        | CancelFailure -> true
        | _ -> false
    )
    
let failures =
    results.Failures
    |> List.filter (fun (result, _) ->
        match result with
        | IgnoredFailure _
        | CancelFailure -> false
        | _ -> true
    )
    
let failureCount = failures |> List.length
    
printfn $"\nTests Passing: %d{results.Successes |> List.length}, Ignored: %d{ignored |> List.length} Failing: %d{failureCount}\n"

failures
|> List.iter (fun (result, test) ->
    printfn $"%s{test.TestFullName}\n\t%A{result}\n\t\t%s{test.FilePath} %d{test.LineNumber}"
)

printfn ""

ignored
|> List.iter (fun (result, test) ->
    printfn $"%s{test.TestFullName}\n%A{result}\n\t%s{test.FilePath} %d{test.LineNumber}"
)

printfn $"\n\nTotal Time: %A{endTime - startTime}"
printfn $"\nSeed: %d{results.Seed}"

printfn "\n"

exit failureCount