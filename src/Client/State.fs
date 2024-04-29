module State

open Shared

type Model = {
    Version: string
    // websocket
    ServerConnected: bool
    CurrentTimerValue: string
    // fastapi
    FastAPIMessage: string
} with
    static member init() = {
            Version = ""
            ServerConnected = false
            CurrentTimerValue = ""
            FastAPIMessage = ""
        }

type Msg =
    | UpdateVersion of string
    /// These are incoming websocket messages
    | SetCurrentTime of string
    | GetFastAPIMessage
    | GetFastAPIMessageResponse of HelloWorld

open Elmish
open Fable.Remoting.Client

let predictionApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IPredictionApiv1>

let appApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IAppApiv1>

let init () =
    let model = Model.init()
    let cmd = Cmd.none
    model, cmd

let update msg model =
    match msg with
    | UpdateVersion v -> {model with Version = v}, Cmd.none
    | SetCurrentTime v -> {model with CurrentTimerValue = v}, Cmd.none
    | GetFastAPIMessage ->
        let cmd =
            Cmd.OfAsync.perform
                appApi.GetVersion
                ()
                UpdateVersion
        model, cmd
    | GetFastAPIMessageResponse hw ->
        let nextModel = {model with FastAPIMessage = hw.message}
        nextModel, Cmd.none