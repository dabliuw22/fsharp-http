module Adapter.Env.Envs

open System

type FromEnv<'a> =
    abstract member From : unit -> 'a

let getEnv (name: string) : string option =
    let value = Environment.GetEnvironmentVariable name

    if value = null then
        None
    else
        Some value

let getEnvWithDefault<'a> (name: string) (f: string -> 'a) (def: 'a) : 'a =
    defaultArg (Option.map f (getEnv name)) def
