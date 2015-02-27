#r @"packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open Fake.AssemblyInfoHelper
open Fake.Git

let testDir = "./src/tests/"

let versionMajorMinor = "1.0"

let commitHash = Information.getCurrentSHA1("")

let versionMacroBuild = 
    match buildServer with
    | _ -> "0.0"

let version = versionMajorMinor + "." + versionMacroBuild

Target "Clean" (fun _ ->
    CleanDirs []
)

Target "Version" (fun _ ->
    CreateCSharpAssemblyInfo "src/VersionInfo.cs"
        [Attribute.Version (versionMajorMinor + ".0.0")
         Attribute.FileVersion version
         Attribute.Metadata("githash", commitHash)]

    match buildServer with
    | _ -> ()
)

Target "Build" (fun _ ->
    !! "src/HealthNet.sln"
        |> MSBuildReleaseExt "" [("Configuration", "Release")] "Build"
        |> Log "AppBuild-Output: "
)

Target "Test" (fun _ ->
    !! (testDir + "**/bin/Release/*Tests.dll")
        |> NUnit (fun p ->
            {p with
                ToolPath = "./packages/NUnit.Runners/tools"
                ToolName = "nunit-console-x86.exe"
                DisableShadowCopy = true
                OutputFile = testDir + "TestResults.xml" })
)

Target "Default" (fun _ ->
    trace "Build Complete"
)

"Clean"
 ==> "Version"
 ==> "Build"
 ==> "Test"
 ==> "Default"

RunTargetOrDefault "Default"