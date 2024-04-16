module PythonService

open System.Net.Http
open Newtonsoft.Json
open Shared

let python_service_api_v1 = Environment.python_service_url + "/api/v1"

// Useful links:
// https://www.newtonsoft.com/json
// https://stackoverflow.com/questions/42000362/creating-a-proxy-to-another-web-api-with-asp-net-core

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