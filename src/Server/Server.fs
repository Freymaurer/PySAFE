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

[<LiteralAttribute>]
let AppVersion = "0.0.1"

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
        let guid = Environment.TestGUID//Storage.generateNewGuid()
        async {
            PythonService.subscribeWebsocket guid data
            return guid
        }
    PutEmail = fun (id, email) ->
        async {
            Storage.Storage.Update (id, fun dr -> {dr with Email = Some email})
        }
    GetStatus = fun id ->
        async {
            match Storage.Storage.TryGet id with
            | Some dr ->
                return dr.Status
            | None ->
                return DataResponseStatus.Error ("No status found")
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

let webApp =
    choose [
        createAppApi
        createPredictionApi
    ]

let configureApp (app : IApplicationBuilder) =
    app.UseGiraffe webApp
    app

let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore
    services



let app = application {
    use_router webApp
    memory_cache
    app_config configureApp
    service_config configureServices
    use_static "public"
    use_gzip
}

[<EntryPoint>]
let main _ =
    run app
    0