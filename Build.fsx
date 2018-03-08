#r @"packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open Fake.Git
let srcDir = "./src"

let versionMajorMinor = "1.2"

let commitHash = Information.getCurrentSHA1("")

let versionMacroBuild = 
  match buildServer with
  | AppVeyor -> ("0." + appVeyorBuildVersion.Replace("1.1.", ""))
  | _ -> "0"

let buildVersion = versionMajorMinor + "." + versionMacroBuild

Target "Version" (fun _ ->
  CreateCSharpAssemblyInfo (srcDir @@ "VersionInfo.cs")
    [Attribute.Metadata("githash", commitHash)]
)

Target "Clean" (fun _ ->
  CleanDirs []
  DeleteDir "bin"
  CreateDir "bin"
)

Target "Build" (fun _ ->
  DotNetCli.Build (fun p -> 
    { p with
        WorkingDir = srcDir
        Configuration = "Release"
        AdditionalArgs = ["/p:Version=" + versionMajorMinor + ".0"
                          "/p:FileVersion=" + buildVersion
                          "/p:InformationalVersion=" + versionMajorMinor
                          "/p:Product=HealthNet"
                          "/p:Company=HealthNet"
                          "/p:Copyright=\"Copyright HealthNet " + System.DateTime.Now.Year.ToString() + "\""] })
)

Target "Test" (fun _ ->
  DotNetCli.Test (fun p ->
    { p with
        Configuration = "Release"
        Project = "src\\Tests\\HealthNet.Tests"
        AdditionalArgs = [ "--no-build" ] })
)

Target "CreatePackage" (fun _ ->
  // Copy all the package files into a package folder
  for projFile in !! (srcDir @@ "/*/*.csproj") do
    let projectName = (System.IO.FileInfo projFile).Directory.Name
    DotNetCli.Pack(fun p ->
    { p with
        Configuration = "Release"
        Project = projFile
        OutputPath = "../../bin"
        AdditionalArgs = [ "--no-build"
                           "--no-restore"
                           "/p:Authors=bronumski"
                           "/p:PackageIconUrl=https://raw.githubusercontent.com/bronumski/HealthNet/release" + buildVersion + "/src/" + projectName + "/icon.png"
                           "/p:PackageProjectUrl=https://github.com/bronumski/HealthNet"
                           "/p:PackageLicenseUrl=https://raw.githubusercontent.com/bronumski/HealthNet/release/" + buildVersion + "/LICENSE"
                           "/p:RepositoryUrl=git@github.com:bronumski/HealthNet.git"
                           "/p:Description=HealthNet"
                           "/p:Copyright=\"Copyright HealthNet " + System.DateTime.Now.Year.ToString() + "\""
                           "/p:PackageVersion=" + buildVersion ] })
)

Target "Default" (fun _ ->
  trace "Build Complete"
)

"Clean"
 ==> "Build"
 ==> "Test"
 ==> "CreatePackage"
 ==> "Default"

RunTargetOrDefault "Default"