open Suave
open Adapter.Logger
open Adapter.Http.Products.Route
open Application.Products
open Adapter.Sql.Database.Config
open Adapter.Sql.Database.Handler
open Adapter.Sql.Products
open Npgsql

[<EntryPoint>]
let main argv =

    let _ = Log.info "Start Server"

    use connection =
        new NpgsqlConnection(
            (FromEnv.DbConfigFromEnv().From
             >> DbConfig.toConnectionStr)
                ()
        )

    connection.Open()

    let db = new PgDatabaseHandler(connection)

    let query = new Query.DefaultQueryProducts(db)

    let command = new Command.DefaultCommandProducts(db)

    let handle =
        new Handle.DefaultProductsHandle(query, command)

    startWebServer defaultConfig (app handle)
    0
