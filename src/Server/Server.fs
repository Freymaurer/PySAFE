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

/// full c# websocket example: https://medium.com/bina-nusantara-it-division/implementing-websocket-client-and-server-on-asp-net-core-6-0-c-4fbda11dbceb
/// exmaple using websocket.client: https://medium.com/nerd-for-tech/your-first-c-websocket-client-5e7acc30681d

[<LiteralAttribute>]
let AppVersion = "0.0.1"

module Storage =
    let Uri = Uri(Shared.EndPoints.fastApiBrideEndpoint)
    let guids = Dictionary<Guid,obj>()
    let generateNewGuid() =
        let existingGuids = guids.Keys
        let rec generate () =
            let nextGuid = System.Guid.NewGuid()
            if existingGuids.Contains nextGuid then generate() else nextGuid
        generate()


let appAPIv1: IAppApiv1 = {
    GetVersion = fun () -> async { return AppVersion }
    Log = fun () -> async { printfn "%A" Storage.guids; return () }
}

let predictionAPIv1: IPredictionApiv1 = {
    StartEvaluation = fun data ->
        let guid = Storage.generateNewGuid()
        async {
            PythonService.subscribeWebsocket guid data
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

let webApp =
    choose [
        createAppApi
        createPredictionApi
    ]

let configureApp (app : IApplicationBuilder) =
    // Add Giraffe to the ASP.NET Core pipeline
    app.UseGiraffe webApp
    app
    // app.UseWebSockets()

let configureServices (services : IServiceCollection) =
    // Add Giraffe dependencies
    // services.AddWebSockets(fun o -> ()) |> ignore
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