namespace Archer.Tests.Scripts.TestLang.Types

open System.ComponentModel
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Archer.CoreTypes.Lib
open Archer.CoreTypes.Lib.InternalTypes

module TypeSupport =
    let success () = TestSuccess
    let continueCancel (cancel: unit -> bool) fb r =
        match r with
        | TestSuccess ->
            match cancel () with
            | false -> fb ()
            | true -> CancelFailure |> TestFailure
        | error -> error
        
    let continueResult fb r =
        continueCancel (fun () -> false) fb r
        
    let joinCancel (getCancelArgs: unit -> 'a when 'a :> CancelEventArgs ) f1 f2 r =
        let c = getCancelArgs ()
        continueResult (fun () -> f1 c) r
        |> continueCancel (fun () -> c.Cancel) f2
        
    let join f1 f2 r =
        continueResult f1 r
        |> continueResult f2
        
    let wrapCancelEvent (getCancel: unit -> 'a :> CancelEventArgs) f (e: Event<'b, 'a>) sender (r: TestResult) =
        let triggerEvent (cancel: 'a :> CancelEventArgs) =
            e.Trigger (sender, cancel)
            r
            
        joinCancel getCancel triggerEvent f r
        
    let wrapCancelResult f (e: Event<'a, TestCancelEventArgsWithResults>) sender (r: TestResult) =
        let getCancelArgs () = TestCancelEventArgsWithResults r
        
        wrapCancelEvent getCancelArgs f e sender r
        
    let wrapSimpleCancelResult e sender r =
        wrapCancelResult success e sender r
        
    let wrapCancel f (e: Event<'a, CancelEventArgs>) sender (r: TestResult) =
        wrapCancelEvent CancelEventArgs f e sender r
        
    let wrapSimpleCancel e sender r =
        wrapCancel success e sender r
        
    let wrap f (e: Event<'a, TestEventArgs>) sender (r: TestResult) =
        let triggerEvent () =
            e.Trigger (sender, TestEventArgs r)
            r
            
        join triggerEvent f r
        
    let wrapSimple e sender r =
        wrap success e sender r
        
open TypeSupport
        
type UnitTestExecuter (parent: ITest, setup: unit -> TestResult, test: unit -> TestResult, tearDown: unit -> TestResult) =
    let startExecution = Event<CancelDelegate, CancelEventArgs> ()
    let startSetup = Event<CancelDelegate, CancelEventArgs> ()
    let endSetup = Event<CancelTestDelegate, TestCancelEventArgsWithResults> ()
    let startTest = Event<CancelDelegate, CancelEventArgs> ()
    let endTest = Event<CancelTestDelegate, TestCancelEventArgsWithResults> ()
    let startTearDown = Event<CancelDelegate, CancelEventArgs> ()
    let endExecution = Event<Delegate, TestEventArgs> ()
    
    member _.Parent with get () = parent
    
    interface ITestExecutor with
        [<CLIEvent>]
        member this.StartExecution = startExecution.Publish
        [<CLIEvent>]
        member this.StartSetup = startSetup.Publish
        [<CLIEvent>]
        member this.EndSetup = endSetup.Publish
        [<CLIEvent>]
        member this.StartTest = startTest.Publish
        [<CLIEvent>]
        member this.EndTest = endTest.Publish
        [<CLIEvent>]
        member this.StartTearDown = startTearDown.Publish
        [<CLIEvent>]
        member this.EndExecution = endExecution.Publish
        member this.Parent = this.Parent
        member this.Execute() =
            TestSuccess
            |> wrapSimpleCancel startExecution this.Parent
            |> wrapCancel setup startSetup this.Parent
            |> wrapSimpleCancelResult endSetup this.Parent
            |> wrapCancel test startTest this.Parent
            |> wrapSimpleCancelResult endTest this.Parent
            |> wrapCancel tearDown startTearDown this.Parent
            |> wrapSimple endExecution this.Parent
            
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
        
    interface ITest with
        member this.ContainerFullName = containerFullName
        member this.ContainerName = containerName
        member this.LineNumber = lineNumber
        member this.Tags = tags
        member this.TestFullName = testFullName
        member this.TestName = testName
        
        member this.GetExecutor() =
            UnitTestExecuter (this, setup, test, tearDown)
            :> ITestExecutor
            
type TestBuilder (containerPath: string, containerName: string) =
    member _.Test (testName: string, action: unit -> TestResult, [<CallerFilePath; Optional; DefaultParameterValue("")>] path: string, [<CallerLineNumber; Optional; DefaultParameterValue(-1)>]lineNumber: int) =
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
            
        UnitTest (fullPath , containerName, testName, lineNumber, [], action, EmptyPart) :> ITest

type TestContainerBuilder () =
    member _.Container (containerPath: string, containerName: string) =
        TestBuilder (containerPath, containerName)