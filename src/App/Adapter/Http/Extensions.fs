module Adapter.Http.Extensions

open Suave
open Suave.Operators
open Suave.Writers
open System.Runtime.CompilerServices

let ApplicationJson = "application/json; charset=utf-8"

[<Extension>]
type WebPartExtension =
    [<Extension>]
    static member inline Json(webPart: HttpContext -> Async<HttpContext option>) =
        webPart >=> setMimeType ApplicationJson
