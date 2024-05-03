module Environment

open System

[<Literal>]
let private url_key = "PYTHON_SERVICE_URL"
[<Literal>]
let private websocket_endpoint_key = "PYTHON_SERVICE_WS"
[<Literal>]
let private storage_timespan_key = "PYTHON_SERVICE_STORAGE_TIMESPAN"

let python_service_url =
    let def = "http://localhost:8000"
    let nullable = System.Environment.GetEnvironmentVariable(url_key)
    if isNull nullable then def else nullable

let TestGUID = System.Guid.NewGuid()

let python_service_websocket =
    let def = Uri(Shared.EndPoints.fastApiBrideEndpoint)
    let nullable = System.Environment.GetEnvironmentVariable(websocket_endpoint_key)
    if isNull nullable then def else Uri(nullable)

let python_service_storage_timespan =
    let def = TimeSpan.FromHours(1)
    let nullable = System.Environment.GetEnvironmentVariable(websocket_endpoint_key)
    if isNull nullable then def else int(nullable) |> TimeSpan.FromDays