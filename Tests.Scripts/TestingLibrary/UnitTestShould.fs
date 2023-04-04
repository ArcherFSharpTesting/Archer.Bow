module Archer.Tests.Scripts.TestingLibrary.``UnitTest should``

open Archer.CoreTypes.Lib.InternalTypes
open Archer.Tests.Scripts.TestLang
open Archer.Tests.Scripts.TestLang.Types

let private container = suite.Container ("Scripting", "UnitTest should")

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
    
let ``have the line number`` =
    container.Test ("have the line number", fun () ->
            let expectedLineNumber = 66
            let test = UnitTest (ignoreString (), ignoreString(), ignoreString (), expectedLineNumber, [], successfulTest, EmptyPart) :> ITest
            
            test.LineNumber
            |> expectsToBe expectedLineNumber
        )
    
let ``have tags`` =
    container.Test ("have tags", fun () ->
            let tags = [Category "My Test"]
            let test = UnitTest (ignoreString (), ignoreString (), ignoreString (), ignoreInt (), tags, successfulTest, EmptyPart) :> ITest
            
            test.Tags
            |> expectsToBe tags
        )
    
let ``have well formed string representation`` =
    container.Test ("have well formed string representation", fun () ->
            let test = UnitTest ("Container Full Path", ignoreString (), "Test Name", 47, [], successfulTest, EmptyPart)
            
            test.ToString ()
            |> expectsToBe "Container Full Path.Test Name"
        )