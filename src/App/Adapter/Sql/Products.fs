namespace Adapter.Sql.Products

open Adapter.Sql.Database
open Domain.Products
open Npgsql.FSharp
open System

module Row =
    type ProductRow =
        { Id: string
          Name: string
          Stock: double
          CreatedAt: DateTimeOffset }

    module ProductRow =
        let toProduct
            { Id = id
              Name = name
              Stock = stock
              CreatedAt = offset }
            =
            Data.Product.make id name stock offset

        let fromRow (row: RowReader) : ProductRow =
            { Id = row.string "id"
              Name = row.string "name"
              Stock = row.double "stock"
              CreatedAt = row.datetimeOffset "created_at" }

module Query =

    type DefaultQueryProducts(db: Handler.DatabaseHandler) =
        interface Query.QueryProducts with
            member _.GetAll =
                db.Query "SELECT * FROM products" [] Row.ProductRow.fromRow
                |> Async.map
                    (fun result ->
                        match result with
                        | Ok v -> Ok(List.map Row.ProductRow.toProduct v)
                        | _ -> Error Error.notFounds)

            member _.GetById(Data.ProductId id) =
                db.Option "SELECT * FROM products WHERE id = @id" [ "@id", Sql.string id ] Row.ProductRow.fromRow
                |> Async.map
                    (fun result ->
                        match result with
                        | Ok v -> Ok(Option.map Row.ProductRow.toProduct v)
                        | _ -> Error(Error.notFound id))


module Command =
    type DefaultCommandProducts(db: Handler.DatabaseHandler) =
        interface Command.CommandProducts with
            member _.Create
                { Id = Data.ProductId id
                  Name = Data.ProductName name
                  Stock = Data.ProductStock stock
                  CreatedAt = Data.ProductCreatedAt created }
                =
                db.CommandRow
                    "INSERT INTO products (id, name, stock, created_at) VALUES (@id, @name, @stock, @created) RETURNING *"
                    [ "@id", Sql.string id
                      "@name", Sql.string name
                      "@stock", Sql.double stock
                      "@created", Sql.timestamptz created ]
                    Row.ProductRow.fromRow
                |> Async.map
                    (fun result ->
                        match result with
                        | Ok v -> Ok(Row.ProductRow.toProduct v)
                        | _ -> Error(Error.notCreated id))



            member _.DeleteById(Data.ProductId id) =
                db.Command "DELETE FROM products WHERE id = @id" [ "@id", Sql.string id ]
                |> Async.map
                    (fun result ->
                        match result with
                        | Ok v -> Ok v
                        | _ -> Error(Error.notDeleted id))

            member _.Update
                { Id = Data.ProductId id
                  Name = Data.ProductName name
                  Stock = Data.ProductStock stock }
                =
                db.Command
                    "UPDATE products SET name = @name, stock = @stock WHERE id = @id"
                    [ "@name", Sql.string name
                      "@stock", Sql.double stock
                      "@id", Sql.string id ]
                |> Async.map
                    (fun result ->
                        match result with
                        | Ok v -> Ok v
                        | _ -> Error(Error.notUpdated id))
