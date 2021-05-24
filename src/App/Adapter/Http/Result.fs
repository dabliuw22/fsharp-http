namespace Adapter.Http.Result

open Adapter.Logger
open Suave

module Error =
    type ErrorHandler<'a> = 'a -> WebPart

module Handler =
    let handle<'a, 'b>
        (result: Result<'a, 'b>)
        (successful: 'a -> WebPart)
        (failure: Error.ErrorHandler<'b>)
        : WebPart =
        match result with
        | Ok value -> successful value
        | Error e ->
            Log.error ("Error")
            failure e
