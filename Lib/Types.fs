﻿namespace Archer.Bow

open System
open System.ComponentModel
open Archer.Bow.Executor
open Archer.CoreTypes.InternalTypes
open Archer.CoreTypes.InternalTypes.RunnerTypes

type Runner (startingTests: ITest list) as this =
    let mutable tests = startingTests
    let lockObj = obj()
    let mutable cancel = false
    let setCancel value =
        lock lockObj (fun () -> cancel <- value)
        cancel
        
    let readCancel () =
        lock lockObj (fun () -> cancel)
        
    let RunnerLifecycleEvent = Event<RunnerExecutionDelegate, RunnerEventLifecycle> ()
    let handleTestEvents (test: obj) (event: TestEventLifecycle) =
        match test with
        | :? ITest as tst ->
            match event with
            | TestStartExecution cancelEventArgs
            | TestStartSetup cancelEventArgs
            | TestEndSetup (_, cancelEventArgs)
            | TestStart cancelEventArgs as eventType ->
                cancelEventArgs.Cancel <- cancelEventArgs.Cancel || readCancel ()
                RunnerLifecycleEvent.Trigger (this, RunnerTestLifeCycle (tst, eventType, cancelEventArgs))
                setCancel cancelEventArgs.Cancel |> ignore
            | TestEnd _
            | TestStartTeardown
            | TestEndExecution _ as eventType ->
                let cancelEventArgs = readCancel () |> CancelEventArgs
                RunnerLifecycleEvent.Trigger (this, RunnerTestLifeCycle (tst, eventType, cancelEventArgs))
                
        | _ -> ()
        
    new () =
        let tests: ITest list = []
        Runner tests
        
    member this.Run () =
        this.Run(fun () -> globalRandom.Next ())
        
    member this.Run (getSeed: unit -> int) =
        this.Run (ifOnlyFilter, getSeed)
        
    member this.Run (filter: ITest list -> ITest list) =
        this.Run (filter, fun () -> globalRandom.Next ())
    
    member this.Run (filter: ITest list -> ITest list, getSeed: unit -> int) =
        let seed = getSeed ()
        let startArgs = CancelEventArgs ()
        RunnerLifecycleEvent.Trigger (this, RunnerStartExecution startArgs)
        
        let hookEvents (executor: ITestExecutor) =
            executor.TestLifecycleEvent.AddHandler handleTestEvents
            executor
            
        let unhookEvents (executor: ITestExecutor) =
            executor.TestLifecycleEvent.RemoveHandler handleTestEvents

        let executors =
            tests
            |> filter
            |> List.map (fun t -> t.GetExecutor () |> hookEvents)

        try
            if startArgs.Cancel then
                buildReport ([], [], [], seed)
            else
                let shuffled = executors |> shuffle seed
                let results =
                    runTests seed shuffled
                let report =
                    results
                    |> buildReport
                
                report
        finally
            executors |> List.iter unhookEvents
            RunnerLifecycleEvent.Trigger (this, RunnerEndExecution)
    
    member this.AddTests (newTests: ITest seq) =
        tests <-
            [
                tests
                (newTests |> List.ofSeq)
            ] |> List.concat
        
        this
        
        
    interface IRunner with
        member this.AddTests newTests = this.AddTests newTests
        
        member this.Run () = this.Run ()
        
        member this.Run (getSeed: unit -> int) = this.Run getSeed
        member this.Run (filter: ITest list -> ITest list) = this.Run filter
        member this.Run (filter, getSeed) = this.Run (filter, getSeed)
        
        [<CLIEvent>]
        member this.RunnerLifecycleEvent = RunnerLifecycleEvent.Publish

        member this.TestTags =
            tests
            |> List.map (getTags >> Seq.toList)
            |> List.concat
            |> List.distinct
        
type Bow () =
    member _.Runner () = Runner () :> IRunner