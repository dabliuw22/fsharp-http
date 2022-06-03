open Suave
open Adapter.Logger
open Adapter.Http.Products.Route
open Application.Products
open Adapter.Sql.Database.Config
open Adapter.Sql.Database.Config.DbConfig
open Adapter.Sql.Database.Handler
open Adapter.Sql.Products
open Npgsql

[<EntryPoint>]
let main _ =
    use log = Log.logger ()

    let loggerF = log |> Log.DefaultLoggerHandler

    let logger = loggerF.GetLogger "Program"

    logger.Information "Start Server"

    use connection = FromEnv.DbConfigFromEnv().From().ToConnection()

    connection.Open()

    let db = connection |> PgDatabaseHandler

    let query = Query.DefaultQueryProducts(db)

    let command = Command.DefaultCommandProducts(db)

    let handler = Handler.DefaultProductHandler(query, command)

    let route = ProductRoute(loggerF, handler)

    route.App |> startWebServer defaultConfig
    0
