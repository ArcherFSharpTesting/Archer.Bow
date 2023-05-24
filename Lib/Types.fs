namespace Archer.Bow

open System.ComponentModel
open Archer
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
                let tm = System.DateTime.Now
                buildReport tm tm ([], [], [], seed)
            else
                let startTm = System.DateTime.Now
                let groups =
                    executors
                    |> List.groupBy
                        (fun exe ->
                            exe.Parent.Tags |> Seq.contains Serial
                        )
                let parallelGroup = groups |> List.filter fst |> List.map snd |> List.concat |> shuffle seed
                let serialGroup = groups |> List.filter (fst >> not) |> List.map snd |> List.concat |> shuffle seed
                let results =
                    let pFailures, pIgnored, pSuccesses, _seed = runTestsParallel seed parallelGroup
                    let sFailures, sIgnored, sSuccesses, _seed = runTestsSerial seed serialGroup
                    ([pFailures; sFailures] |> List.concat, [pIgnored; sIgnored] |> List.concat, [pSuccesses; sSuccesses] |> List.concat, seed)
                let endTm = System.DateTime.Now
                
                let report =
                    results
                    |> buildReport startTm endTm
                
                report
        finally
            executors |> List.iter unhookEvents
            RunnerLifecycleEvent.Trigger (this, RunnerEndExecution)
    
    member this.AddTests (newTests: ITest seq) =
        tests <-
            [
                tests
                (newTests |> List.ofSeq)
            ]
            |> List.concat
            |> List.distinct
            
        let names =
            tests
            |> List.map (fun test -> test.ContainerPath, test.ContainerName, test.TestName)
            
        let errors =
            names
            |> List.countBy id
            |> List.filter (fun (_, cnt) -> 1 < cnt)
            |> List.map (fst >> (fun (path, container, test) -> $"%s{path}.%s{container}.%s{test}"))
            
        if 0 < errors.Length then
            failwith $"Each test name must be unique:\n%A{errors}"
        
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