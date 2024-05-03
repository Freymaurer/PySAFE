module PythonService

open System.Net.Http
open Newtonsoft.Json
open Shared


open System.Net.WebSockets;
open System.Text;
open System.Threading;
open System.Threading.Tasks;
open System

//let python_service_api_v1 = Environment.python_service_url + "/api/v1"
//let pythonServiceHandler() =
//    let httpClient = new HttpClient()
//    httpClient.BaseAddress <- System.Uri(Environment.python_service_url)
//    httpClient.Timeout <- System.TimeSpan(0,5,0)
//    httpClient

//// Used to serialize Enum Unioncases as strings.
//let settings = JsonSerializerSettings()
//settings.Converters.Add(Converters.StringEnumConverter())

//let helloWorldHandler () =
//    task {
//        let! response = pythonServiceHandler().GetAsync("/")
//        let! content = response.Content.ReadAsStringAsync()
//        let r = JsonConvert.DeserializeObject<HelloWorld>(content)
//        return r
//    }
//    |> Async.AwaitTask


//open System.Threading.Tasks

// Useful links:
// https://www.newtonsoft.com/json
// https://stackoverflow.com/questions/42000362/creating-a-proxy-to-another-web-api-with-asp-net-core

open Websocket.Client

let inline logws (id: Guid) format =
    Printf.kprintf (fun msg -> printfn "[%A] %s" id msg) format

type Message = {
    API: string
    Data: obj
}

type ResultType = {
    Results: int list
    Batch: int
}

let subscribeWebsocket (id: System.Guid) (data: DataInput) =
    let dataResponse0 = DataResponse.init(id, data)
    Storage.Storage.Set(id, dataResponse0)
    let exitEvent = new ManualResetEvent(false)
    async {
        logws id "Subscribing to websocket .." 
        use client = new WebsocketClient(Environment.python_service_websocket)
        let closeAll(msg) =
            client.Stop(WebSocketCloseStatus.NormalClosure, msg) |> ignore
            client.Dispose()
            exitEvent.Set() |> ignore
        client.ReconnectionHappened.Subscribe(fun info -> logws id "Reconnection happened, type: %A" info.Type)
        |> ignore
        client.MessageReceived.Subscribe(fun response ->
            try
                let msg = Json.JsonSerializer.Deserialize<Message>(response.Text)
                logws id "Message received: %A" msg.API
                match msg.API with
                | "Exit" ->
                    logws id "Closing client .."
                    closeAll("Task run successfully")
                    logws id "Starting analysis .."
                    Storage.Storage.Update(id, fun current -> { current with Status = DataResponseStatus.AnalysisRunning } )
                    Storage.Storage.Update(id,fun current ->
                        if current.AllItemsProcessed then
                            logws id "Running analysis .."
                            let res = Analysis.runAnalysis current |> Async.RunSynchronously
                            res
                        else
                            { current with Status = DataResponseStatus.Error "Unhandled Error: Not all items processed."}
                    )
                    logws id "Analysis done. Task completed successfully .."
                | "Error" ->
                    logws id "Python Error: %A" msg.Data
                    closeAll("Python error")
                | "DataResponse" ->
                    let responseData: ResultType = Json.JsonSerializer.Deserialize<ResultType>(msg.Data :?> Json.JsonElement)
                    Storage.Storage.Update(id,fun current ->
                        { current with
                            Status = MLRunning(responseData.Batch)
                            ResultData =
                                let newData = responseData.Results |> List.map DataResponseItem.init
                                newData@current.ResultData
                        }
                    ) 
                    logws id "DataResponse received: %A" responseData.Batch
                | msg ->
                    logws id "Unhandled Message received: %A" msg
            with
                | e ->
                    logws id "Error: Unable to parse received message: %A -  %A" e.Message response.Text
                    closeAll(".NET error")
        )
        |> ignore
        logws id "Starting client .."
        client.Start() |> ignore
        let _ =
            logws id "transforming data to json .."
            let json = Json.JsonSerializer.Serialize<DataInput>(data)
            logws id "Sending data .."
            client.Send(json)
        exitEvent.WaitOne() |> ignore
    }
    |> Async.Start
