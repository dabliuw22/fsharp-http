namespace Adapter.Logger

open Serilog
open Serilog.Formatting.Json

module Config =
    type LogConfig = { Template: string; FilePath: string }

    module FromEnv =
        open Adapter.Env.Envs

        [<Literal>]
        let private logTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{MachineName}] [{Level:u3}] [{CorrelationId}] {Message:lj} {Properties}{NewLine}{Exception}"

        [<Literal>]
        let private localPath = "logs/log.json"

        type LogConfigFromEnv() =
            member this.From = (this :> FromEnv<LogConfig>).From

            interface FromEnv<LogConfig> with
                member _.From() =
                    let template = getEnvWithDefault "LOG_TEMPLATE" id logTemplate

                    let filePath = getEnvWithDefault "LOG_FILE_PATH" id localPath

                    { Template = template
                      FilePath = filePath }

    module LogConfig =
        open System.Runtime.CompilerServices

        [<Extension; Sealed>]
        type LogExtension =
            [<Extension>]
            static member inline ToLoggerConfiguration
                (({ Template = template; FilePath = path }: LogConfig) as _)
                : Core.Logger =
                LoggerConfiguration()
                    .MinimumLevel.Information()
                    .Enrich.WithThreadId()
                    .Enrich.WithThreadName()
                    .Enrich.WithCorrelationId()
                    .Enrich.WithMachineName()
                    .WriteTo
                    .Async(fun writer ->
                        writer.Console(outputTemplate = template)
                        |> ignore)
                    .WriteTo.Async(fun writer -> writer.File(JsonFormatter(), path) |> ignore)
                    .CreateLogger()

module Log =
    open Config.LogConfig
    open System.Runtime.CompilerServices

    let private logConfig = Config.FromEnv.LogConfigFromEnv().From()

    let logger () : Core.Logger = logConfig.ToLoggerConfiguration()

    type LogAction = string -> string -> unit

    [<Extension; Sealed>]
    type LoggerExtension =
        [<Extension>]
        static member inline GetLogger(log: Core.Logger) =
            fun (ctx: string) -> log.ForContext("SourceContext", ctx)

    type LoggerHandler =
        abstract member GetLogger: string -> ILogger

    type DefaultLoggerHandler(log: Core.Logger) =
        member this.GetLogger = (this :> LoggerHandler).GetLogger

        interface LoggerHandler with
            member _.GetLogger ctx = log.GetLogger () ctx


    let info: LogAction =
        fun ctx msj ->
            using (logger ()) (fun log ->
                log
                    .ForContext("SourceContext", ctx)
                    .Information msj)

    let error: LogAction =
        fun ctx msj -> using (logger ()) (fun log -> log.ForContext("SourceContext", ctx).Error msj)

    let warning: LogAction =
        fun ctx msj -> using (logger ()) (fun log -> log.ForContext("SourceContext", ctx).Warning msj)

    let debug: LogAction =
        fun ctx msj -> using (logger ()) (fun log -> log.ForContext("SourceContext", ctx).Debug msj)
