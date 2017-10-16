// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/build/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open Fake.UserInputHelper

open System
open System.IO
open System.Diagnostics


let baseLinePath = @"D:\code\visualfsharpMaster\Release\net40\bin\" 
let prPath = @"D:\code\visualfsharp\Release\net40\bin\" 

let project = "FSCBenchmark"

// Short summary of the project
// (used as description in AssemblyInfo and as a short summary for NuGet package)
let summary = "Benchmarks for the F# compiler"

// Longer description of the project
// (used as a description for NuGet package; line breaks are automatically cleaned up)
let description = "Benchmarks for the F# compiler"

// List of author names (for NuGet package)
let authors = [ "Steffen Forkmann" ]

// Tags for your project (for NuGet package)
let tags = "f#"

// File system information
let solutionFile  = "FSCBenchmark.sln"

// Default target configuration
let configuration = "Release"

// Git configuration (used for publishing documentation in gh-pages branch)
// The profile where the project is posted
let gitOwner = "fsprojects"
let gitHome = sprintf "%s/%s" "https://github.com" gitOwner

// The name of the project on GitHub
let gitName = "FSCBenchmark"

// The url for the raw files hosted
let gitRaw = environVarOrDefault "gitRaw" "https://raw.githubusercontent.com/fsprojects"

// --------------------------------------------------------------------------------------
// END TODO: The rest of the file includes standard build steps
// --------------------------------------------------------------------------------------

// Read additional information from the release notes document
let release = LoadReleaseNotes "RELEASE_NOTES.md"

// Helper active pattern for project types
let (|Fsproj|Csproj|Vbproj|Shproj|) (projFileName:string) =
    match projFileName with
    | f when f.EndsWith("fsproj") -> Fsproj
    | f when f.EndsWith("csproj") -> Csproj
    | f when f.EndsWith("vbproj") -> Vbproj
    | f when f.EndsWith("shproj") -> Shproj
    | _                           -> failwith (sprintf "Project file %s not supported. Unknown project type." projFileName)


// --------------------------------------------------------------------------------------
// Clean build results

let vsProjProps = 
#if MONO
    [ ("DefineConstants","MONO"); ("Configuration", configuration) ]
#else
    [ ("Configuration", configuration); ("Platform", "Any CPU") ]
#endif


// --------------------------------------------------------------------------------------
// Build library & test project

Target "Clean" (fun _ ->
    CleanDirs ["results"]
)

let buildSln fscPath =
    setProcessEnvironVar "FSC_BIN_PATH" fscPath

    !! solutionFile |> MSBuildReleaseExt "" vsProjProps "Clean" |> ignore
    CleanDirs ["bin"; "temp"; ]

    !! solutionFile
    |> MSBuildReleaseExt "" vsProjProps "Rebuild"
    |> ignore

    !! "src/**/*.??proj"
    -- "src/**/*.shproj"
    |>  Seq.map (fun f -> ((System.IO.Path.GetDirectoryName f) </> "bin" </> configuration, "bin" </> (System.IO.Path.GetFileNameWithoutExtension f)))
    |>  Seq.iter (fun (fromDir, toDir) -> CopyDir toDir fromDir (fun _ -> true))


let copyResults v =
    let target = FullName ("./results/" </> v)
    CleanDir target
    !! @"bin\FSCBenchmark\BenchmarkDotNet.Artifacts\results\*.csv"
    |> CopyFiles target

Target "RunBaseLineBenchmark" (fun _ ->
    buildSln baseLinePath
    let result = 
        ExecProcess (fun info ->
            info.FileName <- FullName("./bin/FSCBenchmark/FSCBenchmark.exe")
            info.WorkingDirectory <- FullName("./bin/FSCBenchmark/")) TimeSpan.MaxValue

    if result <> 0 then failwith "Benchmark shut down."

    copyResults "baseline"
)

Target "RunPRBenchmark" (fun _ ->
    buildSln prPath
    let result = 
        ExecProcess (fun info ->
            info.FileName <- FullName("./bin/FSCBenchmark/FSCBenchmark.exe")
            info.WorkingDirectory <- FullName("./bin/FSCBenchmark/")) TimeSpan.MaxValue

    if result <> 0 then failwith "Benchmark shut down."
    copyResults "pr"
)

// --------------------------------------------------------------------------------------
// Analysis
#load "analysis/Analysis.fsx"

Target "RunAnalysis" (fun _ ->
    Analysis.createDiagram()
)

// --------------------------------------------------------------------------------------
// Generate the documentation


let fakePath = "packages" </> "build" </> "FAKE" </> "tools" </> "FAKE.exe"
let fakeStartInfo script workingDirectory args fsiargs environmentVars =
    (fun (info: ProcessStartInfo) ->
        info.FileName <- System.IO.Path.GetFullPath fakePath
        info.Arguments <- sprintf "%s --fsiargs -d:FAKE %s \"%s\"" args fsiargs script
        info.WorkingDirectory <- workingDirectory
        let setVar k v =
            info.EnvironmentVariables.[k] <- v
        for (k, v) in environmentVars do
            setVar k v
        setVar "MSBuild" msBuildExe
        setVar "GIT" Git.CommandHelper.gitPath
        setVar "FSI" fsiPath)

/// Run the given buildscript with FAKE.exe
let executeFAKEWithOutput workingDirectory script fsiargs envArgs =
    let exitCode =
        ExecProcessWithLambdas
            (fakeStartInfo script workingDirectory "" fsiargs envArgs)
            TimeSpan.MaxValue false ignore ignore
    System.Threading.Thread.Sleep 1000
    exitCode

// Documentation
let buildDocumentationTarget fsiargs target =
    trace (sprintf "Building documentation (%s), this could take some time, please wait..." target)
    let exit = executeFAKEWithOutput "docsrc/tools" "generate.fsx" fsiargs ["target", target]
    if exit <> 0 then
        failwith "generating reference documentation failed"
    ()

Target "GenerateReferenceDocs" (fun _ ->
    buildDocumentationTarget "-d:RELEASE -d:REFERENCE" "Default"
)

let generateHelp' fail debug =
    let args =
        if debug then "--define:HELP"
        else "--define:RELEASE --define:HELP"
    try
        buildDocumentationTarget args "Default"
        traceImportant "Help generated"
    with
    | e when not fail ->
        traceImportant "generating help documentation failed"

let generateHelp fail =
    generateHelp' fail false

Target "GenerateHelp" (fun _ ->
    DeleteFile "docsrc/content/release-notes.md"
    CopyFile "docsrc/content/" "RELEASE_NOTES.md"
    Rename "docsrc/content/release-notes.md" "docsrc/content/RELEASE_NOTES.md"

    DeleteFile "docsrc/content/license.md"
    CopyFile "docsrc/content/" "LICENSE.txt"
    Rename "docsrc/content/license.md" "docsrc/content/LICENSE.txt"

    generateHelp true
)

Target "GenerateHelpDebug" (fun _ ->
    DeleteFile "docsrc/content/release-notes.md"
    CopyFile "docsrc/content/" "RELEASE_NOTES.md"
    Rename "docsrc/content/release-notes.md" "docsrc/content/RELEASE_NOTES.md"

    DeleteFile "docsrc/content/license.md"
    CopyFile "docsrc/content/" "LICENSE.txt"
    Rename "docsrc/content/license.md" "docsrc/content/LICENSE.txt"

    generateHelp' true true
)

Target "GenerateDocs" DoNothing

let createIndexFsx lang =
    let content = """(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use
// it to define helpers that you do not want to show in the documentation.
#I "../../../bin"

(**
F# Project Scaffold ({0})
=========================
*)
"""
    let targetDir = "docsrc/content" </> lang
    let targetFile = targetDir </> "index.fsx"
    ensureDirectory targetDir
    System.IO.File.WriteAllText(targetFile, System.String.Format(content, lang))

Target "AddLangDocs" (fun _ ->
    let args = System.Environment.GetCommandLineArgs()
    if args.Length < 4 then
        failwith "Language not specified."

    args.[3..]
    |> Seq.iter (fun lang ->
        if lang.Length <> 2 && lang.Length <> 3 then
            failwithf "Language must be 2 or 3 characters (ex. 'de', 'fr', 'ja', 'gsw', etc.): %s" lang

        let templateFileName = "template.cshtml"
        let templateDir = "docsrc/tools/templates"
        let langTemplateDir = templateDir </> lang
        let langTemplateFileName = langTemplateDir </> templateFileName

        if System.IO.File.Exists(langTemplateFileName) then
            failwithf "Documents for specified language '%s' have already been added." lang

        ensureDirectory langTemplateDir
        Copy langTemplateDir [ templateDir </> templateFileName ]

        createIndexFsx lang)
)


// --------------------------------------------------------------------------------------
// Release Scripts

#load "paket-files/build/fsharp/FAKE/modules/Octokit/Octokit.fsx"
open Octokit

Target "Release" (fun _ ->
    let user =
        match getBuildParam "github-user" with
        | s when not (String.IsNullOrWhiteSpace s) -> s
        | _ -> getUserInput "Username: "
    let pw =
        match getBuildParam "github-pw" with
        | s when not (String.IsNullOrWhiteSpace s) -> s
        | _ -> getUserPassword "Password: "
    let remote =
        Git.CommandHelper.getGitResult "" "remote -v"
        |> Seq.filter (fun (s: string) -> s.EndsWith("(push)"))
        |> Seq.tryFind (fun (s: string) -> s.Contains(gitOwner + "/" + gitName))
        |> function None -> gitHome + "/" + gitName | Some (s: string) -> s.Split().[0]

    StageAll ""
    Git.Commit.Commit "" (sprintf "Bump version to %s" release.NugetVersion)
    Branches.pushBranch "" remote (Information.getBranchName "")

    Branches.tag "" release.NugetVersion
    Branches.pushTag "" remote release.NugetVersion

    // release on github
    createClient user pw
    |> createDraft gitOwner gitName release.NugetVersion (release.SemVer.PreRelease <> None) release.Notes
    // TODO: |> uploadFile "PATH_TO_FILE"
    |> releaseDraft
    |> Async.RunSynchronously
)

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "All" DoNothing

"Clean"
  ==> "RunBaseLineBenchmark"
  ==> "RunPRBenchmark"
  ==> "RunAnalysis"
  ==> "GenerateReferenceDocs"
  ==> "GenerateDocs"
  ==> "All"

"GenerateHelp"
  ==> "GenerateReferenceDocs"
  ==> "GenerateDocs"

RunTargetOrDefault "RunAnalysis"
