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
        match model.Timer with
        | Some timer -> timer.Dispose()
        | None -> ()
        Model.init(), Cmd.none
    | Remote msg ->
        match msg with
        | ClientToServer.StartTimer ->
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
                timer.Stop()
                timer.Dispose()
                {model with Timer = None}, Cmd.none
            | None ->
                model, Cmd.none
        |> fun (model, cmd) ->
            printBridge "%A" model
            model, cmd


