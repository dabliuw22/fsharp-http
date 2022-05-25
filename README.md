# fsharp-http

## Requirements
* dotnet
* paket
* fake

## Dependencies
```bash
$ dotnet paket add FAKE --version 5.16.0 --project App --group Build
$ dotnet paket add NUnit --version 3.13.2 --project App --group Test
$ dotnet paket add NUnit.Runners --version 3.12.0 --project App --group Test
$ dotnet paket add Suave --version 2.6.0 --project App
$ dotnet paket add Npgsql.FSharp --version 4.0 --project App
$ dotnet paket add System.Text.Json --version 5.0.2 --project App
$ dotnet paket add Serilog --version 2.10.1-dev-01285 --project App
$ dotnet paket add Serilog.Sinks.Console --version 4.0.0-dev-00839 --project App
$ dotnet paket add Serilog.Sinks.File --version 5.0.0-dev-00909 --project App
$ dotnet paket add Serilog.Formatting.Compact --version 1.1.0 --project App
$ dotnet paket add Serilog.Sinks.Async --version 1.5.0 --project App
$ dotnet paket add Serilog.Settings.Configuration --version 3.2.0-dev-00269 --project App
$ dotnet paket add Serilog.Enrichers.Thread --version 3.2.0-dev-00750 --project App
$ dotnet paket add Serilog.Enrichers.Environment --version 2.2.0-dev-00784 --project App
$ dotnet paket add Serilog.Enrichers.CorrelationId --version 3.0.1 --project App
$ dotnet paket add Serilog.Enrichers.Process --version 2.0.1 --project App
$ dotnet paket add FSharp.Control.Reactive --version 5.0.2 --project App
$ dotnet paket add FSharp.Control.Reactive.Testing --version 5.0.2 --project App --group Test
$ dotnet paket add FsToolkit.ErrorHandling --version 2.2.0 --project App
```

## Apply formatter:
```bash
$ find src/App/ -type f -name "*.fs" -not -path "*obj*" | xargs dotnet fantomas
```

## Install:
```bash
$ dotnet paket install
```

## Run Project:
```bash
$ dotnet run --project src/App
```