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