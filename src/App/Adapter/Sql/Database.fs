namespace Adapter.Sql.Database

open Npgsql.FSharp

module Config =
    type DbConfig =
        { Host: string
          Username: string
          Password: string
          Database: string
          Port: int }

    module FromEnv =
        open Adapter.Env.Envs

        type DbConfigFromEnv() =
            member this.From = (this :> FromEnv<DbConfig>).From

            interface FromEnv<DbConfig> with
                member _.From() =
                    let host = getEnvWithDefault "DATABASE_HOST" id "localhost"

                    let username = getEnvWithDefault "DATABASE_USERNAME" id "fsharp"

                    let password = getEnvWithDefault "DATABASE_PASSWORD" id "fsharp"

                    let database = getEnvWithDefault "DATABASE_NAME" id "fsharp_db"

                    let port = getEnvWithDefault "DATABASE_PORT" int 5432

                    { Host = host
                      Username = username
                      Password = password
                      Database = database
                      Port = port }

    module DbConfig =
        open Npgsql
        open System.Runtime.CompilerServices

        [<Extension; Sealed>]
        type DatabaseExtension =
            [<Extension>]
            static member inline ToConnectionStr
                (({ Host = host
                    Username = username
                    Password = pass
                    Database = name
                    Port = port }: DbConfig) as _)
                : string =
                Sql.host host
                |> Sql.database name
                |> Sql.username username
                |> Sql.password pass
                |> Sql.port port
                |> Sql.formatConnectionString

            [<Extension>]
            static member inline ToConnection(config: DbConfig) : NpgsqlConnection =
                new NpgsqlConnection(config.ToConnectionStr())

module Error =
    type DatabaseError =
        | CommandError of string
        | QueryError of string

    let command msj = CommandError msj
    let query msj = QueryError msj

module Handler =
    open Domain
    open Domain.Result.Handler

    [<AbstractClass>]
    type DatabaseHandler() =
        abstract member Query:
            string -> ((string * SqlValue) list) -> (RowReader -> 'a) -> AsyncResult<'a list, Error.DatabaseError>

        abstract member Option:
            string -> ((string * SqlValue) list) -> (RowReader -> 'a) -> AsyncResult<'a option, Error.DatabaseError>

        abstract member Command: string -> ((string * SqlValue) list) -> AsyncResult<int, Error.DatabaseError>

        abstract member CommandRow:
            string -> ((string * SqlValue) list) -> (RowReader -> 'a) -> AsyncResult<'a, Error.DatabaseError>

    [<Sealed>]
    type PgDatabaseHandler(conn: Npgsql.NpgsqlConnection) =
        inherit DatabaseHandler()

        override _.Query query ``params`` mapper =
            conn
            |> Sql.existingConnection
            |> Sql.query query
            |> Sql.prepare
            |> Sql.parameters ``params``
            |> Sql.executeAsync mapper
            |> Async.AwaitTask
            |> Async.Catch
            |> AsyncResult.apply (Error.query $"Query Error: {query}")

        override this.Option query ``params`` mapper =
            (this :> DatabaseHandler).Query query ``params`` mapper
            |> AsyncResult.map (List.tryHead)

        override _.Command cmd ``params`` =
            conn
            |> Sql.existingConnection
            |> Sql.query cmd
            |> Sql.prepare
            |> Sql.parameters ``params``
            |> Sql.executeNonQueryAsync
            |> Async.AwaitTask
            |> Async.Catch
            |> AsyncResult.apply (Error.command $"Command Error: {cmd}")

        override _.CommandRow cmd ``params`` mapper =
            conn
            |> Sql.existingConnection
            |> Sql.query cmd
            |> Sql.prepare
            |> Sql.parameters ``params``
            |> Sql.executeRowAsync mapper
            |> Async.AwaitTask
            |> Async.Catch
            |> AsyncResult.apply (Error.command $"Command Error: {cmd}")
