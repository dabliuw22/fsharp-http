namespace Adapter.Http.Json

open System.Text.Json

module Serializer =
    let serialize<'A> (data: 'A) =
        try
            JsonSerializer.Serialize data |> Some
        with _ -> None

module Deserializer =
    let deserialize<'A> (json: string) =
        try
            JsonSerializer.Deserialize<'A> json |> Some
        with _ -> None
