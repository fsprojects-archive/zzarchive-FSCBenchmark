[![Issue Stats](http://issuestats.com/github/fsprojects/FSCBenchmark/badge/issue)](http://issuestats.com/github/fsprojects/FSCBenchmark)
[![Issue Stats](http://issuestats.com/github/fsprojects/FSCBenchmark/badge/pr)](http://issuestats.com/github/fsprojects/FSCBenchmark)

# F# compiler benchmark

This project uses [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) to benchmark the [F# compiler](https://github.com/Microsoft/visualfsharp).

## Getting started

* Clone git@github.com:Microsoft/visualfsharp.git
* Open Developer Command Promt in admin mode 
* cd visualfsharp
* Run `build.cmd release` - this will be used as baseline
* Copy visualfsharp folder to new folder "pr"
* Git checkout a pr
* Run `build.cmd release`
* Now go to `build.fsx` of the benchmark project
* Configure the paths at top of `build.fsx`
* Write some benchmarks in Program.fs
* Run `build.cmd`

## Build Status

Mono | .NET
---- | ----
[![Mono CI Build Status](https://img.shields.io/travis/fsprojects/FSCBenchmark/master.svg)](https://travis-ci.org/fsprojects/FSCBenchmark) | [![.NET Build Status](https://img.shields.io/appveyor/ci/fsgit/FSCBenchmark/master.svg)](https://ci.appveyor.com/project/fsgit/FSCBenchmark)

## Maintainer(s)

- [@forki](https://github.com/forki)