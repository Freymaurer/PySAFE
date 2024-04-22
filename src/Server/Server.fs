module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open Shared
open Giraffe
open Elmish.Bridge

module Storage =
    let todos = ResizeArray()

    let addTodo todo =
        if Todo.isValid todo.Description then
            todos.Add todo
            Ok()
        else
            Error "Invalid todo"

    do
        addTodo (Todo.create "Create new SAFE project") |> ignore
        addTodo (Todo.create "Write your app") |> ignore
        addTodo (Todo.create "Ship it!!!") |> ignore

let todosApi = {
    getTodos = fun () -> async { return Storage.todos |> List.ofSeq }
    addTodo =
        fun todo -> async {
            return
                match Storage.addTodo todo with
                | Ok() -> todo
                | Error e -> failwith e
        }
    getHelloWorld = fun () -> async {return! PythonService.helloWorldHandler()}
}

let createTodosApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue todosApi
    |> Remoting.buildHttpHandler

let webApp =
    choose [
        createTodosApi
    ]

let app = application {
    use_router webApp
    memory_cache
    app_config Giraffe.useWebSockets
    use_static "public"
    use_gzip
}

[<EntryPoint>]
let main _ =
    run app
    0