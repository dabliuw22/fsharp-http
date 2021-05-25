namespace Adapter.Http.Products

open Adapter.Http.Extensions
open Adapter.Http.Json
open Adapter.Http.Handler
open Adapter.Http.Result
open Adapter.Logger
open Application.Products
open Domain.Products
open Suave
open Suave.Filters
open Suave.Operators
open System

module Data =
    type ProductQueryDto =
        { id: string
          name: string
          stock: double
          created_at: DateTimeOffset }
        override this.ToString() =
            "ProductDto { id = "
            + this.id
            + ", name = "
            + this.name
            + ", stock = "
            + this.stock.ToString()
            + ", created_at = "
            + this.created_at.ToString()
            + " }"

    module ProductQueryDto =
        let make i n s c =
            { id = i
              name = n
              stock = s
              created_at = c }

        let fromProduct
            ({ Id = (Data.ProductId id)
               Name = (Data.ProductName name)
               Stock = (Data.ProductStock stock)
               CreatedAt = Data.ProductCreatedAt created }: Data.Product)
            : ProductQueryDto =
            make id name stock created

    type ProductCommandDto = { name: string; stock: double }

    module ProductCommandDto =
        let toProduct id dto = Data.Product.make id dto.name dto.stock
        let toProductGen dto = Data.Product.gen dto.name dto.stock

module Error =

    let handle : Error.ErrorHandler<Error.ProductError> =
        fun error ->
            match error with
            | Error.ProductNotFound _ -> (RequestErrors.NOT_FOUND "").Json()
            | Error.ProductsNotFound _ -> (Successful.OK "[]").Json()
            | Error.ProductNotCreated _ -> (RequestErrors.CONFLICT "").Json()
            | Error.ProductNotDeleted _ -> (RequestErrors.NOT_FOUND "").Json()
            | Error.ProductNotUpdated _ -> (RequestErrors.CONFLICT "").Json()

module Route =

    let productsHandler (handle: Handle.ProductsHandle) : WebPart =
        fun (ctx: HttpContext) ->
            async {
                let _ = Log.info "Get All Products"

                let! result =
                    handle.GetAll
                    |> Async.map (Result.map (List.map Data.ProductQueryDto.fromProduct))
                    |> Async.map (Result.map Serializer.serialize)

                return!
                    (Handler.handle
                        result
                        (fun json ->
                            Response.handle json (fun body -> Successful.OK body) (ServerErrors.INTERNAL_ERROR ""))
                        (Error.handle))
                        ctx
            }

    let productByIdHandler (handle: Handle.ProductsHandle) (id: string) : WebPart =
        fun (ctx: HttpContext) ->
            async {
                let _ = Log.info $"Get Product By Id: {id}"

                let! result =
                    Data.ProductId id
                    |> handle.GetById
                    |> Async.map (Result.map Data.ProductQueryDto.fromProduct)
                    |> Async.map (Result.map Serializer.serialize)

                return!
                    Handler.handle<string option, Error.ProductError>
                        result
                        (fun json ->
                            Response.handle json (fun json -> Successful.OK json) (ServerErrors.INTERNAL_ERROR ""))
                        (Error.handle)
                        ctx
            }

    let createHandler (handle: Handle.ProductsHandle) (request: HttpRequest) : WebPart =
        let _ = Log.info "Create Product"

        let f (dto: Data.ProductCommandDto) =
            let result =
                Data.ProductCommandDto.toProductGen dto
                |> handle.Create
                |> Async.map (Result.map Data.ProductQueryDto.fromProduct)
                |> Async.map (Result.map Serializer.serialize)
                |> Async.RunSynchronously

            Handler.handle<string option, Error.ProductError>
                result
                (fun response ->
                    Response.handle response (fun json -> Successful.CREATED json) (ServerErrors.INTERNAL_ERROR ""))
                (Error.handle)

        Request.handle<Data.ProductCommandDto> request f

    let deleteHandler (handle: Handle.ProductsHandle) (id: string) : WebPart =
        fun (ctx: HttpContext) ->
            async {
                let _ = Log.info $"Delete Product By Id: {id}"

                let! result = Data.ProductId id |> handle.DeleteById

                return!
                    Handler.handle<int, Error.ProductError>
                        result
                        (fun _ -> (Successful.ACCEPTED "").Json())
                        (Error.handle)
                        ctx
            }

    let app (handle: Handle.ProductsHandle) : HttpContext -> Async<HttpContext option> =
        choose [ GET
                 >=> (path "/products" >=> productsHandler handle)
                 GET
                 >=> (pathScan "/products/%s" (fun id -> productByIdHandler handle id))
                 POST
                 >=> (path "/products"
                      >=> request (fun req -> createHandler handle req))
                 DELETE
                 >=> (pathScan "/products/%s" (fun id -> deleteHandler handle id))
                 (RequestErrors.NOT_FOUND "").Json() ]
