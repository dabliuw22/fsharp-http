namespace Adapter.Http.Json

open System.Text.Json

module Serializer =
    let serialize<'a> (data: 'a) =
        try
            JsonSerializer.Serialize data |> Some
        with
        | _ -> None

module Deserializer =
    let deserialize<'a> (json: string) =
        try
            JsonSerializer.Deserialize<'a> json |> Some
        with
        | _ -> None
