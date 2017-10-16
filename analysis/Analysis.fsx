#load "../.paket/load/net45/Analysis/FSharp.Data.fsx"
#I "../packages/analysis/XPlot.GoogleCharts/lib/net45"
#I "../packages/analysis/XPlot.GoogleCharts.WPF/lib/net45"
#I "../packages/analysis/Google.DataTable.Net.Wrapper/lib"
#r "Google.DataTable.Net.Wrapper.dll"
#r "XPlot.GoogleCharts.dll"
#r "XPlot.GoogleCharts.WPF.dll"
open XPlot.GoogleCharts

open System.IO
open FSharp.Data


let [<Literal>]ResultsFile = __SOURCE_DIRECTORY__ + "/SprintfBenchmark-report.csv"
type Results = CsvProvider<ResultsFile, ";", InferRows = 0, Quote='"' >
let loadResults (fileName:string) = 
    let s = File.ReadAllText(fileName).Replace("\"","").Replace(" us","")
    Results.Parse s

let baseLineFile = __SOURCE_DIRECTORY__ + "/../results/baseline/SprintfBenchmark-report.csv"
let prFile = __SOURCE_DIRECTORY__ + "/../results/pr/SprintfBenchmark-report.csv"

let createDiagram() =
    let baseLine = loadResults baseLineFile
    let pr = loadResults prFile

    let baseLineData =
        baseLine.Rows
        |> Seq.map (fun r -> sprintf "%s %d" r.Method r.N, r.Mean)
        |> Seq.toList

    let prData =
        pr.Rows
        |> Seq.map (fun r -> sprintf "%s %d" r.Method r.N, r.Mean)
        |> Seq.toList

    let options =
        Options(
            title = "Sprintf Performance"
        )
  
    [baseLineData; prData]
    |> Chart.Column
    |> Chart.WithOptions options
    |> Chart.WithLabels ["BaseLine"; "PR" ]
    |> Chart.Show