module Program

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running


type SprintfBenchmark () =
    let mutable n : int = 0
    
    [<Params (100, 500, 1000, 2000)>] 
    member val public N = 0 with get, set

    [<GlobalSetup>]
    member self.SetupData() =
        n <- self.N

    [<Benchmark; MemoryDiagnoser>]
    member __.Run () =
        for i in 1.. n do
            sprintf "%s %s %d" "hello" "world" 1 |> ignore
            sprintf "%s %s %s" "hello" "world" "hello" |> ignore
            sprintf "%s %s %A" "hello" "world" n |> ignore
            sprintf "%s %s %d" "hello" "world" n |> ignore
            sprintf "%s %s %d" "hello" "world" i |> ignore
            ()


    [<Benchmark; MemoryDiagnoser>]
    member __.RunLongstring () =
        for i in 1.. n do
            sprintf 
                "%s %s %d %s %s %s %s %s %A%s %s %d%s %s %d" "hello" "world" 1 
                "hello" "world" "hello"
                "hello" "world" n
                "hello" "world" n
                "hello" "world" i
            |> ignore
            ()

    [<Benchmark; MemoryDiagnoser>]
    member __.RunParallel () =
        [| for i in 1..n do
                yield async { 
                        sprintf "%s %s %s" "hello" "world" "hello" |> ignore
                        let s = sprintf "%s %s" "hello" "world"
                        return sprintf "%s %d" s i
                    }  
        |] 
        |> Async.Parallel 
        |> Async.RunSynchronously
 

[<EntryPoint>]
let Main _ =
    BenchmarkRunner.Run<SprintfBenchmark>() |> ignore
    0