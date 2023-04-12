[<AutoOpen>]
module Archer.Tests.Scripts.TestHelpers

open System.IO
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Archer
open Archer.Bow
open Archer.MicroLang
open Archer.MicroLang.Types

let buildTestFramework (testAction: (FrameworkEnvironment -> TestResult) option) (parts: TestPart option) =
    let framework = bow.Framework ()
    let test = buildDummyTest testAction parts

    (framework.AddTests [test]), test
    
type FailureBuilder () =
    let getCodeLocation (fullPath: string) (lineNumber: int) =
        let fileInfo = System.IO.FileInfo fullPath
        let path = fileInfo.Directory.FullName
        let fileName = fileInfo.Name
        {
            FilePath = path
            FileName = fileName
            LineNumber = lineNumber 
        }
        
    member _.AsGeneralFailure (message: string, [<CallerFilePath; Optional; DefaultParameterValue("")>] fullPath: string, [<CallerLineNumber; Optional; DefaultParameterValue(-1)>]lineNumber: int) =
        (
            message,
            getCodeLocation fullPath lineNumber
        ) |> GeneralFailure
        
    member this.AsGeneralTestFailure (message: string, [<CallerFilePath; Optional; DefaultParameterValue("")>] fullPath: string, [<CallerLineNumber; Optional; DefaultParameterValue(-1)>]lineNumber: int) =
         this.AsGeneralFailure (message, fullPath, lineNumber)
         |> TestFailure
         
    member _.AsIgnored (message: string option, [<CallerFilePath; Optional; DefaultParameterValue("")>] fullPath: string, [<CallerLineNumber; Optional; DefaultParameterValue(-1)>]lineNumber: int) =
        (
            message,
            getCodeLocation fullPath lineNumber
        ) |> Ignored
         
    member this.AsIgnored (message: string, [<CallerFilePath; Optional; DefaultParameterValue("")>] fullPath: string, [<CallerLineNumber; Optional; DefaultParameterValue(-1)>]lineNumber: int) =
        (Some message, fullPath, lineNumber) |> this.AsIgnored
        
let build = FailureBuilder ()