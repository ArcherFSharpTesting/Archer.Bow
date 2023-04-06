﻿namespace Archer.Bow

open System
open System.ComponentModel
open Archer.CoreTypes
open Archer.CoreTypes.InternalTypes
open Archer.Bow.Executor

type FrameWorkTestArgs (test: ITest) =
    inherit EventArgs ()
    
    member _.Test with get () = test

type FrameWorkTestResultArgs (test: ITest, result: TestResult) =
    inherit EventArgs ()
    
    member _.Test with get () = test
    member _.Result with get () = result

type FrameworkTestCancelArgs (cancel: bool, test: ITest) =
    inherit CancelEventArgs (cancel)

    new (test) = FrameworkTestCancelArgs (false, test)
    
    member _.Test with get () = test
    
type FrameworkTestResultCancelArgs (cancel: bool, test: ITest, result: TestResult) =
    inherit CancelEventArgs (cancel)
    
    new (test: ITest) = FrameworkTestResultCancelArgs (false, test, TestSuccess)
    
    new (test: ITest, result: TestResult) = FrameworkTestResultCancelArgs (false, test, result)
    
    member _.Test with get () = test
    member _.TestResult with get () = result
    
type FrameworkDelegate = delegate of obj * EventArgs -> unit
type FrameworkCancelDelegate = delegate of obj * CancelEventArgs -> unit

type FrameworkTestDelegate = delegate of obj * FrameWorkTestArgs -> unit
type FrameworkTestResultDelegate = delegate of obj * FrameWorkTestResultArgs -> unit
type FrameworkTestCancelDelegate = delegate of obj * FrameworkTestCancelArgs -> unit
type FrameworkTestResultCancelDelegate = delegate of obj * FrameworkTestResultCancelArgs -> unit

type Framework () as this =
    let frameworkStart = Event<FrameworkCancelDelegate, CancelEventArgs> ()
    let frameworkEnd = Event<FrameworkDelegate, EventArgs> ()

    let testExecutionStarted = Event<FrameworkTestCancelDelegate, FrameworkTestCancelArgs> () 
    let testStartSetup = Event<FrameworkTestCancelDelegate, FrameworkTestCancelArgs> ()
    let testEndSetup = Event<FrameworkTestResultCancelDelegate, FrameworkTestResultCancelArgs> ()
    let testStart = Event<FrameworkTestCancelDelegate, FrameworkTestCancelArgs> ()
    let testEnd = Event<FrameworkTestResultDelegate, FrameWorkTestResultArgs> ()
    let testStartTearDown = Event<FrameworkTestDelegate, FrameWorkTestArgs> ()
    
    let mutable tests = System.Collections.Generic.List<ITestExecutor>()
    
    let creatTestEventHandler (createArgs: ITest -> 'b -> 'c) (event: Event<'a, 'c>) (testObj: obj) (testArgs: 'b) =
        match testObj with
        | :? ITest as test ->
            let args = createArgs test testArgs
            event.Trigger (this, args)
        | _ -> ()
    
    let createTestCancelEventHandler =
        creatTestEventHandler (fun test (cancelArgs: CancelEventArgs) -> FrameworkTestCancelArgs (cancelArgs.Cancel, test))
        
    let createTestResultCancelEventHandler (event: Event<'a, FrameworkTestResultCancelArgs>) (testObj: obj) (cancelArgs: TestCancelEventArgsWithResults) =
        match testObj with
        | :? ITest as test ->
            let args = FrameworkTestResultCancelArgs (cancelArgs.Cancel, test, cancelArgs.TestResult)
            event.Trigger (this, args)
        | _ -> ()
        
    let createTestResultEventHandler (event: Event<'a, FrameWorkTestResultArgs>) (testObj: obj) (args: TestEventArgs) =
        match testObj with
        | :? ITest as test ->
            let args = FrameWorkTestResultArgs (test, args.TestResult)
            event.Trigger (this, args)
        | _ -> ()
        
    let createTestEventHandler (event: Event<'a, FrameWorkTestArgs>) (testObj: obj) (args: EventArgs) =
        match testObj with
        | :? ITest as test ->
            let args = FrameWorkTestArgs test
            event.Trigger (this, args)
        | _ -> ()
    
    let handleTestExecutionStarted = createTestCancelEventHandler testExecutionStarted
        
    let handleTestSetupStarted = createTestCancelEventHandler testStartSetup
    
    let handleTestSetupEnded = createTestResultCancelEventHandler testEndSetup
    
    let handleTestStart = createTestCancelEventHandler testStart
    
    let handleTestEnd = createTestResultEventHandler testEnd
    
    let handleTestStartTearDown = createTestEventHandler testStartTearDown
        
    member this.Run () =
        this.Run(fun () -> Random().Next ())
        
    member this.Run (getSeed: unit -> int) =
        let seed = getSeed ()
        let startArgs = CancelEventArgs ()
        frameworkStart.Trigger (this, startArgs)

        if startArgs.Cancel then
            buildReport [] [] seed
        else
            let result = runTests seed tests
            frameworkEnd.Trigger (this, EventArgs.Empty)
            result
    
    member this.AddTests (newTests: ITest seq) =
        let hookEvents (executor: ITestExecutor) =
            executor.StartExecution.AddHandler handleTestExecutionStarted
            executor.StartSetup.AddHandler handleTestSetupStarted
            executor.EndSetup.AddHandler handleTestSetupEnded
            executor.StartTest.AddHandler handleTestStart
            executor.EndTest.AddHandler handleTestEnd
            executor.StartTearDown.AddHandler handleTestStartTearDown
            executor
            
        let getExecutor (test: ITest) = test.GetExecutor ()
            
        let executors =
            newTests
            |> Seq.map (getExecutor >> hookEvents)
            
        executors
        |> tests.AddRange
        
    [<CLIEvent>]
    member _.FrameworkStartExecution = frameworkStart.Publish
    
    [<CLIEvent>]
    member _.FrameworkEndExecution = frameworkEnd.Publish
    
    [<CLIEvent>]
    member _.TestStartExecution = testExecutionStarted.Publish
    
    [<CLIEvent>]
    member _.TestStartSetup = testStartSetup.Publish
    
    [<CLIEvent>]
    member _.TestEndSetup = testEndSetup.Publish
    
    [<CLIEvent>]
    member _.TestStart = testStart.Publish
    
    [<CLIEvent>]
    member _.TestEnd = testEnd.Publish
    
    [<CLIEvent>]
    member _.TestStartTearDown = testStartTearDown.Publish
        
type Archer () =
    member _.Framework () = Framework ()