module Index

open Elmish
open Fable.Remoting.Client
open Shared
open State
open Elmish.Bridge
open Feliz
open Feliz.DaisyUI

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
    | Remote stcMsg ->
        Bridge.State.update stcMsg model
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
        
    

open Feliz

let private todoAction model dispatch =
    Html.div [
        prop.className "flex flex-col sm:flex-row mt-4 gap-4"
        prop.children [
            Html.input [
                prop.className
                    "shadow appearance-none border rounded w-full py-2 px-3 outline-none focus:ring-2 ring-teal-300 text-grey-darker"
                prop.value model.Input
                prop.placeholder "What needs to be done?"
                prop.autoFocus true
                prop.onChange (SetInput >> dispatch)
                prop.onKeyPress (fun ev ->
                    if ev.key = "Enter" then
                        dispatch AddTodo)
            ]
            Html.button [
                prop.className
                    "flex-no-shrink p-2 px-12 rounded bg-teal-600 outline-none focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed"
                prop.disabled (Todo.isValid model.Input |> not)
                prop.onClick (fun _ -> dispatch AddTodo)
                prop.text "Add"
            ]
        ]
    ]

let private todoList model dispatch =
    Html.div [
        prop.className "bg-white/80 rounded-md shadow-md p-4 w-5/6 lg:w-3/4 lg:max-w-2xl mb-2"
        prop.children [
            Html.ol [
                prop.className "list-decimal ml-6"
                prop.children [
                    for todo in model.Todos do
                        Html.li [ prop.className "my-1"; prop.text todo.Description ]
                ]
            ]

            todoAction model dispatch
        ]
    ]

let private websocket (model: Model) dispatch =
    Html.div [
        prop.className "bg-white/80 rounded-md shadow-md p-4 w-5/6 lg:w-3/4 lg:max-w-2xl mb-2"
        prop.children [
            Html.button [
                prop.className
                    "flex-no-shrink p-2 px-12 rounded bg-teal-600 outline-none focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed"
                prop.onClick (fun _ -> Bridge.Send ClientToServer.StartTimer)
                prop.text "Start timer"
            ]
            Html.button [
                prop.className
                    "flex-no-shrink p-2 px-12 rounded bg-teal-600 outline-none focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed"
                prop.onClick (fun _ -> Bridge.Send ClientToServer.StopTimer)
                prop.text "Stop Timer"
            ]
            Html.span model.CurrentTimerValue
        ]
    ]

let private fastapi (model: Model) dispatch =
    Html.div [
        prop.className "bg-white/80 rounded-md shadow-md p-4 w-5/6 lg:w-3/4 lg:max-w-2xl mb-2"
        prop.children [
            Html.button [
                prop.className
                    "flex-no-shrink p-2 px-12 rounded bg-teal-600 outline-none focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed"
                prop.onClick (fun _ -> dispatch GetFastAPIMessage)
                prop.text "Get fast api message"
            ]
            Html.span model.FastAPIMessage
        ]
    ]


[<ReactComponent>]
let private FastapiBridge (model: Model) dispatch =
    let ws = React.useMemo (fun () -> Bridge.FastAPI.create(EndPoints.fastApiBrideEndpoint))
    ws.onmessage <- (fun event ->
        let content = {message = string event.data}
        GetFastAPIMessageResponse content |> dispatch
    )
    let start() = ws.send (box "start")
    let stop() = ws.send (box "stop")
    Html.div [
        prop.className "bg-white/80 rounded-md shadow-md p-4 w-5/6 lg:w-3/4 lg:max-w-2xl mb-2"
        prop.children [
            Html.span (ws.readyState.ToString())
            Html.button [
                prop.className
                    "flex-no-shrink p-2 px-12 rounded bg-teal-600 outline-none focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed"
                prop.onClick (fun _ -> start() )
                prop.text "Start fastapi timer"
            ]
            Html.button [
                prop.className
                    "flex-no-shrink p-2 px-12 rounded bg-teal-600 outline-none focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed"
                prop.onClick (fun _ -> stop() )
                prop.text "Stop fastapi timer"
            ]
            Html.span model.FastAPIMessage
        ]
    ]

let view model dispatch =
    Html.section [
        prop.className "h-screen w-screen"
        prop.style [
            style.backgroundSize "cover"
            style.backgroundImageUrl "https://unsplash.it/1200/900?random"
            style.backgroundPosition "no-repeat center center fixed"
        ]

        prop.children [
            Html.a [
                prop.href "https://safe-stack.github.io/"
                prop.className "absolute block ml-12 h-12 w-12 bg-teal-300 hover:cursor-pointer hover:bg-teal-400"
                prop.children [ Html.img [ prop.src "/favicon.png"; prop.alt "Logo" ] ]
            ]

            Html.div [
                prop.className "flex flex-col items-center justify-center h-full"
                prop.children [
                    Html.h1 [
                        prop.className "text-center text-5xl font-bold text-white mb-3 rounded-md p-4"
                        prop.text "PySAFE"
                    ]
                    todoList model dispatch
                    websocket model dispatch
                    fastapi model dispatch
                    FastapiBridge model dispatch
                ]
            ]
        ]
    ]