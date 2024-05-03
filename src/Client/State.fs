namespace App

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