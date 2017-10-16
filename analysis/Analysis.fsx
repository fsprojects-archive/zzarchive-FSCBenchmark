#load "../.paket/load/net45/Analysis/FSharp.Data.fsx"
#load "../.paket/load/net45/Analysis/XPlot.GoogleCharts.fsx"

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

let createRuntimeDiagram() =
    let baseLine = loadResults baseLineFile
    let pr = loadResults prFile

    let baseLineData =
        baseLine.Rows
        |> Seq.zip pr.Rows
        |> Seq.map (fun (b,r) -> sprintf "%s %d" b.Method b.N, b.Mean / r.Mean * 100.)
        |> Seq.toList
        |> List.sortBy fst


    let options =
        Options(title = "Sprintf Performance in %" )
  
    [baseLineData]
    |> Chart.Column
    |> Chart.WithOptions options
    |> Chart.WithLabels ["PR / BaseLine * 100%" ]
    |> Chart.Show

// createRuntimeDiagram()