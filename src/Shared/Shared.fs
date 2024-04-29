namespace Shared

open System

type Todo = { Id: Guid; Description: string }

type Data = obj

module Todo =
    let isValid (description: string) =
        String.IsNullOrWhiteSpace description |> not

    let create (description: string) = {
        Id = Guid.NewGuid()
        Description = description
    }


module EndPoints =
    [<Literal>]
    let endpoint = "/bridge"

    [<Literal>]
    let siteDomain = "localhost:5000"

    [<Literal>]
    let siteUrl = "http://" + siteDomain

    [<Literal>]
    let clientEndpoint = "ws://" + siteDomain + endpoint

    [<Literal>]
    let fastApiBrideEndpoint = "ws://localhost:8000/ws"
    let fastApiBrideEndpointURI = Uri(fastApiBrideEndpoint)

module Route =
    let clientBuilder typeName methodName =
        sprintf "%s/api/%s/%s" EndPoints.siteUrl typeName methodName

    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type HelloWorld = {
    message: string
}

type IAppApiv1 = {
    GetVersion: unit -> Async<string>
    Log: unit -> Async<unit>
}

type IPredictionApiv1 = {
    StartEvaluation: Data -> Async<Guid>
}