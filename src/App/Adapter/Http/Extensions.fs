module Adapter.Http.Extensions

open Suave
open Suave.Operators
open Suave.Writers
open System.Runtime.CompilerServices

[<Literal>]
let ApplicationJson = "application/json; charset=utf-8"

[<Extension; Sealed>]
type WebPartExtension =
    [<Extension>]
    static member inline Json(webPart: HttpContext -> Async<HttpContext option>) =
        webPart >=> setMimeType ApplicationJson
