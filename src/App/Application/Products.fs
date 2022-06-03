namespace Application.Products

open Domain.Products
open FSharpPlus

module Handler =
    type ProductHandler =
        abstract member GetAll: Async<Result<Data.Product list, Error.ProductError>>
        abstract member GetById: Data.ProductId -> Async<Result<Data.Product, Error.ProductError>>
        abstract member Create: Data.Product -> Async<Result<Data.Product, Error.ProductError>>
        abstract member DeleteById: Data.ProductId -> Async<Result<int, Error.ProductError>>
        abstract member Update: Data.Product -> Async<Result<int, Error.ProductError>>

    type DefaultProductHandler(query: Query.QueryProducts, command: Command.CommandProducts) =
        member this.GetAll = (this :> ProductHandler).GetAll
        member this.GetById = (this :> ProductHandler).GetById
        member this.Create = (this :> ProductHandler).Create
        member this.DeleteById = (this :> ProductHandler).DeleteById
        member this.Update = (this :> ProductHandler).Update

        interface ProductHandler with
            member _.GetAll =
                monad {
                    let! products = query.GetAll

                    return (Result.bindError (fun _ -> Ok([])) products)
                }

            member _.GetById((Data.ProductId value) as id) =
                async {
                    let! product = query.GetById id

                    let result =
                        match product with
                        | Ok (Some value) -> (Ok value)
                        | Ok (None) -> Error(Error.notFound value)
                        | Error (error) -> Error error

                    return result
                }

            member _.Create product = command.Create product

            member _.DeleteById((Data.ProductId value) as id) =
                async {
                    let! deleted = command.DeleteById id

                    let result =
                        match deleted with
                        | Ok (count) as ok ->
                            if count < 1 then
                                Error(Error.notDeleted value)
                            else
                                ok
                        | error -> error

                    return result
                }

            member _.Update
                ({ Id = Data.ProductId id
                   Name = _
                   Stock = _
                   CreatedAt = _ } as product)
                =
                async {
                    let! updated = command.Update product

                    let result =
                        match updated with
                        | Ok (count) as ok ->
                            if count < 1 then
                                Error(Error.notUpdated id)
                            else
                                ok
                        | error -> error

                    return result
                }
