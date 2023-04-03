module Archer.Tests.Scripts.Scripting.``UnitTest should``

open Archer.Bow.Lib
open Archer.CoreTypes.Lib
open Archer.CoreTypes.Lib.InternalTypes
open Archer.Tests.Scripts.TestLang
open Archer.Tests.Scripts.TestLang.Types

let defaultSeed = 33
let getDefaultSeed () = defaultSeed

let private container = suite.Container ("Scripting", "UnitTest should")

let randomInt _ = System.Random().Next ()
let ignoreInt _ = randomInt ()
let ignoreString _ = $"%d{randomInt ()}%d{randomInt ()}%d{randomInt ()}"
let successfulTest () = TestSuccess

let ``have the test name`` =
    container.Test ("have the test name", fun () ->
            let expectedName = "My Test Name"
            let test = UnitTest (ignoreString (), ignoreString (), expectedName, ignoreInt (), [], successfulTest, EmptyPart) :> ITest
            
            test.TestName
            |> expectsToBe expectedName
        )
    
let ``have the container name`` =
    container.Test ("have the container name", fun () ->
            let expectedName = "My Container Name"
            let test = UnitTest (ignoreString(), expectedName, ignoreString (), ignoreInt (), [], successfulTest, EmptyPart) :> ITest
            
            test.ContainerName
            |> expectsToBe expectedName
        )
    
let ``have the container fullname`` =
    container.Test ("have the container fullname", fun () ->
            let expectedName = "My Container Full Name"
            let test = UnitTest (expectedName, ignoreString (), ignoreString (), ignoreInt (), [], successfulTest, EmptyPart) :> ITest
            
            test.ContainerFullName
            |> expectsToBe expectedName
        )
    
let ``have the test fullname`` =
    container.Test ("have the test fullname", fun () ->
            let testName = "My Test Name"
            let containerFullName = "My Container Full Name"
            let expectedName = $"%s{containerFullName}.%s{testName}"
            let test = UnitTest (containerFullName, ignoreString (), testName, ignoreInt (), [], successfulTest, EmptyPart) :> ITest
            
            test.TestFullName
            |> expectsToBe expectedName
        )