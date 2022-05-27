namespace Adapter.Sql.Products

open Adapter.Sql.Database
open Domain.Products
open Domain.Result.Handler
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
    [<Literal>]
    let private findAll = "SELECT * FROM products"

    [<Literal>]
    let private findById = "SELECT * FROM products WHERE id = @id"

    type DefaultQueryProducts(db: Handler.DatabaseHandler) =
        interface Query.QueryProducts with
            member _.GetAll =
                db.Query findAll [] Row.ProductRow.fromRow
                |> AsyncResult.handle (fun v -> List.map Row.ProductRow.toProduct v) (fun _ -> Error.notFounds)

            member _.GetById(Data.ProductId id) =
                db.Option findById [ "@id", Sql.string id ] Row.ProductRow.fromRow
                |> AsyncResult.handle (fun v -> Option.map Row.ProductRow.toProduct v) (fun _ -> Error.notFound id)


module Command =
    [<Literal>]
    let private save =
        """
        INSERT INTO products (id, name, stock, created_at) 
        VALUES (@id, @name, @stock, @created) 
        RETURNING *
    """

    [<Literal>]
    let private deleteById = "DELETE FROM products WHERE id = @id"

    [<Literal>]
    let private update =
        """
        UPDATE products 
        SET name = @name, stock = @stock 
        WHERE id = @id
    """

    type DefaultCommandProducts(db: Handler.DatabaseHandler) =
        interface Command.CommandProducts with
            member _.Create
                { Id = Data.ProductId id
                  Name = Data.ProductName name
                  Stock = Data.ProductStock stock
                  CreatedAt = Data.ProductCreatedAt created }
                =
                db.CommandRow
                    save
                    [ "@id", Sql.string id
                      "@name", Sql.string name
                      "@stock", Sql.double stock
                      "@created", Sql.timestamptz created ]
                    Row.ProductRow.fromRow
                |> AsyncResult.handle (fun v -> Row.ProductRow.toProduct v) (fun _ -> Error.notCreated id)


            member _.DeleteById(Data.ProductId pId) =
                db.Command deleteById [ "@id", Sql.string pId ]
                |> AsyncResult.handle id (fun _ -> Error.notDeleted pId)

            member _.Update
                { Id = Data.ProductId pId
                  Name = Data.ProductName name
                  Stock = Data.ProductStock stock }
                =
                db.Command
                    update
                    [ "@name", Sql.string name
                      "@stock", Sql.double stock
                      "@id", Sql.string pId ]
                |> AsyncResult.handle id (fun _ -> Error.notUpdated pId)
