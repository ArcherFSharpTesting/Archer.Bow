namespace Archer.Bow.Lib

open System
open System.ComponentModel
open Archer.CoreTypes.Lib
open Archer.CoreTypes.Lib.InternalTypes
open Archer.Bow.Lib.Executor

type FrameworkTestCancelArgs (cancel: bool, test: ITest) =
    inherit CancelEventArgs (cancel)

    new (test) = FrameworkTestCancelArgs (false, test)
    
    member _.Test with get () = test
    
type FrameworkTestResultCancelArgs (cancel: bool, test: ITest, result: TestResult) =
    inherit CancelEventArgs (cancel)
    
    new (test: ITest) = FrameworkTestResultCancelArgs (false, test, TestSuccess)
    
    new (test: ITest, result: TestResult) = FrameworkTestResultCancelArgs (false, test, result)
    
    member _.Test with get () = test
    member _.Result with get () = result
    
type FrameworkDelegate = delegate of obj * EventArgs -> unit
type FrameworkTestCancelDelegate = delegate of obj * FrameworkTestCancelArgs -> unit
type FrameworkTestResultCancelDelegate = delegate of obj * FrameworkTestResultCancelArgs -> unit

type Framework () as this =
    let frameworkStart = Event<FrameworkDelegate, EventArgs> ()
    let testExecutionStarted = Event<FrameworkTestCancelDelegate, FrameworkTestCancelArgs> () 
    let testStartSetup = Event<FrameworkTestCancelDelegate, FrameworkTestCancelArgs> ()
    let frameworkEnd = Event<FrameworkDelegate, EventArgs> ()
    
    let mutable tests = System.Collections.Generic.List<ITestExecutor>()
    
    let handleTestExecutionStarted (testObj: obj) (cancelArgs: CancelEventArgs) =
        match testObj with
        | :? ITest as test ->
            let args = FrameworkTestCancelArgs (cancelArgs.Cancel, test)
            testExecutionStarted.Trigger (this, args)
        | _ -> ()
        
    let handleTestSetupStarted (testObj: obj) (cancelArgs: CancelEventArgs) =
        match testObj with
        | :? ITest as test ->
            let args = FrameworkTestCancelArgs (cancelArgs.Cancel, test)
            testStartSetup.Trigger (this, args)
        | _ -> ()
        
    member this.Run () =
        this.Run(fun () -> Random().Next ())
        
    member this.Run (getSeed: unit -> int) =
        frameworkStart.Trigger (this, EventArgs.Empty)
        let result = runTests getSeed tests
        frameworkEnd.Trigger (this, EventArgs.Empty)
        result
    
    member this.AddTests (newTests: ITest seq) =
        let hookEvents (executor: ITestExecutor) =
            executor.StartExecution.AddHandler handleTestExecutionStarted
            executor.StartSetup.AddHandler handleTestSetupStarted
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

        
type Archer () =
    member _.Framework () = Framework ()