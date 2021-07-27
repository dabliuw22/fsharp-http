namespace Adapter.Http.Json

open System.Text.Json

module Policy =
    open System
    open System.Linq
    open System.Runtime.CompilerServices

    [<Extension>]
    type private JsonNamingExtension =
        [<Extension>]
        static member inline ToSnakeCase(name: string) =
            String.Concat(
                name.Select
                    (fun x i ->
                        if Char.IsUpper x then
                            let lower = x |> Char.ToLower |> Char.ToString
                            if i > 0 then "_" + lower else lower
                        else
                            Char.ToString x)
            )

    type private ToSnakeCase() =
        inherit JsonNamingPolicy()
        override _.ConvertName(name: string) = name.ToSnakeCase()


    let toOptions =
        JsonSerializerOptions(PropertyNamingPolicy = ToSnakeCase())

module Serializer =

    let serialize<'a> (data: 'a) =
        try
            // JsonSerializer.Serialize(data, toOptions) |> Some
            JsonSerializer.Serialize data |> Some
        with
        | _ -> None

module Deserializer =

    let deserialize<'a> (json: string) =
        try
            JsonSerializer.Deserialize<'a> json |> Some
        with
        | _ -> None
