module Environment

open System

type internal EmailConfig = {
    Email: string
    AccountName: string
    Password: string
    Server: string
    Port: int
}

module EmailKeys =
    [<Literal>]
    let email_key = "NET_EMAIL_EMAIL"
    [<Literal>]
    let account_name_key = "NET_EMAIL_ACCOUNTNAME"
    [<Literal>]
    let password_key = "NET_EMAIL_PASSWORD"
    [<Literal>]
    let server_key = "NET_EMAIL_SERVER"
    [<Literal>]
    let port_key = "NET_EMAIL_PORT"

open Microsoft.Extensions.Configuration

let mutable missingEmailCredentials = false
let internal EmailConfig =
    let config = (new ConfigurationBuilder()).AddUserSecrets("adad89d0-732c-4735-8c06-0b0000a3b1d9").Build()
    let accessKey(key: string) =
        let userSecret = config.[sprintf "email:%s" key]
        let envVariable = System.Environment.GetEnvironmentVariable(key)
        match userSecret, envVariable with
        | null, null ->
            let c0 = Console.ForegroundColor
            Console.ForegroundColor <- ConsoleColor.Yellow
            missingEmailCredentials <- true
            Console.WriteLine "[WARNING]: No email credentials found, email functionality disabled."
            Console.ForegroundColor <- c0
            ""
        | v, null -> v
        | _, v -> v
    {
        Email = accessKey EmailKeys.email_key
        AccountName = accessKey EmailKeys.account_name_key
        Password = accessKey EmailKeys.password_key
        Server = accessKey EmailKeys.server_key
        Port = accessKey EmailKeys.port_key |> int
    }

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
    let nullable = System.Environment.GetEnvironmentVariable(storage_timespan_key)
    if isNull nullable then def else int(nullable) |> TimeSpan.FromDays