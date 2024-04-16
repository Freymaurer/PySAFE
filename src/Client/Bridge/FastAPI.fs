module Bridge.FastAPI

open Fable.Core
open Fable.Core.JsInterop
open Shared

[<Emit("new WebSocket($0)")>]
let create (url: string): Browser.Types.WebSocket = jsNative
