namespace Pages.Predictor

open Fable.Core
open Feliz
open Feliz.DaisyUI
open Fetch

[<RequireQualifiedAccess>]
type private Status =
    | Idle
    | Loading
    | Started

[<RequireQualifiedAccess>]
type EmailState =
    | Idle
    | Loading
    | Success

type private State = {
    Status: Status
    Identifier: System.Guid option
    Email: string
} with
    static member init() = { Status = Status.Idle; Identifier = None; Email = "" }

module private Helper =

    let copyToClipboard (text: string) =
        async {
            do! navigator.clipboard.writeText text |> Async.AwaitPromise
        }
        |> Async.StartImmediate

    let uploadLargeDataFile (guid: System.Guid, blob: Browser.Types.Blob) =
        let url = Shared.Route.builder "ILargeFileApiv1" "UploadLargeFile"
        fetch url [
            requestHeaders [
                HttpRequestHeaders.ContentType "application/octet-stream"
                HttpRequestHeaders.Custom("X-GUID", guid.ToString())
            ]
            RequestProperties.Method HttpMethod.POST
            RequestProperties.Body (BodyInit.Case1 blob)
        ]
        |> Async.AwaitPromise

type Runner =

    static member private EmailForm(state: State, setState, submitEmail, emailStatus:EmailState) =
        Daisy.formControl [ // Email
            prop.className "flex-grow gap-3"
            prop.children [
                Daisy.alert [
                    alert.info
                    prop.children [
                        Html.i [prop.className [fa.faSolid; fa.faInfoCircle; fa.fa2Xl]]
                        Html.p [
                            prop.text "You can opt-in to receive an email when the prediction is ready. You will receive a confirmation email verifying your request."
                        ]
                    ]
                ]
                Daisy.join [
                    Daisy.input [
                        prop.className "w-full join-item"
                        prop.label "Email"
                        prop.placeholder "email@provider.com"
                        prop.valueOrDefault state.Email
                        prop.onChange (fun e -> setState { state with Email = e })
                        prop.onKeyDown(key.enter, fun _ -> submitEmail state.Email)
                        if emailStatus = EmailState.Success then
                            prop.disabled true
                    ]
                    Daisy.button.button [
                        prop.className "join-item"
                        if state.Email = "" then
                            button.disabled
                        else
                            button.primary
                        if emailStatus = EmailState.Success then
                            button.disabled
                            button.primary
                        prop.onClick (fun _ -> submitEmail state.Email)
                        prop.children [
                            match emailStatus with
                            | EmailState.Loading ->
                                Daisy.loading [loading.bars; color.textAccent]
                            | EmailState.Idle ->
                                Html.span "Submit"
                            | EmailState.Success ->
                                Html.i [prop.className [fa.faRegular; fa.faCheckCircle; fa.fa2Xl]]
                        ]
                    ]
                ]
            ]
        ]

    static member private IdleContent(startRun: unit -> unit) =
        Html.div [
            prop.className "flex flex-grow flex-col xl:flex-row gap-6"
            prop.children [
                Html.div [
                    prop.className "prose"
                    prop.children [
                        Html.p "Perfect! 🎉 We are read to start the prediction."
                        Html.p "After starting the process you will receive an ID, which is necessary to access the results."
                        Html.p "Calculation can take some time, you can come back at any time to check the status using your ID."
                    ]
                ]
                Html.div [
                    prop.className "flex justify-center"
                    prop.children [
                        Daisy.button.button [
                            prop.className "flex-col w-[300px] h-[100px] gap-6 xl:h-full"
                            button.primary
                            button.lg
                            prop.onClick(fun _ -> startRun())
                            prop.children [
                                Html.div [ prop.className "text-[30px]"; prop.text "🚀"]
                                Html.div "Start"
                            ]
                        ]
                    ]
                ]
            ]
        ]

    static member private LoadingContent =
        Html.div [
            prop.className "flex flex-grow justify-center items-center flex-col"
            prop.children [
                Daisy.loading [
                    prop.className "w-[4rem]"
                    loading.lg
                    loading.ring
                    color.textPrimary
                ]
                Html.p [
                    prop.style [style.display.contents]
                    prop.text "Starting the prediction..."
                ]
            ]
        ]

    [<ReactComponent>]
    static member private StartedContent(state: State, setState: State -> unit, copied, setCopied) =
        let id = state.Identifier |> Option.map _.ToString() |> Option.defaultValue "<placeholder>"
        let copy (id: string) =
            Helper.copyToClipboard id
            setCopied true
        Html.div [
            prop.className "flex flex-col gap-3"
            prop.children [
                Daisy.alert [
                    alert.warning
                    prop.children [
                        Html.i [prop.className [fa.faSolid; fa.faInfoCircle; fa.fa2Xl]]
                        Html.p [
                            prop.text "Remember to copy the id below. Without it you will not be able to access your analyzed data!"
                        ]
                    ]
                ]
                Daisy.formControl [ // ID Copy
                    prop.className "relative mb-4"
                    prop.children [
                        Daisy.join [
                            Daisy.input [
                                input.lg
                                prop.className "w-full"
                                prop.label "ID"
                                prop.value id
                                prop.readOnly true
                                join.item
                            ]
                            Daisy.button.button [
                                button.lg
                                prop.className "relative z-[2]"
                                join.item
                                button.primary
                                prop.onClick (fun _ -> copy id)
                                prop.children [
                                    if not copied then 
                                        Html.span [ //ping
                                            prop.className "absolute flex h-3 w-3 top-0 right-0 -mt-1 -mr-1"
                                            prop.children [
                                                Html.span [
                                                    prop.className "animate-ping absolute inline-flex h-full w-full rounded-full bg-primary opacity-75"
                                                ]
                                                Html.span [
                                                    prop.className "relative inline-flex rounded-full h-3 w-3 bg-secondary"
                                                ]
                                            ]
                                        ]
                                    Html.i [prop.className [fa.faRegular; fa.faClipboard]]
                                ]
                            ]
                        ]   
                        Daisy.label [
                            prop.style [
                                if copied then
                                    style.transform(transform.translateY (length.perc 100))
                            ]
                            prop.className "bottom-0 right-0 absolute transition-transform"
                            prop.children [
                                Daisy.labelTextAlt [
                                    prop.className "!text-success"
                                    prop.text "Copied!"
                                ]
                            ]
                        ]
                    ]
                ]              
            ]
        ]

    [<ReactComponent>]
    static member Main(datasrc: DataSrc option, data: Shared.DataInputItem [], config: Shared.DataInputConfig, reset: unit -> unit, setIsRunning: bool -> unit)  =
        let (state: State), setState = React.useState(State.init)
        let emailStatus, setEmailStatus= React.useState(EmailState.Idle)
        let copied, setCopied = React.useState(false)
        let startRun = fun () ->
            async {
                setIsRunning true
                setState { state with Status = Status.Loading }
                let! guid =
                    match datasrc with
                    | Some (DataSrc.LargeFile bytes) ->
                        async {
                            let! guid = Api.predictionApi.PutConfig(config)
                            let! response = Helper.uploadLargeDataFile(guid, bytes)
                            //Api.lsApi.UploadFile (bytes)
                            return guid
                        }
                    | Some _ ->
                        let dto = Shared.DataInput.init(data, config)
                        Api.predictionApi.StartEvaluation dto
                    | None -> failwith "No data source selected"
                setState { state with Status = Status.Started; Identifier = Some guid }
            }
            |> Async.StartImmediate
        let submitEmail (email: string) =
            async {
                setEmailStatus EmailState.Loading
                do! Api.predictionApi.PutEmail(state.Identifier.Value, state.Email)
                setEmailStatus EmailState.Success
            }
            |> Async.StartImmediate
        Html.div [
            prop.className "flex flex-col flex-grow gap-3 justify-between"
            prop.children [
                Html.div [
                    prop.className "flex justify-between"
                    prop.children [
                        Daisy.cardTitle "Run"
                    ]
                ]
                match state.Status with
                | Status.Idle -> Runner.IdleContent startRun
                | Status.Loading -> Runner.LoadingContent
                | Status.Started -> Runner.StartedContent(state, setState, copied, setCopied)
                Daisy.cardActions [
                    prop.className "flex justify-between items-end"
                    if state.Status = Status.Started then
                        let exitButton =
                            Daisy.button.button [
                                if not copied then
                                    button.disabled
                                prop.className "ml-auto"
                                button.info
                                prop.onClick(fun _ ->
                                    reset()
                                )
                                prop.text "Exit"
                            ]  
                        prop.children [
                            Runner.EmailForm(state,setState,submitEmail,emailStatus)
                            if not copied then
                                Daisy.tooltip [
                                    prop.custom("data-tip", "Copy ID first!")
                                    prop.className "ml-auto"
                                    prop.children [
                                        exitButton     
                                    ]
                                ]
                            else
                                exitButton
                        ]
                ]
            ]
        ]