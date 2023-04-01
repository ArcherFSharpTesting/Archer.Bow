module Archer.Tests.Scripts.Program
// For more information see https://aka.ms/fsharp-console-apps

open Archer.CoreTypes.Lib
open Archer.Bow.Lib

let framework = archer.Framework ()

[
    Archer.Tests.Scripts.``Framework Run Should``.``return empty results when it has no tests``
    Archer.Tests.Scripts.``Framework Run Should``.``return empty results with specific seed when it has no tests``
    Archer.Tests.Scripts.``Framework Run Should``.``return a successful result when one test passes``
    Archer.Tests.Scripts.``Framework Run Should``.``return a successful result when two tests pass``
    Archer.Tests.Scripts.``Framework Run Should``.``return failure when a test fails``
    Archer.Tests.Scripts.``Framework Run Should``.``return failure when second test fails``
]
|> List.iter framework.AddTest

let results = framework.Run ()
    
printfn $"\nTests Passing: %d{results.Successes |> List.length}, Failing: %d{results.Failures |> List.length}\n"

results.Failures
|> List.iter (fun (result, test) ->
        printfn $"%A{result} <- %s{test.TestFullName} : %d{test.LineNumber}"
    )

printfn "\n\n\n"

exit (results.Failures |> List.length)