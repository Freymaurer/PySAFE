module App

open Elmish
open Elmish.React
open Fable.Core.JsInterop
open Feliz
open App


let init () =
    let model = Model.init()
    let cmd = Cmd.none
    model, cmd

let update (msg: Msg) (model: Model) =
    match msg with
    | UpdateVersion v -> {model with Version = v}, Cmd.none
    | SetCurrentTime v -> {model with CurrentTimerValue = v}, Cmd.none
    | GetFastAPIMessage ->
        let cmd =
            Cmd.OfAsync.perform
                Api.appApi.GetVersion
                ()
                UpdateVersion
        model, cmd
    | GetFastAPIMessageResponse hw ->
        let nextModel = {model with FastAPIMessage = hw.message}
        nextModel, Cmd.none

[<ReactComponent>]
let private Main (model) dispatch =
    let modalContainer = React.useElementRef()
    React.strictMode [
        React.contextProvider(ReactContext.modalContext, modalContainer, React.fragment [
            Html.div [
                prop.id "modal-container"
                prop.ref modalContainer
            ]
            App.View.Main model dispatch
        ])
    ]

importSideEffects "./index.css"

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update Main
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run