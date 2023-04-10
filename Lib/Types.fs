namespace Archer.Bow

open System
open System.ComponentModel
open Archer.Bow.Executor
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.FrameworkTypes

type Framework (tests: ITestExecutor list) as this =
    let frameworkStart = Event<FrameworkCancelDelegate, CancelEventArgs> ()
    let frameworkEnd = Event<FrameworkDelegate, EventArgs> ()

    let testExecutionStarted = Event<FrameworkTestCancelDelegate, FrameworkTestCancelArgs> () 
    let testStartSetup = Event<FrameworkTestCancelDelegate, FrameworkTestCancelArgs> ()
    let testEndSetup = Event<FrameworkTestResultCancelDelegate, FrameworkTestResultCancelArgs> ()
    let testStart = Event<FrameworkTestCancelDelegate, FrameworkTestCancelArgs> ()
    let testEnd = Event<FrameworkTestResultDelegate, FrameWorkTestResultArgs> ()
    let testStartTearDown = Event<FrameworkTestDelegate, FrameWorkTestArgs> ()
    let testEndExecution = Event<FrameworkTestResultDelegate, FrameWorkTestResultArgs> ()
    
    let createTestEventHandler (createArgs: ITest -> 'b -> 'c) (event: Event<'a, 'c>) (testObj: obj) (testArgs: 'b) =
        match testObj with
        | :? ITest as test ->
            let args = createArgs test testArgs
            event.Trigger (this, args)
        | _ -> ()
    
    let createTestCancelEventHandler =
        createTestEventHandler (fun test (cancelArgs: CancelEventArgs) -> FrameworkTestCancelArgs (cancelArgs.Cancel, test))
        
    let createTestResultCancelEventHandler =
        createTestEventHandler (fun test (cancelArgs: TestCancelEventArgsWithResults) -> FrameworkTestResultCancelArgs (cancelArgs.Cancel, test, cancelArgs.TestResult))
        
    let createTestResultEventHandler =
        createTestEventHandler (fun test (args: TestEventArgs) -> FrameWorkTestResultArgs (test, args.TestResult))
        
    let createTestEventHandler =
        createTestEventHandler (fun test (_args: EventArgs) -> FrameWorkTestArgs test)
    
    let handleTestExecutionStarted = createTestCancelEventHandler testExecutionStarted
        
    let handleTestSetupStarted = createTestCancelEventHandler testStartSetup
    
    let handleTestSetupEnded = createTestResultCancelEventHandler testEndSetup
    
    let handleTestStart = createTestCancelEventHandler testStart
    
    let handleTestEnd = createTestResultEventHandler testEnd
    
    let handleTestStartTearDown = createTestEventHandler testStartTearDown
    
    let handleTestEndExecution = createTestResultEventHandler testEndExecution
    
    do
        let hookEvents (executor: ITestExecutor) =
            executor.StartExecution.AddHandler handleTestExecutionStarted
            executor.StartSetup.AddHandler handleTestSetupStarted
            executor.EndSetup.AddHandler handleTestSetupEnded
            executor.StartTest.AddHandler handleTestStart
            executor.EndTest.AddHandler handleTestEnd
            executor.StartTearDown.AddHandler handleTestStartTearDown
            executor.EndExecution.AddHandler handleTestEndExecution
            ()
            
        tests
        |> List.iter hookEvents
    
    new () =
        let tests: ITestExecutor list = []
        Framework tests
        
    new (tests: ITest List) =
        let executors =
            tests
            |> List.map (fun tst -> tst.GetExecutor ())
            
        Framework executors
    
    member this.Run () =
        this.Run(fun () -> Random().Next ())
        
    member this.Run (getSeed: unit -> int) =
        let seed = getSeed ()
        let startArgs = CancelEventArgs ()
        frameworkStart.Trigger (this, startArgs)

        if startArgs.Cancel then
            buildReport [] [] [] seed
        else
            let shuffled = tests |> shuffle seed
            let result = runTests seed shuffled
            frameworkEnd.Trigger (this, EventArgs.Empty)
            result
    
    member this.AddTests (newTests: ITest seq) =
        let tsts =
            newTests
            |> List.ofSeq
            
        Framework tsts
        
        
    interface IFramework with
        member this.AddTests newTests = this.AddTests newTests
        
        member this.Run () = this.Run ()
        
        member this.Run getSeed = this.Run getSeed

        [<CLIEvent>]
        member this.FrameworkEndExecution = frameworkEnd.Publish
        
        [<CLIEvent>]
        member _.FrameworkStartExecution = frameworkStart.Publish
    
        [<CLIEvent>]
        member _.TestEnd = testEnd.Publish
    
        [<CLIEvent>]
        member _.TestEndExecution = testEndExecution.Publish
        
        [<CLIEvent>]
        member _.TestEndSetup = testEndSetup.Publish
        
        [<CLIEvent>]
        member _.TestStart = testStart.Publish
        
        [<CLIEvent>]
        member _.TestStartExecution = testExecutionStarted.Publish
        
        [<CLIEvent>]
        member _.TestStartSetup = testStartSetup.Publish
        
        [<CLIEvent>]
        member _.TestStartTearDown = testStartTearDown.Publish
        
type Bow () =
    member _.Framework () = Framework () :> IFramework