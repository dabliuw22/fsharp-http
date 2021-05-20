module Domain.Result

let handle<'a, 'b> (failure: 'b) (bin: Choice<'a, exn>) : Result<'a, 'b> =
    match bin with
    | Choice1Of2 value -> Ok value
    | Choice2Of2 _ -> Error failure
