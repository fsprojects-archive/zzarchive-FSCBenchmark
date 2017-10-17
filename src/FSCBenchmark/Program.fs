module Program

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running


type SprintfBenchmark () =
    let mutable n : int = 0
    
    [<Params (1, 10, 100, 500, 1000, 2000)>] 
    member val public N = 0 with get, set

    [<GlobalSetup>]
    member self.SetupData() =
        n <- self.N

    [<Benchmark; MemoryDiagnoser>]
    member __.Multiple () =
        for i in 1 .. n do
            sprintf "%s %s %d" "hello" "world" 1 |> ignore
            sprintf "%s %s %s" "hello" "world" "hello" |> ignore
            sprintf "%s %s %A" "hello" "world" n |> ignore
            sprintf "%s %s %d" "hello" "world" n |> ignore
            sprintf "%s %s %d" "hello" "world" i |> ignore
            ()


    [<Benchmark; MemoryDiagnoser>]
    member __.Long () =
        for i in 1 .. n do
            sprintf 
                "%s %s %d %s %s %s %s %s %A%s %s %d%s %s %d" "hello" "world" 1 
                "hello" "world" "hello"
                "hello" "world" n
                "hello" "world" n
                "hello" "world" i
            |> ignore
            ()


    [<Benchmark; MemoryDiagnoser>]
    member __.Simple () =
        for i in 1 .. n do
            sprintf "%s" "hello world"
            |> ignore
            ()

    [<Benchmark; MemoryDiagnoser>]
    member __.SimpleArr () =
        let x = [| 1; 2 ; 3|]
        for i in 1 .. n do
            sprintf "%A" x
            |> ignore
            ()


    [<Benchmark; MemoryDiagnoser>]
    member __.Double () =
        for i in 1 .. n do
            sprintf "Hello %s" "world"
            |> ignore
            ()

    [<Benchmark; MemoryDiagnoser>]
    member __.DoubleArr () =
        let x = [| 1; 2 ; 3|]
        for i in 1 .. n do
            sprintf "Hello %A" x
            |> ignore
            ()          

    [<Benchmark; MemoryDiagnoser>]
    member __.Parallel () =
        [| for i in 1 .. n do
                yield async { 
                        sprintf "%s %s %s" "hello" "world" "hello" |> ignore
                        let s = sprintf "%s %s" "hello" "world"
                        return sprintf "%s %d" s i
                    }  
        |] 
        |> Async.Parallel 
        |> Async.RunSynchronously

type PrintfBenchmark () =
    let mutable n : int = 0
    
    [<Params (1, 10, 100, 500, 1000, 2000)>] 
    member val public N = 0 with get, set

    [<GlobalSetup>]
    member self.SetupData() =
        n <- self.N

    [<Benchmark; MemoryDiagnoser>]
    member __.Simple () =
        for i in 1 .. n do
            printf "%s" "hello world"
            |> ignore
            ()

    [<Benchmark; MemoryDiagnoser>]
    member __.Double () =
        for i in 1 .. n do
            sprintf "Hello %s" "world"
            |> ignore
            ()

[<EntryPoint>]
let Main _ =
    BenchmarkRunner.Run<SprintfBenchmark>() |> ignore
    BenchmarkRunner.Run<PrintfBenchmark>() |> ignore
    0