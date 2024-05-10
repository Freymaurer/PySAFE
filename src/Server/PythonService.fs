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

type RequestType = {
    Fasta: DataInputItem []
} with
    static member init(items: DataInputItem []) = { Fasta = items }

type ResponseType = {
    Results: DataResponseItem list
    Batch: int
}

let subscribeWebsocket (dro: DataResponse) =
    let id = dro.Id
    Storage.Storage.Set(dro.Id, dro)
    let exitEvent = new ManualResetEvent(false)
    async {
        logws id "Subscribing to websocket .."
        use client = new WebsocketClient(Environment.python_service_websocket)
        let closeAll(msg) =
            logws id "%s" msg
            client.Dispose()
            exitEvent.Set() |> ignore
        client.ReconnectionHappened.Subscribe(fun info ->
            match info.Type with
            | ReconnectionType.Lost ->
                Storage.Storage.Update(id,fun current -> { current with Status = DataResponseStatus.Error "Connection lost." })
                closeAll("Connection lost.")
            | _ ->
                logws id "Reconnection happened, type: %A" info.Type
        )
        |> ignore
        client.DisconnectionHappened.Subscribe(fun info ->
            match info.Type with
            | DisconnectionType.Error ->
                Storage.Storage.Update(id,fun current -> { current with Status = DataResponseStatus.Error "Disconnection.Error happened." })
                closeAll("Disconnection.Error happened.")
            | _ ->
                logws id "Disconnection happened, type: %A" info.Type
        )
        |> ignore
        client.MessageReceived.Subscribe(fun response ->
            try
                let msg = Json.JsonSerializer.Deserialize<Message>(response.Text)
                logws id "Message received: %A" msg.API
                match msg.API with
                | "Exit" ->
                    closeAll("Closing client ..")
                    logws id "Starting analysis .."
                    Storage.Storage.Update(id, fun current -> { current with Status = DataResponseStatus.AnalysisRunning } )
                    Storage.Storage.Update(id, fun current ->
                        if current.AllItemsProcessed then
                            logws id "Running analysis .."
                            let analysisResult =
                                try
                                    let res = Analysis.runAnalysis current |> Async.RunSynchronously
                                    res
                                with
                                    | e -> {current with Status = DataResponseStatus.Error e.Message}
                            analysisResult
                        else
                            { current with Status = DataResponseStatus.Error "Unhandled Error: Not all items processed."}
                    )
                    //email
                    match (Storage.EmailStorage.TryGet id) with
                    | Some email -> 
                        logws id "Sending notification email .."
                        Email.sendNotification email
                        |> Async.RunSynchronously
                    | None ->
                        logws id "Not subscribed to notification service .."
                    logws id "Analysis done. Task completed successfully .."
                | "Error" ->
                    logws id "Python Error: %A" msg.Data
                    closeAll("Python error")
                | "DataResponse" ->
                    logws id "DataResponse received ..: %A" msg.Data
                    let responseData: ResponseType = Json.JsonSerializer.Deserialize<ResponseType>(msg.Data :?> Json.JsonElement)
                    logws id "DataResponse received: %A" responseData
                    Storage.Storage.Update(id,fun current ->
                        { current with
                            Status = DataResponseStatus.MLRunning(responseData.Batch)
                            ResultData =
                                let newData = responseData.Results
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
        //if client.IsRunning then
        logws id "transforming data to json .."
        let bytes = Encoding.UTF8.GetBytes(Json.JsonSerializer.Serialize<RequestType>(RequestType.init dro.InitData.Items))
        client.Start() |> ignore
        Storage.Storage.Update(id,fun current -> { current with Status = DataResponseStatus.Starting})
        logws id "Sending data .."
        client.Send(bytes) |> ignore
        //else
        //    Storage.Storage.Update(id,fun current -> { current with Status = DataResponseStatus.Error "Unable to connect to ml service." })
        //    closeAll("Unable to connect to ml service.")
            
        exitEvent.WaitOne() |> ignore
    }
    |> Async.Start
