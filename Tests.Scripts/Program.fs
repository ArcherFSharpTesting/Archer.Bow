module Archer.Tests.Scripts.Program
// For more information see https://aka.ms/fsharp-console-apps

open Archer.Bow
open Archer

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
|> List.groupBy (fun (_, test) -> test.ContainerPath, test.ContainerName)
|> List.iter (fun ((containerPath, containerName), (results)) ->
    printfn $"%s{containerPath}"
    printfn $"\t%s{containerName}"

    results
    |> List.iter (fun (failure, test) ->
        printfn $"\t\t%s{test.TestName}"
        printfn $"\t\t\t%A{failure}"
        printfn ""
        printfn $"\t\t%s{System.IO.Path.Join (test.FilePath, test.FileName)}(%d{test.LineNumber})"
    )
)

printfn ""

ignored
|> List.groupBy (fun (_, test) -> test.ContainerPath, test.ContainerName)
|> List.iter (fun ((containerPath, containerName), (results)) ->
    printfn $"%s{containerPath}"
    printfn $"\t%s{containerName}"

    results
    |> List.iter (fun (failure, test) ->
        printfn $"\t\t%s{test.TestName}"
        printfn $"\t\t\t%A{failure}"
        printfn ""
        printfn $"\t\t%s{System.IO.Path.Join (test.FilePath, test.FileName)}(%d{test.LineNumber})"
    )
)

printfn $"\n\nTotal Time: %A{endTime - startTime}"
printfn $"\nSeed: %d{results.Seed}"

printfn "\n"

exit failureCount