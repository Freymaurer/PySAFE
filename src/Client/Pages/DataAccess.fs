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
    | LoadingData
    | DataReady of DataResponseDTO
    | ErrorState of exn

    member this.Status = 
        match this with
        | Success status -> Some status
        | LoadingData -> Some DataResponseStatus.Finished
        | _ -> None

    member this.Error =
        match this with
        | ErrorState exn -> Some exn.Message
        | Success (DataResponseStatus.Error s) -> Some s
        | _ -> None

[<RequireQualifiedAccess>]
type private FlipStates =
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

module private Helper =

    open Browser.Types
    open Fable.Core
    open System

    [<Emit("new Blob([$0.buffer], { type: $1 })")>]
    let createBlobFromBytesAndMimeType (value: byte[]) (mimeType: string) : Blob = jsNative

    [<Emit("window.URL.createObjectURL($0)")>]
    let createObjectUrl (blob: Blob) : string = jsNative

    [<Emit "URL.revokeObjectURL($0)">]
    let revokeObjectUrl (dataUrl: string) : unit = jsNative

    type TextEncoder =
        abstract member encode: string -> byte []

    [<Emit("new TextEncoder()")>]
    let textEncoder () : TextEncoder = jsNative

    /// From Fable.Remoting https://github.com/Zaid-Ajaj/Fable.Remoting/blob/master/Fable.Remoting.Client/Extensions.fs
    let saveFileAs(content: byte[], fileName: string) =

        if String.IsNullOrWhiteSpace(fileName) then
            ()
        else
        let mimeType = "application/octet-stream"
        let blob = createBlobFromBytesAndMimeType content mimeType
        let dataUrl = createObjectUrl blob
        let anchor = Browser.Dom.document.createElement "a"
        anchor?style <- "display: none"
        anchor?href <- dataUrl
        anchor?download <- fileName
        anchor?rel <- "noopener"
        anchor.click()
        // clean up
        anchor.remove()
        // clean up the created object url because it is being kept in memory
        Browser.Dom.window.setTimeout(unbox(fun () -> revokeObjectUrl(dataUrl)), 40 * 1000)
        |> ignore

    let dtoToString(dto: DataResponseDTO) =
        dto.Data
        |> List.mapi (fun i x ->
            [|
                string i
                x.Header
                string x.Chloropred
                string x.Qchloro
                string x.Mitopred
                string x.Qmito
                string x.Secrpred
                string x.Qsecr
                x.FinalPred |> Array.map string |> String.concat ";"
            |]
            |> String.concat "\t"
        )
        |> String.concat "\n"

    let downloadData (dto: DataResponseDTO) =
        let content = dtoToString dto
        let fileName = sprintf "%s_data.txt" <| System.DateTime.Now.ToString("yyyyMMdd_HHmm")
        let utf8Encode = textEncoder()
        let bytes = utf8Encode.encode(content)
        saveFileAs (bytes, fileName)

type DataAccess =
    //[<ReactComponent>]
    //static member private ErrorModal(msg: string, setStatus) =
    //    let modalContainer = React.useContext(ReactContext.modalContext)
    //    Daisy.modal.dialog [
    //        prop.isOpen true
    //        prop.className "modal-bottom sm:modal-middle"
    //        prop.children [
    //            Daisy.modalBox.div [
    //                prop.className "flex flex-col px-6 py-4 justify-center items-center gap-6"
    //                prop.children [
    //                    Html.div [
    //                        prop.className "prose"
    //                        prop.children [
    //                            Html.p msg
    //                        ]
    //                    ]
    //                    Html.i [prop.className ["text-error"; "text-6xl"; fa.faSolid; fa.faExclamationTriangle; ]]
    //                ]
    //            ]
    //            Daisy.modalBackdrop [
    //                prop.className "bg-black bg-opacity-50"
    //                prop.children [
    //                    Html.button [
    //                        prop.text "Close"
    //                        prop.onClick(fun _ -> setStatus AccessStatusStatus.Idle)
    //                    ]
    //                ]
    //            ]
    //        ]
    //    ]
    //    |> fun modal -> ReactDOM.createPortal(modal,modalContainer.current.Value)

    [<ReactComponent>]
    static member private ErrorView(msg: string, setStatus) =
        Html.div [
            prop.className "flex flex-grow justify-center items-center flex-col gap-3"
            prop.children [
                Html.div [
                    prop.className "prose text-center"
                    prop.children [
                        Html.p msg
                    ]
                ]
                Html.i [prop.className ["text-error"; "text-6xl"; fa.faSolid; fa.faExclamationTriangle; ]]
            ]
        ]

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

    static member private PaginationButton(target: int, current: int, setActiveChunk: int -> unit) =
        Daisy.button.a [
            button.sm
            button.outline
            prop.className "join-item"
            if target = current then button.active
            prop.onClick(fun _ -> setActiveChunk target)
            prop.text (target+1)
        ]

    static member private PaginationButtonPlaceholder() =
        Daisy.button.a [
            button.sm
            button.outline
            prop.className "join-item"
            button.disabled
            prop.text ""
        ]
        //87cc0fed-ea60-46fc-8040-5be8efaee695
    [<ReactComponent>]
    static member private DataView(data: DataResponseDTO) =
        let activeChunk, setActiveChunk = React.useState(0)
        let chunkedData = data.Data |> Seq.chunkBySize 50
        let chunkCount = Seq.length chunkedData
        let tpHeader (name: string) (txt: string) = 
            Html.th [
                prop.className "relative"
                prop.children [
                    Html.div [
                        prop.className "flex justify-between"
                        prop.children [
                            Html.span name
                            Daisy.dropdown [
                                dropdown.hover
                                prop.children [
                                    Daisy.badge [
                                        badge.xs
                                        prop.role "button"
                                        prop.text "?"
                                    ]
                                    Daisy.dropdownContent [
                                        prop.className "z-[1] card p-2 shadow bg-primary text-primary-content absolute"
                                        prop.children [
                                            Daisy.cardBody [
                                                prop.className "p-2"
                                                prop.children [
                                                    Html.div [
                                                        prop.className "prose text-white w-64 overflow text-xs whitespace-normal"
                                                        prop.children [
                                                            Html.p txt
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        Html.div [
            prop.className "flex flex-grow flex-col gap-4 w-full"
            prop.children [
                Html.div [
                    prop.className "overflow-x-auto flex-grow max-h-[400px]"
                    prop.id "table-container"
                    prop.children [
                        Daisy.table [
                            table.pinCols
                            table.xs
                            prop.children [
                                Html.thead [
                                    Html.tr [
                                        Html.th ""
                                        Html.th "Header"
                                        tpHeader "Prediction"
                                            """Represents the model's final prediction of the\n
protein's localization based on the highest score
and its corresponding q-value. The final localization
is determined by comparing the q-values and
prediction scores against preset cutoffs. If all
q-values exceed the cutoff, the protein is classified as "Cytoplasmic." """
                                        tpHeader "Chloropred"
                                            "Prediction score indicating the likelihood of the protein being localized to the Chloroplast. A higher scores suggest a stronger prediction that the protein is localized in the Chloroplast."
                                        tpHeader "Qchloro"
                                            "q-value associated with the Chloroplast prediction score. Provides a measure of statistical significance for the Chloroplast prediction. Lower q-values indicate higher statistical significance."
                                        tpHeader "Mitopred"
                                            "Prediction score for the localization of the protein to the Mitochondria. A higher scores suggest a stronger prediction of Mitochondrial localization."
                                        tpHeader "Qmito"
                                            "q-value associated with the Mitochondria prediction score. Indicates the statistical significance of the Mitochondria localization prediction. Lower q-values suggest a more reliable prediction."
                                        tpHeader "Secrpred"
                                            "Prediction score for identifying the protein as a Secretory Protein.A higher scores indicate a stronger likelihood that the protein functions as a Secretory Protein."
                                        tpHeader "Qsecr"
                                            "q-value for the Secretory Protein prediction. Provides a measure of the statistical significance of the Secretory Protein prediction. Lower q-values are indicative of more statistically significant predictions."
                                    ]
                                ]
                                Html.tbody [
                                    let chunk = Seq.item activeChunk chunkedData
                                    for i in 0 .. (chunk.Length-1) do
                                        let ele = chunk.[i]
                                        let innerIndex = i+1
                                        let outerIndex = innerIndex * (activeChunk+1)
                                        Html.tr [
                                            Html.th (outerIndex)
                                            for field in [|
                                                ele.Header
                                                ele.FinalPred |> Array.map string |> String.concat ";"
                                                !!ele.Chloropred
                                                !!ele.Qchloro
                                                !!ele.Mitopred
                                                !!ele.Qmito
                                                !!ele.Secrpred
                                                !!ele.Qsecr
                                            |] do
                                                Html.td field
                                        ]
                                ]
                            ]
                        ]
                    ]
                ]
                Html.div [
                    prop.className "flex flex-row justify-between items-end"
                    prop.children [
                        Daisy.join [
                            // Start chunk
                            DataAccess.PaginationButton(0, activeChunk, setActiveChunk)
                            let chunkMinus3 = activeChunk-3
                            let chunkMinus2 = activeChunk-2
                            let chunkMinus1 = activeChunk-1
                            let chunkPlus1 = activeChunk+1
                            let chunkPlus2 = activeChunk+2
                            let chunkPlus3 = activeChunk+3
                            let showChunk(chunk:int) (isPlaceholder: bool) =
                                if chunk >= 1 && chunk < chunkCount-1 then
                                    if isPlaceholder then
                                        [DataAccess.PaginationButtonPlaceholder()]
                                    else
                                        [DataAccess.PaginationButton(chunk, activeChunk, setActiveChunk)]
                                else
                                    []
                            yield! showChunk chunkMinus3 true
                            yield! showChunk chunkMinus2 false
                            yield! showChunk chunkMinus1 false
                            // active chunk
                            yield! showChunk activeChunk false
                            yield! showChunk chunkPlus1 false
                            yield! showChunk chunkPlus2 false
                            yield! showChunk chunkPlus3 true
                            // End chunk, only if more than one chunk
                            if chunkCount > 1 then
                                DataAccess.PaginationButton(chunkCount-1, activeChunk, setActiveChunk)
                        ]
                        Daisy.button.button [
                            button.primary
                            prop.text "Download"
                            prop.onClick (fun _ -> Helper.downloadData data)
                        ]
                    ]
                ]
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
                    let! response = Api.predictionApi.GetStatus guid
                    match response with
                    | DataResponseStatus.Finished ->
                        setStatus (AccessStatusStatus.LoadingData)
                        let! data = Api.predictionApi.GetData guid
                        setStatus (AccessStatusStatus.DataReady data)
                    | anyElse -> 
                        setStatus (AccessStatusStatus.Success response)
            }
            |> Async.StartImmediate
        Components.MainCard.Main [
            Daisy.cardTitle "Data Access"
            Html.div [
                prop.className "flex flex-grow flex-col gap-6"
                prop.children [
                    DataAccess.AccessForm(status, id, setId, submitId)
                    Html.div [
                        prop.className "flex flex-col flex-grow gap-4 justify-center items-center"
                        prop.children [
                            match status with
                            | AccessStatusStatus.NoValidGuid ->
                                Html.none
                            | AccessStatusStatus.Success DataResponseStatus.Starting ->
                                Html.h2 "Warming up..."
                            | AccessStatusStatus.Success (DataResponseStatus.Validating) ->
                                Html.h2 "Validating data input.."
                                DataAccess.PeriodicFlip()
                            | AccessStatusStatus.Success (DataResponseStatus.MLRunning batch) ->
                                Html.h2 "The machine is thinking.."
                                Html.i [prop.className "fa-solid fa-robot text-6xl fa-bounce text-primary"; ]
                                Html.div (sprintf "Batch: %A" batch)
                            | AccessStatusStatus.Success (DataResponseStatus.AnalysisRunning) ->
                                Html.h2 "Processing results.."
                                DataAccess.PeriodicFlip()
                            | AccessStatusStatus.Success (DataResponseStatus.Finished) | AccessStatusStatus.LoadingData ->
                                Html.h2 "Results are ready!"
                                Html.div "Getting your data ready.."
                                Daisy.loading [
                                    loading.infinity
                                ]
                            | AccessStatusStatus.DataReady data ->
                                DataAccess.DataView data
                                
                            | AccessStatusStatus.Idle -> Html.none
                            | AccessStatusStatus.Success (DataResponseStatus.Error e) -> DataAccess.ErrorView(e, setStatus)
                            | AccessStatusStatus.ErrorState e -> DataAccess.ErrorView(e.Message, setStatus)
                        ]
                    ]
                ]
            ]
        ]

