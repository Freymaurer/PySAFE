module Api

open Shared
open Fable.Remoting.Client

let predictionApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IPredictionApiv1>

let appApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IAppApiv1>

let lsApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.withBinarySerialization
    |> Remoting.buildProxy<ILargeFileApiv1>