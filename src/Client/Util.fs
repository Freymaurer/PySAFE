[<AutoOpenAttribute>]
module Util

let log = fun o -> Browser.Dom.console.log(o)

let logBridge = fun (o:obj) -> Browser.Dom.console.log("[WebSocket]", o)