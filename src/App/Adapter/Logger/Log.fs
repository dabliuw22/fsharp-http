namespace Adapter.Logger

open Microsoft.Extensions.Configuration
open Serilog
open Serilog.Configuration
open Serilog.Events
open Serilog.Formatting.Json
open Serilog.Sinks
open Serilog.Enrichers


module Log =

    let template =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{MachineName}] [{Level:u3}] [{CorrelationId}] {Message:lj} {Properties}{NewLine}{Exception}"

    let logger =
        LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .Enrich.WithCorrelationId()
            .Enrich.WithMachineName()
            .WriteTo.Console(outputTemplate = template)
            .CreateLogger()

    type LogAction = string -> unit

    let info : LogAction =
        fun msj -> using logger (fun log -> log.ForContext<Log>().Information msj)

    let error : LogAction =
        fun msj -> using logger (fun log -> log.Error msj)

    let warning : LogAction =
        fun msj -> using logger (fun log -> log.Warning msj)

    let debug : LogAction =
        fun msj -> using logger (fun log -> log.Debug msj)
