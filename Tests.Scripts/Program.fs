module Archer.Tests.Scripts.Program
// For more information see https://aka.ms/fsharp-console-apps

open Archer.CoreTypes.Lib

let results =
    [
        Archer.Tests.Scripts.``Framework Run Should``.``return empty results when it has no tests``
        Archer.Tests.Scripts.``Framework Run Should``.``return empty results with specific seed when it has no tests``
        Archer.Tests.Scripts.``Framework Run Should``.``return a successful result when one test passes``
        Archer.Tests.Scripts.``Framework Run Should``.``return a successful result when two tests pass``
        Archer.Tests.Scripts.``Framework Run Should``.``return failure when a test fails``
        Archer.Tests.Scripts.``Framework Run Should``.``return failure when second test fails``
    ]
    |> List.map (fun t -> t.LineNumber, t.TestFullName, t.GetExecutor().Execute())
    
printfn "\n"
results
|> List.iter (fun (lineNumber, testName, result) ->
        printfn $"%A{result} <- %s{testName} : %d{lineNumber}"
    )

printfn "\n\n\n"

exit (results
      |> List.filter (fun (_, _, result) -> result = TestSuccess |> not)
      |> List.length)