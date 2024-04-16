module App

open Elmish
open Elmish.React
open Fable.Core.JsInterop
open Elmish.Bridge
open State

importSideEffects "./index.css"

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

let fromServer (msg: Shared.ServerToClient.Msg) =
    logBridge ("newMessage", msg)
    match msg with
    | _ -> Remote msg


Program.mkProgram Index.init Index.update Index.view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withBridgeConfig (
   Bridge.endpoint Shared.EndPoints.endpoint
   |> Bridge.withMapping fromServer )
|> Program.withReactSynchronous "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run