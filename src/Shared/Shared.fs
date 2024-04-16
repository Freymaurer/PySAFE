namespace Shared

open System

type Todo = { Id: Guid; Description: string }

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

module Route =
    let clientBuilder typeName methodName =
        sprintf "%s/api/%s/%s" EndPoints.siteUrl typeName methodName

    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName 

module ServerToClient =

    type Msg =
    | ServerConnected
    | TellTime of string
    | Error of exn


module ClientToServer =

    open Elmish.Bridge.RPC

    type Msg =
    //| AskTime of unit * IReplyChannel<string>
    | StartTimer
    | StopTimer

type HelloWorld = {
    message: string
}

type ITodosApi = {
    getTodos: unit -> Async<Todo list>
    addTodo: Todo -> Async<Todo>
    /// This is a hello world example for Client -> Server -> fastapi -> Server -> Client
    getHelloWorld: unit -> Async<HelloWorld>
}