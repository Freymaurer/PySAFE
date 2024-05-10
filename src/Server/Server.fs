module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn
open System
open System.Collections.Generic

open Shared
open Giraffe
open Microsoft.Extensions
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.WebSockets
open System.Net.WebSockets
open Microsoft.Extensions.Configuration
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting

let errorHandler (ex: Exception) (routeInfo: RouteInfo<HttpContext>) : ErrorResult = 
    // do some logging
    printfn "Error at %s on method %s" routeInfo.path routeInfo.methodName
    Propagate (sprintf "%s" ex.Message)

[<LiteralAttribute>]
let DefaultAnoFail = "No data accessible."

[<LiteralAttribute>]
let AppVersion = "0.0.1"

let lsAPIv1 (ctx: HttpContext): ILargeFileApiv1 = {
    UploadFile = fun (bytes) -> async {
        match ctx.GetRequestHeader("X-GUID") with
        | Ok guid ->
            let guid = System.Guid.Parse(guid)
            match Storage.Storage.TryGet guid with
            | Some dro0 ->
                async {
                    let dataRaw = System.Text.Encoding.ASCII.GetString bytes
                    let! validationResult = ValidateData.Main(dataRaw)
                    match validationResult with
                    | Ok data0 ->
                        let data = {dro0 with InitData.Items = data0}
                        PythonService.subscribeWebsocket data
                    | Error exn ->
                        Storage.Storage.Update(guid, fun dr -> {dr with Status = DataResponseStatus.Error (exn.Message)})
                } |> Async.Start
                ()
            | None ->
                failwith DefaultAnoFail
        | Error exn ->
            failwith exn
        return ()
    }
}

let appAPIv1: IAppApiv1 = {
    GetVersion = fun () -> async { return AppVersion }
    Log = fun () -> async {
        printfn "--LOG_START--"
        Storage.Storage.Get(Environment.TestGUID) |> printfn "%A"
        printfn "--LOG_END--"
        return ()
    }
}

let predictionAPIv1: IPredictionApiv1 = {
    StartEvaluation = fun data ->
        let guid = Storage.generateNewGuid()
        async {
            let dro = DataResponse.init (guid, data)
            PythonService.subscribeWebsocket dro
            return guid
        }
    PutEmail = fun (id, email) ->
        async {
            match Storage.Storage.TryGet id with
            | Some {Email = None} -> // Only send emails if not already sent
                Storage.Storage.Update (id, fun dr -> {dr with Email = Some email})
                Email.sendConfirmation email
                if Storage.Storage.Get (id) |> _.IsExited then
                    Email.sendNotification email
            | _ -> ()
        }
    GetStatus = fun id ->
        async {
            match Storage.Storage.TryGet id with
            | Some dr ->
                return dr.Status
            | None ->
                return DataResponseStatus.Error ("No status found")
        }
    GetData = fun id ->
        async {
            let dto =
                match Storage.Storage.TryGet id with
                | Some dr ->
                    let dto: DataResponseDTO = { Data = dr.ResultData }
                    if dr.Status <> DataResponseStatus.Finished then
                        failwith DefaultAnoFail
                    dto
                | None ->
                    failwith DefaultAnoFail
            return dto
        }
    PutConfig = fun config ->
        async {
            let guid = Storage.generateNewGuid()
            let dro =
                {
                    Id = guid
                    Email = None
                    Status = DataResponseStatus.Validating
                    InitData = { Items = [||]; Config = config}
                    ResultData = []
                }
            Storage.Storage.Set(guid, dro)
            return guid
        }
}

let createAppApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue appAPIv1
    |> Remoting.buildHttpHandler

let createPredictionApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue predictionAPIv1
    |> Remoting.buildHttpHandler

let createLSApi : HttpHandler = 
    Remoting.createApi()
    |> Remoting.withDiagnosticsLogger (printfn "%s")
    |> Remoting.withErrorHandler errorHandler
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromContext lsAPIv1
    |> Remoting.withBinarySerialization
    |> Remoting.buildHttpHandler

let webApp =
    choose [
        createAppApi
        createLSApi
        createPredictionApi
    ]

let configureApp (app : IApplicationBuilder) =
    app.UseGiraffe webApp
    app

let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore
    services

let webhost (config: IWebHostBuilder) : IWebHostBuilder =
    config.UseKestrel(fun options ->
        options.Limits.MaxRequestBodySize <- System.Nullable()
        ()
    )

let app = application {
    use_router webApp
    memory_cache
    app_config configureApp
    service_config configureServices
    use_static "public"
    use_gzip
    webhost_config webhost
}

[<EntryPoint>]
let main _ =
    run app
    0