module State

open Shared

type Model = {
    ServerConnected: bool
    Todos: Todo list;
    Input: string
    CurrentTimerValue: string
} with
    static member init() =
        {
            ServerConnected = false
            Todos = []
            Input = ""
            CurrentTimerValue = ""
        }

type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo
    /// These are incoming websocket messages
    | Remote of ServerToClient.Msg
    | SetCurrentTime of string