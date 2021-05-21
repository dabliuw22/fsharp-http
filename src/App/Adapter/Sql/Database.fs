namespace Adapter.Sql.Database

open Npgsql.FSharp

module Config =
    type DbConfig =
        { Host: string
          Username: string
          Password: string
          Database: string
          Port: int }

    module DbConfig =
        open System

        let private getEnv (name: string) : string option =
            let value = Environment.GetEnvironmentVariable name

            if value = null then
                None
            else
                Some value

        let fromEnv () : DbConfig =
            let host =
                defaultArg (getEnv "DATABASE_HOST") "localhost"

            let username =
                defaultArg (getEnv "DATABASE_USERNAME") "fsharp"

            let password =
                defaultArg (getEnv "DATABASE_PASSWORD") "fsharp"

            let database =
                defaultArg (getEnv "DATABASE_NAME") "fsharp_db"

            let port =
                (defaultArg (Option.map (fun p -> int p) (getEnv "DATABASE_PORT")) 5432)

            { Host = host
              Username = username
              Password = password
              Database = database
              Port = port }

        let connectionStr
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

    type DatabaseHandler =
        abstract member Query :
            string ->
            ((string * SqlValue) list) ->
            (RowReader -> 'a) ->
            Async<Result<'a list, Error.DatabaseError>>

        abstract member Option :
            string ->
            ((string * SqlValue) list) ->
            (RowReader -> 'a) ->
            Async<Result<'a option, Error.DatabaseError>>

        abstract member Command : string -> ((string * SqlValue) list) -> Async<Result<int, Error.DatabaseError>>

        abstract member CommandRow :
            string ->
            ((string * SqlValue) list) ->
            (RowReader -> 'a) ->
            Async<Result<'a, Error.DatabaseError>>

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
                |> Async.map (Domain.Result.handle (Error.query $"Query Error: {query}"))

            member _.Option query ``params`` mapper =
                conn
                |> Sql.existingConnection
                |> Sql.query query
                |> Sql.parameters ``params``
                |> Sql.executeAsync mapper
                |> Async.AwaitTask
                |> Async.map (List.tryHead)
                |> Async.Catch
                |> Async.map (Domain.Result.handle (Error.query $"Query Error: {query}"))

            member _.Command cmd ``params`` =
                conn
                |> Sql.existingConnection
                |> Sql.query cmd
                |> Sql.parameters ``params``
                |> Sql.executeNonQueryAsync
                |> Async.AwaitTask
                |> Async.Catch
                |> Async.map (Domain.Result.handle (Error.command $"Command Error: {cmd}"))

            member _.CommandRow cmd ``params`` mapper =
                conn
                |> Sql.existingConnection
                |> Sql.query cmd
                |> Sql.parameters ``params``
                |> Sql.executeRowAsync mapper
                |> Async.AwaitTask
                |> Async.Catch
                |> Async.map (Domain.Result.handle (Error.command $"Command Error: {cmd}"))
