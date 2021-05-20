namespace Domain.Products

open System

module Data =
    type ProductId =
        | ProductId of string
        override this.ToString() =
            match this with
            | ProductId v -> "ProductId { " + v + " }"

    module ProductId =
        let inline make value = ProductId value
        let inline gen () = make (Guid.NewGuid().ToString())

    type ProductName =
        | ProductName of string
        override this.ToString() =
            match this with
            | ProductName v -> "ProductName { " + v + " }"

    module ProductName =
        let inline make value = ProductName value

    type ProductStock =
        | ProductStock of double
        override this.ToString() =
            match this with
            | ProductStock v -> "ProductStock { " + v.ToString() + " }"

    module ProductStock =
        let inline make value = ProductStock value

    type ProductCreatedAt =
        | ProductCreatedAt of DateTimeOffset
        override this.ToString() =
            match this with
            | ProductCreatedAt v -> "ProductCreatedAt { " + v.ToString() + " }"

    module ProductCreatedAt =
        let inline make value = ProductCreatedAt value
        let inline gen () = ProductCreatedAt(DateTimeOffset.Now)

    type Product =
        { Id: ProductId
          Name: ProductName
          Stock: ProductStock
          CreatedAt: ProductCreatedAt }
        override this.ToString() =
            "Product { Id = "
            + this.Id.ToString()
            + ", Name = "
            + this.Name.ToString()
            + ", Stock = "
            + this.Stock.ToString()
            + ", CreatedAt ="
            + this.CreatedAt.ToString()
            + " }"

    module Product =
        let inline make id name stock created =
            { Id = ProductId.make id
              Name = ProductName.make name
              Stock = ProductStock.make stock
              CreatedAt = ProductCreatedAt.make created }

        let inline gen name stock =
            { Id = ProductId.gen ()
              Name = ProductName.make name
              Stock = ProductStock.make stock
              CreatedAt = ProductCreatedAt.gen () }

module Error =
    type ProductError =
        | ProductNotFound of string
        | ProductsNotFound of string
        | ProductNotCreated of string
        | ProductNotUpdated of string
        | ProductNotDeleted of string

    let notFound (id: string) =
        ProductNotFound $"Not found product: {id}"

    let notFounds = ProductsNotFound "Not found products"

    let notCreated (id: string) =
        ProductsNotFound $"Error creating the product: {id}"

    let notDeleted (id: string) =
        ProductNotDeleted $"Error removing the product: {id}"

    let notUpdated (id: string) =
        ProductNotUpdated $"Error updating product: {id}"

module Query =
    type QueryProducts =
        abstract member GetAll : Async<Result<Data.Product list, Error.ProductError>>
        abstract member GetById : Data.ProductId -> Async<Result<Data.Product option, Error.ProductError>>

module Command =
    type CommandProducts =
        abstract member Create : Data.Product -> Async<Result<Data.Product, Error.ProductError>>
        abstract member DeleteById : Data.ProductId -> Async<Result<int, Error.ProductError>>
        abstract member Update : Data.Product -> Async<Result<int, Error.ProductError>>
