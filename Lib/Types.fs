namespace Archer.Bow.Lib

open System
open Archer.CoreTypes.Lib.InternalTypes
open Archer.Bow.Lib.Executor

type FrameworkDelegate = delegate of obj * EventArgs -> unit

type Framework () =
    let frameworkStart = Event<FrameworkDelegate, EventArgs> ()
    let mutable tests = System.Collections.Generic.List<ITest>()
    
    member this.Run () =
        this.Run(fun () -> Random().Next ())
        
    member this.Run (getSeed: unit -> int) =
        frameworkStart.Trigger (this, EventArgs.Empty)
        runTests getSeed tests
        
    member _.AddTests (newTests: ITest seq) =
        tests.AddRange newTests
        
    [<CLIEvent>]
    member _.FrameworkExecutionStarted = frameworkStart.Publish
        
type Archer () =
    member _.Framework () = Framework ()