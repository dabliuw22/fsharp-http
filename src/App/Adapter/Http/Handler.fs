namespace Adapter.Http.Handler

open Adapter.Http.Extensions
open Adapter.Http.Json
open Suave

module Request =
    let handle<'a> (request: HttpRequest) (f: 'a -> WebPart) : WebPart =
        let req =
            request.rawForm
            |> System.Text.Encoding.UTF8.GetString
            |> Deserializer.deserialize<'a>

        match req with
        | Some dto -> f dto
        | _ -> (RequestErrors.BAD_REQUEST "").Json()

module Response =
    let handle (response: string option) (f: string -> WebPart) (g: WebPart) =
        match response with
        | Some json -> (json |> f).Json()
        | _ -> g.Json()
