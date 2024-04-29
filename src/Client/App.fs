module App

open Elmish
open Elmish.React
open Fable.Core.JsInterop
open State

open Feliz

[<ReactComponent>]
let private Main (model) dispatch =
    React.strictMode [
        App.View.Main model dispatch
    ]

importSideEffects "./index.css"

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram State.init State.update Main
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run