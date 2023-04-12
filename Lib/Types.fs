namespace Archer.Bow

open System
open System.ComponentModel
open Archer.Bow.Executor
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.FrameworkTypes

type Framework (tests: ITestExecutor list) as this =
    let lockObj = obj()
    let mutable cancel = false
    let setCancel value =
        lock lockObj (fun () -> cancel <- value)
        cancel
        
    let readCancel () =
        lock lockObj (fun () -> cancel)
        
    let frameworkLifecycleEvent = Event<FrameworkExecutionDelegate, FrameworkEventLifecycle> ()
    let handleTestEvents (test: obj) (event: TestEventLifecycle) =
        match test with
        | :? ITest as tst ->
            match event with
            | TestExecutionStarted cancelEventArgs
            | TestSetupStarted cancelEventArgs
            | TestEndSetup (_, cancelEventArgs)
            | TestStart cancelEventArgs as eventType ->
                cancelEventArgs.Cancel <- cancelEventArgs.Cancel || readCancel ()
                frameworkLifecycleEvent.Trigger (this, FrameworkTestLifeCycle (tst, eventType, cancelEventArgs))
                setCancel cancelEventArgs.Cancel |> ignore
            | TestEnd _
            | TestStartTearDown
            | TestEndExecution _ as eventType ->
                let cancelEventArgs = readCancel () |> CancelEventArgs
                frameworkLifecycleEvent.Trigger (this, FrameworkTestLifeCycle (tst, eventType, cancelEventArgs))
                
        | _ -> ()
        
    do
        let hookEvents (executor: ITestExecutor) =
            executor.TestLifecycleEvent.AddHandler handleTestEvents
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
        this.Run(fun () -> globalRandom.Next ())
        
    member this.Run (getSeed: unit -> int) =
        let seed = getSeed ()
        let startArgs = CancelEventArgs ()
        frameworkLifecycleEvent.Trigger (this, FrameworkStartExecution startArgs)

        if startArgs.Cancel then
            buildReport ([], [], [], seed)
        else
            let shuffled = tests |> shuffle seed
            let results =
                runTests seed shuffled
            let report =
                results
                |> buildReport
            
            frameworkLifecycleEvent.Trigger (this, FrameworkEndExecution)
            report
    
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
        member this.FrameworkLifecycleEvent = frameworkLifecycleEvent.Publish
        
type Bow () =
    member _.Framework () = Framework () :> IFramework