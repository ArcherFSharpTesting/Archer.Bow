module Archer.Tests.Scripts.Program
// For more information see https://aka.ms/fsharp-console-apps

open Archer.Bow
open Archer.MicroLang.Lang
bow.Framework ()
|> addManyTests [
    ``Framework Run Should``.``Test Cases``
    ``FrameworkExecutionStarted Event should``.``Test Cases``
    ``FrameworkExecutionEnded Event should``.``Test Cases``
    ``TestExecutionStarted Event should``.``Test Cases``
    ``TestStartSetup Event should``.``Test Cases``
    ``TestEndSetup Event should``.``Test Cases``
    ``TestStart Event should``.``Test Cases``
    ``TestEnd Event should``.``Test Cases``
    ``TestStartTearDown Event should``.``Test Cases``
    ``TestEndExecution Event should``.``Test Cases``
    ``When running tests that throw exception framework should``.``Test Cases``
]
|> runAndReport