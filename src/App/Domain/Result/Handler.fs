module Domain.Result.Handler

type AsyncResult<'a, 'e> = Async<Result<'a, 'e>>
type AsyncChoice<'a, 'e> = Async<Choice<'a, 'e>>

module AsyncResult =
    let map<'a, 'b, 'e> (f: 'a -> 'b) (result: AsyncResult<'a, 'e>) : AsyncResult<'b, 'e> =
        result |> Async.map (Result.map f)

    let handle<'a, 'b, 'e1, 'e2> (f: 'a -> 'b) (g: 'e1 -> 'e2) (result: AsyncResult<'a, 'e1>) =
        result
        |> Async.map (fun result ->
            match result with
            | Ok v -> Ok(f v)
            | Error error -> Error(g error))

    let apply<'a, 'e> (failure: 'e) (result: AsyncChoice<'a, exn>) : AsyncResult<'a, 'e> =
        result
        |> Async.map (Domain.Error.Handler.handle failure)
