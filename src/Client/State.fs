module State

open Shared

type Model = {
    Todos: Todo list;
    Input: string
    // websocket
    ServerConnected: bool
    CurrentTimerValue: string
    // fastapi
    FastAPIMessage: string
} with
    static member init() =
        {
            Todos = []
            Input = ""
            ServerConnected = false
            CurrentTimerValue = ""
            FastAPIMessage = ""
        }

type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo
    /// These are incoming websocket messages
    | SetCurrentTime of string
    | GetFastAPIMessage
    | GetFastAPIMessageResponse of HelloWorld

open Elmish
open Fable.Remoting.Client

let todosApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>

let init () =
    let model = Model.init()
    let cmd = Cmd.OfAsync.perform todosApi.getTodos () GotTodos
    model, cmd

let update msg model =
    match msg with
    | GotTodos todos -> { model with Todos = todos }, Cmd.none
    | SetInput value -> { model with Input = value }, Cmd.none
    | AddTodo ->
        let todo = Todo.create model.Input

        let cmd = Cmd.OfAsync.perform todosApi.addTodo todo AddedTodo

        { model with Input = "" }, cmd
    | AddedTodo todo ->
        {
            model with
                Todos = model.Todos @ [ todo ]
        },
        Cmd.none
    | SetCurrentTime v -> {model with CurrentTimerValue = v}, Cmd.none
    | GetFastAPIMessage ->
        let cmd =
            Cmd.OfAsync.perform
                todosApi.getHelloWorld
                ()
                GetFastAPIMessageResponse
        model, cmd
    | GetFastAPIMessageResponse hw ->
        let nextModel = {model with FastAPIMessage = hw.message}
        nextModel, Cmd.none