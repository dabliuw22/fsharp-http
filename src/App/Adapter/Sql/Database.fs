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
                    let host =
                        getEnvWithDefault "DATABASE_HOST" id "localhost"

                    let username =
                        getEnvWithDefault "DATABASE_USERNAME" id "fsharp"

                    let password =
                        getEnvWithDefault "DATABASE_PASSWORD" id "fsharp"

                    let database =
                        getEnvWithDefault "DATABASE_NAME" id "fsharp_db"

                    let port =
                        getEnvWithDefault "DATABASE_PORT" int 5432

                    { Host = host
                      Username = username
                      Password = password
                      Database = database
                      Port = port }

    module DbConfig =

        let toConnectionStr
            { Host = host
              Username = username
              Password = pass
              Database = name
              Port = port }
            =
            Sql.host host
            |> Sql.database name
            |> Sql.username username
            |> Sql.password pass
            |> Sql.port port
            |> Sql.formatConnectionString

module Error =
    type DatabaseError =
        | CommandError of string
        | QueryError of string

    let command msj = CommandError msj
    let query msj = QueryError msj

module Handler =
    open Domain

    type DatabaseHandler =
        abstract member Query :
            string -> ((string * SqlValue) list) -> (RowReader -> 'a) -> Async<Result<'a list, Error.DatabaseError>>

        abstract member Option :
            string -> ((string * SqlValue) list) -> (RowReader -> 'a) -> Async<Result<'a option, Error.DatabaseError>>

        abstract member Command : string -> ((string * SqlValue) list) -> Async<Result<int, Error.DatabaseError>>

        abstract member CommandRow :
            string -> ((string * SqlValue) list) -> (RowReader -> 'a) -> Async<Result<'a, Error.DatabaseError>>

    type PgDatabaseHandler(conn: Npgsql.NpgsqlConnection) =
        interface DatabaseHandler with
            member _.Query query ``params`` mapper =
                conn
                |> Sql.existingConnection
                |> Sql.query query
                |> Sql.parameters ``params``
                |> Sql.executeAsync mapper
                |> Async.AwaitTask
                |> Async.Catch
                |> Async.map (Error.Handler.handle (Error.query $"Query Error: {query}"))

            member this.Option query ``params`` mapper =
                (this :> DatabaseHandler).Query query ``params`` mapper
                |> Async.map (Result.map List.tryHead)

            member _.Command cmd ``params`` =
                conn
                |> Sql.existingConnection
                |> Sql.query cmd
                |> Sql.parameters ``params``
                |> Sql.executeNonQueryAsync
                |> Async.AwaitTask
                |> Async.Catch
                |> Async.map (Error.Handler.handle (Error.command $"Command Error: {cmd}"))

            member _.CommandRow cmd ``params`` mapper =
                conn
                |> Sql.existingConnection
                |> Sql.query cmd
                |> Sql.parameters ``params``
                |> Sql.executeRowAsync mapper
                |> Async.AwaitTask
                |> Async.Catch
                |> Async.map (Error.Handler.handle (Error.command $"Command Error: {cmd}"))
