[<AutoOpenAttribute>]
module Util

let log = fun o -> Browser.Dom.console.log(o)

let logBridge = fun (o:obj) -> Browser.Dom.console.log("[WebSocket]", o)

open Fable.Core.JsInterop

[<RequireQualifiedAccess>]
module StaticFile =

    /// Function that imports a static file by it's relative path.
    let inline import (path: string) : string = importDefault<string> path