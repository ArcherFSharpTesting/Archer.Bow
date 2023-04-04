namespace Archer.Tests.Scripts.TestLang.Types

open System.ComponentModel
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Archer.CoreTypes.Lib
open Archer.CoreTypes.Lib.InternalTypes

module TypeSupport =
    let success () = TestSuccess
    let maybeTriggerCancel sender (event: Event<'a, 'b :> CancelEventArgs>) (getCancel: unit -> 'b) previousResult =
        let cancelArgs = getCancel ()
        event.Trigger (sender, cancelArgs)

        match cancelArgs.Cancel with
        | true -> CancelFailure |> TestFailure
        | _ -> previousResult

    let trigger sender (event: Event<'a, TestEventArgs>) previousResult =
        match previousResult with
        | TestFailure CancelFailure -> previousResult
        | _ ->
            let args = TestEventArgs previousResult
            event.Trigger (sender, args)
            previousResult

    let wrapEvent event sender previousResult = 
        trigger sender event previousResult

    let wrapCancel event sender previousResult =
        maybeTriggerCancel sender event CancelEventArgs previousResult

    let wrapCancelResult event sender previousResult =
        maybeTriggerCancel sender event (fun () -> TestCancelEventArgsWithResults previousResult) previousResult

    let joinCancelEventResult event action sender previousResult =
        let result = wrapCancelResult event sender previousResult
        match result with
        | TestFailure _ -> result
        | _ -> action ()

    let joinCancelEvent event action sender previousResult =
        let result = wrapCancel event sender previousResult
        match result with
        | TestFailure _ -> result
        | _ -> action ()
        
open TypeSupport
        
type UnitTestExecutor (parent: ITest, setup: unit -> TestResult, test: unit -> TestResult, tearDown: unit -> TestResult) =
    let startExecution = Event<CancelDelegate, CancelEventArgs> ()
    let startSetup = Event<CancelDelegate, CancelEventArgs> ()
    let endSetup = Event<CancelTestDelegate, TestCancelEventArgsWithResults> ()
    let startTest = Event<CancelDelegate, CancelEventArgs> ()
    let endTest = Event<CancelTestDelegate, TestCancelEventArgsWithResults> ()
    let startTearDown = Event<CancelDelegate, CancelEventArgs> ()
    let endExecution = Event<Delegate, TestEventArgs> ()
    [<CLIEvent>]
    member _.StartExecution = startExecution.Publish
    [<CLIEvent>]
    member _.StartSetup = startSetup.Publish
    [<CLIEvent>]
    member _.EndSetup = endSetup.Publish
    [<CLIEvent>]
    member _.StartTest = startTest.Publish
    [<CLIEvent>]
    member _.EndTest = endTest.Publish
    [<CLIEvent>]
    member _.StartTearDown = startTearDown.Publish
    [<CLIEvent>]
    member _.EndExecution = endExecution.Publish
    
    member _.Parent with get () = parent
    
    member _.Execute () =
        TestSuccess
        |> wrapCancel startExecution parent
        |> joinCancelEvent startSetup setup parent
        |> wrapCancelResult endSetup parent
        |> joinCancelEvent startTest test parent
        |> wrapCancelResult endTest parent
        |> joinCancelEvent startTearDown tearDown parent
        |> wrapEvent endExecution parent
    
    interface ITestExecutor with
        [<CLIEvent>]
        member this.StartExecution = this.StartExecution
        [<CLIEvent>]
        member this.StartSetup = this.StartSetup
        [<CLIEvent>]
        member this.EndSetup = this.EndSetup
        [<CLIEvent>]
        member this.StartTest = this.StartTest
        [<CLIEvent>]
        member this.EndTest = this.EndTest
        [<CLIEvent>]
        member this.StartTearDown = this.StartTearDown
        [<CLIEvent>]
        member this.EndExecution = this.EndExecution
        member _.Parent with get () = parent
        member this.Execute () = this.Execute ()
            
type TestPart =
    | EmptyPart
    | SetupPart of (unit -> TestResult)
    | TearDownPart of (unit -> TestResult)
    | Both of setup: (unit -> TestResult) * tearDown: (unit -> TestResult)
            
type UnitTest (containerFullName: string, containerName: string, testName: string, lineNumber: int, tags: TestTag seq, test: unit -> TestResult, testParts: TestPart) =
    let testFullName =
        [
            containerFullName
            testName
        ]
        |> List.filter (System.String.IsNullOrEmpty >> not)
        |> fun items -> System.String.Join (".", items)
        
    let setup, tearDown =
        match testParts with
        | EmptyPart -> success, success
        | SetupPart setup -> setup, success
        | TearDownPart tearDown -> success, tearDown
        | Both (setup, tearDown) -> setup, tearDown

    override this.ToString () =
        let test = this :> ITest
        test.TestFullName
        
    member _.ContainerFullName = containerFullName
    member _.ContainerName = containerName
    member _.LineNumber = lineNumber
    member _.Tags = tags
    member _.TestFullName = testFullName
    member _.TestName = testName
    
    member this.GetExecutor() =
        UnitTestExecutor (this, setup, test, tearDown)
        :> ITestExecutor
        
    interface ITest with
        member _.ContainerFullName = containerFullName
        member _.ContainerName = containerName
        member _.LineNumber = lineNumber
        member _.Tags = tags
        member _.TestFullName = testFullName
        member _.TestName = testName
        
        member this.GetExecutor() = this.GetExecutor ()
            
type TestBuilder (containerPath: string, containerName: string) =
    member _.Test(testName: string, action: unit -> TestResult, part: TestPart, [<CallerFilePath; Optional; DefaultParameterValue("")>] path: string, [<CallerLineNumber; Optional; DefaultParameterValue(-1)>]lineNumber: int) =
        let fullPath =
            [
                path
                [
                    containerPath
                    containerName
                ]
                |> List.filter (System.String.IsNullOrEmpty >> not)
                |> fun items -> System.String.Join (".", items)
            ]
            |> List.filter (System.String.IsNullOrEmpty >> not)
            |> fun paths -> System.String.Join (" >> ", paths)
            
        UnitTest (fullPath , containerName, testName, lineNumber, [], action, part) :> ITest
    
    member this.Test (testName: string, action: unit -> TestResult, [<CallerFilePath; Optional; DefaultParameterValue("")>] path: string, [<CallerLineNumber; Optional; DefaultParameterValue(-1)>]lineNumber: int) =
        this.Test(testName, action, EmptyPart, path, lineNumber)
    
type TestContainerBuilder () =
    member _.Container (containerPath: string, containerName: string) =
        TestBuilder (containerPath, containerName)