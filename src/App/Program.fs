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
let main argv =
    use log = Log.logger ()

    let loggerF = Log.DefaultLoggerHandler(log)

    let logger = loggerF.GetLogger "Program"

    logger.Information "Start Server"

    use connection = FromEnv.DbConfigFromEnv().From().ToConnection()

    connection.Open()

    let db = PgDatabaseHandler(connection)

    let query = Query.DefaultQueryProducts(db)

    let command = Command.DefaultCommandProducts(db)

    let handler = Handler.DefaultProductHandler(query, command)

    let route = ProductRoute(loggerF, handler)

    startWebServer defaultConfig route.App
    0
