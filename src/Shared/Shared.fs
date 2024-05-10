namespace Shared

open System

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
    let fastApiBrideEndpoint = "ws://localhost:8000/dataml"
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
    StartEvaluation: DataInput -> Async<Guid>
    PutEmail: Guid * string -> Async<unit>
    GetStatus: Guid -> Async<DataResponseStatus>
    GetData: Guid -> Async<DataResponseDTO>
    PutConfig: DataInputConfig -> Async<Guid>
    ValidateData: string -> Async<Result<DataInputItem [], exn>>
}

type ILargeFileApiv1 = {
    UploadLargeFile: byte [] -> Async<unit>
}