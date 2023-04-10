module Archer.Tests.Scripts.Program
// For more information see https://aka.ms/fsharp-console-apps

open Archer.Bow
open Archer.MicroLang.Lang
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

framework
|> runAndReport