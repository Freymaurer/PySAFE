module Bridge.State

open Elmish
open Elmish.Bridge
open System
open System.Timers
open Shared

let printBridge format = 
    Printf.ksprintf(fun s ->
        "[Bridge] " + s |> printfn "%s"
    ) format

type Model = {
    Timer: Timer option
    Start: DateTime option
} with
    static member init() =
        {
            Start = None
            Timer = None
        }
    member this.DisposeTimer() =
        match this with
        | {Timer = Some timer} -> timer.Stop(); timer.Dispose()
        | _ -> ()

let init(clientDisaptch:Dispatch<ServerToClient.Msg>) () =
    clientDisaptch ServerToClient.ServerConnected
    Model.init(), Cmd.none

type Msg =
| TellCurrentTimer of signalTime: DateTime
| Reset
| Remote of ClientToServer.Msg

let update (clientDispatch:Dispatch<ServerToClient.Msg>) (msg: Msg) (model: Model) =
    match msg with
    | TellCurrentTimer signalTime ->
        let diff = signalTime - model.Start.Value
        clientDispatch (ServerToClient.TellTime <| diff.ToString())
        model, Cmd.none
    | Reset ->
        printBridge "---CLOSED---"
        model.DisposeTimer()
        Model.init(), Cmd.none
    | Remote msg ->
        match msg with
        | ClientToServer.StartTimer ->
            model.DisposeTimer()
            let timer = new Timer(100.)
            let nextModel = 
                {
                    Start = Some DateTime.Now
                    Timer = Some timer
                }
            let cmd = Cmd.ofEffect (fun dispatch ->
                timer.Elapsed.Add(fun args ->
                    TellCurrentTimer args.SignalTime |> dispatch
                )
                timer.Start()
            )
            nextModel, cmd
        | ClientToServer.StopTimer ->
            match model.Timer with
            | Some timer ->
                model.DisposeTimer()
                Model.init(), Cmd.none
            | None ->
                model, Cmd.none


