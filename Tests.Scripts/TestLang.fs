[<AutoOpen>]
module Archer.Tests.Scripts.TestLang.Lang

open Archer.Tests.Scripts.TestLang.Types

let suite = TestContainerBuilder ()

let newTest fn (container: TestBuilder) =
    fn container