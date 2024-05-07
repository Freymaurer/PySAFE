namespace Pages

open Fable.Core
open Feliz
open Feliz.DaisyUI
open Shared

[<RequireQualifiedAccess>]
type private AccessStatusStatus =
    | Idle
    | NoValidGuid
    | Success of DataResponseStatus
    | ErrorState of exn

    member this.Status = 
        match this with
        | Success status -> Some status
        | _ -> None

    member this.Error =
        match this with
        | ErrorState exn -> Some exn.Message
        | Success (DataResponseStatus.Error s) -> Some s
        | _ -> None

[<RequireQualifiedAccess>]
type FlipStates =
    | On
    | Off
    | Indeterminate
    | SomeonesGod
    member this.Next() =
        let rdnNumber = System.Random().Next(1,101)
        match rdnNumber, this with
        | 100, _ -> SomeonesGod
        | _, On -> Off
        | _, Off -> Indeterminate
        | _, Indeterminate -> On
        | _, SomeonesGod -> On

open Fable.Core.JsInterop

type DataAccess =
    [<ReactComponent>]
    static member private ErrorModal(msg: string, setStatus) =
        let modalContainer = React.useContext(ReactContext.modalContext)
        Daisy.modal.dialog [
            prop.isOpen true
            prop.className "modal-bottom sm:modal-middle"
            prop.children [
                Daisy.modalBox.div [
                    prop.className "flex flex-col px-6 py-4 justify-center items-center gap-6"
                    prop.children [
                        Html.div [
                            prop.className "prose"
                            prop.children [
                                Html.p msg
                            ]
                        ]
                        Html.i [prop.className ["text-error"; "text-6xl"; fa.faSolid; fa.faExclamationTriangle; ]]
                    ]
                ]
                Daisy.modalBackdrop [
                    prop.className "bg-black bg-opacity-50"
                    prop.children [
                        Html.button [
                            prop.text "Close"
                            prop.onClick(fun _ -> setStatus AccessStatusStatus.Idle)
                        ]
                    ]
                ]
            ]
        ]
        |> fun modal -> ReactDOM.createPortal(modal,modalContainer.current.Value)

    static member private AccessForm(status: AccessStatusStatus, id: string, setId: string -> unit, submitId: string -> unit) =
        Daisy.formControl [
            prop.className "flex flex-col"
            prop.children [
                Daisy.join [
                    Html.div [
                        prop.className "relative flex flex-grow"
                        prop.children [
                            Daisy.input [
                                prop.className "join-item flex-grow relative z-[2]"
                                prop.valueOrDefault id
                                prop.placeholder ".. Enter ID"
                                prop.onChange (fun e -> setId e)
                                prop.onKeyDown(key.enter, fun _ -> submitId id)
                            ]
                            Daisy.label [
                                prop.style [
                                    if status = AccessStatusStatus.NoValidGuid then
                                        style.transform (transform.translateY (length.perc 100))
                                ]
                                prop.className "absolute left-0 bottom-0 transition-transform"
                                prop.children [
                                    Daisy.labelText [
                                        prop.className "text-error"
                                        prop.text "Invalid ID"
                                    ]
                                ]
                            ]
                        ]
                    ]
                    Daisy.button.button [
                        prop.className "join-item"
                        button.primary
                        prop.text "Submit"
                        if id = "" then button.disabled
                        prop.onClick (fun _ -> submitId id)
                    ]
                ]
            ]
        ]

    [<ReactComponent>]
    static member PeriodicFlip() =
        let toggle, setToggle = React.useState(FlipStates.On)
        let checkbox = React.useElementRef()
        let activateToggle() =
            let next = toggle.Next()
            setToggle next
            match next with
            | FlipStates.SomeonesGod | FlipStates.Off ->
                checkbox.current.Value?indeterminate <- false
                checkbox.current.Value?``checked`` <- false
            | FlipStates.On ->
                checkbox.current.Value?indeterminate <- false
                checkbox.current.Value?``checked`` <- true
            | FlipStates.Indeterminate ->
                checkbox.current.Value?indeterminate <- true
        React.useLayoutEffect(fun () ->
            let id = Browser.Dom.window.setInterval(activateToggle, 1000)
            { new System.IDisposable with member __.Dispose() = Browser.Dom.window.clearInterval id}
        )
        //let id = Helper.recCall(f, 1000)
        Daisy.swap [
            swap.flip
            prop.className "text-9xl text-primary"
            prop.children [
                Html.input [prop.type'.checkbox; prop.ref checkbox]
                Daisy.swapOn [Html.i [prop.className [fa.faSolid; fa.faChartLine]]]
                Daisy.swapIndeterminate [Html.i [prop.className [fa.faSolid; fa.faChartBar]]]
                if toggle = FlipStates.SomeonesGod then
                    Daisy.swapOff [Html.i [prop.className [fa.faSolid; fa.faSpaghettiMonsterFlying]]]
                else
                    Daisy.swapOff [Html.i [prop.className [fa.faSolid; fa.faWaveSquare]]]
                    
            ]
        ]
        
    [<ReactComponent>]
    static member Main() =
        let id, setId0 = React.useState("")
        let status, setStatus = React.useState<AccessStatusStatus>(AccessStatusStatus.Idle)
        let setId id =
            if status = AccessStatusStatus.NoValidGuid then
                setStatus AccessStatusStatus.Idle
            setId0 id
        let submitId (id: string) =
            async {
                // check if id is a valid guid
                match System.Guid.TryParse id with
                // if not trigger a small note below input
                | false, _ -> setStatus AccessStatusStatus.NoValidGuid
                | true, guid ->
                    // if valid, get the prediction status
                    // Test value: 03a25364-45db-4add-9fd7-65c603da9c11
                    log "start get status"
                    let! response = Api.predictionApi.GetStatus guid

                    setStatus (AccessStatusStatus.Success response)
            }
            |> Async.StartImmediate
        Components.MainCard.Main [
            if status.Error.IsSome then
                DataAccess.ErrorModal(status.Error.Value, setStatus)
            Daisy.cardTitle "Data Access"
            Html.div [
                prop.className "flex flex-grow flex-col"
                prop.children [
                    DataAccess.AccessForm(status, id, setId, submitId)
                    Html.div [
                        prop.className "flex flex-col flex-grow gap-4 justify-center items-center prose"
                        prop.children [
                            match status.Status with
                            | Some DataResponseStatus.Starting ->
                                Html.h2 "Warming up..."
                            | Some (DataResponseStatus.MLRunning batch) ->
                                Html.h2 "The machine is thinking.."
                                Html.i [prop.className "fa-solid fa-robot text-6xl fa-bounce text-primary"; ]
                                Html.div (sprintf "Batch: %A" batch)
                            | Some (DataResponseStatus.AnalysisRunning) ->
                                Html.h2 "Processing results.."
                                DataAccess.PeriodicFlip()
                            | Some (DataResponseStatus.Finished) ->
                                Html.h2 "Results are ready!"
                                Html.div "Getting your data ready.."
                                Daisy.loading [
                                    loading.infinity
                                ]
                            | Some (DataResponseStatus.Error _)
                            | None -> Html.none
                        ]
                    ]
                ]
            ]
        ]

