module Bridge.AskQueries

open Shared
open Elmish.Bridge

let askTime(setCurrentTime: string -> unit) =
    async {
        let! result = Bridge.AskServer(fun rc ->ClientToServer.AskTime((),rc))
        logBridge result
        match result with
        | _ -> setCurrentTime result
    }