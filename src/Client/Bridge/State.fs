module Bridge.State

open Shared
open State
open Elmish

let update (msg: ServerToClient.Msg) (model:Model) =
    match msg with
    | ServerToClient.ServerConnected ->
        let nextModel = {model with ServerConnected = true}
        nextModel, Cmd.none
    | ServerToClient.TellTime newTime ->
        let nextModel = {model with CurrentTimerValue = newTime}
        nextModel, Cmd.none
    | ServerToClient.Error exn ->
        Browser.Dom.window.alert(exn)
        model, Cmd.none
