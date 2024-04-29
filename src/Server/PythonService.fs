module PythonService

open System.Net.Http
open Newtonsoft.Json
open Shared

let python_service_api_v1 = Environment.python_service_url + "/api/v1"

open System.Net.WebSockets;
open System.Text;
open System.Threading;
open System.Threading.Tasks;
open System

let pythonServiceHandler() =
    let httpClient = new HttpClient()
    httpClient.BaseAddress <- System.Uri(Environment.python_service_url)
    httpClient.Timeout <- System.TimeSpan(0,5,0)
    httpClient

// Used to serialize Enum Unioncases as strings.
let settings = JsonSerializerSettings()
settings.Converters.Add(Converters.StringEnumConverter())

let helloWorldHandler () =
    task {
        let! response = pythonServiceHandler().GetAsync("/")
        let! content = response.Content.ReadAsStringAsync()
        let r = JsonConvert.DeserializeObject<HelloWorld>(content)
        return r
    }
    |> Async.AwaitTask


open System.Threading.Tasks

// Useful links:
// https://www.newtonsoft.com/json
// https://stackoverflow.com/questions/42000362/creating-a-proxy-to-another-web-api-with-asp-net-core

open Websocket.Client

let inline logws (id: Guid) format =
    Printf.kprintf (fun msg -> printfn "[WS-%A] %s" id msg) format

let subscribeWebsocket (id: System.Guid) (data: obj) =
    let exitEvent = new ManualResetEvent(false)
    async {
        logws id "Subscribing to websocket .." 
        use client = new WebsocketClient(EndPoints.fastApiBrideEndpointURI)
        //client.ReconnectTimeout <- TimeSpan.FromSeconds(30.)
        client.ReconnectionHappened.Subscribe(fun info -> logws id "Reconnection happened, type: %A" info.Type)
        |> ignore
        client.MessageReceived.Subscribe(fun msg ->
            match msg.Text with
            | "EXIT" ->
                logws id "Closing client .."
                client.Stop(WebSocketCloseStatus.NormalClosure, "Task run successfully") |> ignore
                client.Dispose()
                exitEvent.Set() |> ignore
            | msg ->
                logws id "Message received: %A" msg
        )
        |> ignore
        logws id "Starting client .."
        client.Start() |> ignore
        let _ = client.Send(string data)
        exitEvent.WaitOne() |> ignore
    }
    |> Async.Start
