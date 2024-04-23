namespace Pages

open Feliz
open Feliz.DaisyUI
open Fable.Core

type private Steps =
    | Canceled = 0
    | DataInput = 1
    | Settings = 2
    | Running = 3

[<RequireQualifiedAccess>]
type private Tabs =
    | Text
    | File

type private DataSrc =
    | File of string
    | Text of string
    member this.Value =
        match this with
        | File s -> s
        | Text s -> s

module private Steps =
    let toStringRdbl(this: Steps) =
        match this with
        | Steps.DataInput -> "Add Data"
        | Steps.Settings  -> "Settings"
        | Steps.Running   -> "Run"
        | _ -> ""

type Main =
    static member private Step(step0: Steps, currentStep: Steps, setStep) =
        Daisy.step [
            prop.classes ["select-none"]
            if step0 <= currentStep then
                step.primary;
                prop.className "cursor-pointer transition-transform active:scale-90 z-[2]"
                prop.onClick (fun _ ->
                    setStep step0
                )
            prop.text (Steps.toStringRdbl step0)
        ]

    static member private Steps(step, setStep) =
        Html.div [
            prop.className "text-black mt-auto"
            prop.children [
                Daisy.divider []
                Daisy.steps [
                    prop.className "w-full"
                    prop.children [
                        Main.Step(Steps.DataInput, step, setStep)
                        Main.Step(Steps.Settings, step, setStep)
                        Main.Step(Steps.Running, step, setStep)
                    ]
                ]
            ]
        ]

    [<ReactComponent>]
    static member private DataInput(data: DataSrc option, setData) =
        let activeTab, setActiveTab = React.useState(Tabs.Text)
        let dataNotEmpty = data.IsSome
        let maxN = 5000
        let getNChars (data: string) = System.Math.Min(data.Length-1, maxN)
        let tab (tab: Tabs, currentTab) =
            Daisy.tab [
                if tab = currentTab then
                    prop.className "tab-active"
                prop.onClick (fun _ -> setActiveTab tab)
                prop.text (string tab)
            ]
        Html.div [
            prop.className "flex flex-col flex-grow gap-3"
            prop.children [
                Html.div [
                    prop.className "flex justify-between"
                    prop.children [
                        Daisy.cardTitle "Data Input"
                        Daisy.tabs [
                            tabs.bordered
                            prop.children [
                                tab (Tabs.Text, activeTab)
                                tab (Tabs.File, activeTab)
                            ]
                        ]
                    ]
                ]
                match activeTab with
                | Tabs.Text ->
                    Daisy.textarea [
                        textarea.bordered
                        // prop.valueOrDefault data
                        if dataNotEmpty then
                            let n = getNChars data.Value.Value
                            let v = data.Value.Value.Substring(0, n)
                            prop.defaultValue v
                        prop.placeholder ".. add fasta information"
                        prop.className "w-full h-full flex-grow"
                        prop.onChange(fun (s: string) ->
                            DataSrc.Text s
                            |> Some
                            |> setData
                        )
                    ]
                    match data with
                    | Some (Text s) ->
                        let n = getNChars(s)
                        let chunked = n = maxN
                        if chunked then
                            Daisy.alert [
                                alert.error
                                prop.children [
                                    Html.i [
                                        prop.className [
                                            fa.faSolid; fa.faCircleXmark
                                        ]
                                    ]
                                    Html.span $"Only {maxN} chars are rendered. For larger inputs please use the file upload."
                                ]
                            ]
                    | _ -> Html.none
                | Tabs.File ->
                    Daisy.file [
                        file.ghost
                        file.bordered
                        prop.className "lg:file-input-lg cursor-pointer"
                        prop.onChange (fun (file: Browser.Types.File) ->
                            file.text().``then``(fun r ->
                                DataSrc.File r
                                |> Some
                                |> setData
                            )
                            |> Async.AwaitPromise
                            |> Async.StartImmediate
                        )
                    ]
                    if dataNotEmpty then
                        let n = getNChars(data.Value.Value)
                        Daisy.textarea [
                            prop.readOnly true
                            prop.className "w-full h-full flex-grow"
                            prop.value (data.Value.Value.Substring(0,n))
                        ]
                Daisy.cardActions [
                    prop.className "mt-auto"
                    prop.children [
                        Daisy.button.button [
                            button.warning
                            prop.onClick(fun _ -> setData None)
                            prop.text "Reset"
                        ]

                        Daisy.button.button [
                            prop.className "ml-auto"
                            button.success
                            if not dataNotEmpty then
                                button.disabled
                            // else
                            prop.text "Continue"
                        ]
                    ]
                ]
            ]
        ]

    static member Settings() =
        Html.div [
            Daisy.cardTitle "Lorem Ipsum"

        ]

    static member Running() =
        Html.div [
            Daisy.cardTitle "Lorem Ipsum"

        ]

    [<ReactComponent>]
    static member Main() =
        let step, setStep = React.useState(Steps.DataInput)
        let (data: DataSrc option), setData = React.useState(None)
        Html.div [
            prop.id "main-predictor"
            prop.className "flex flex-grow p-2 sm:p-10 justify-center"
            prop.children [
                Daisy.card [
                    prop.className "bg-white/[.95] text-black h-fit min-w-[350px] md:min-w-[70%] min-h-[80%]"
                    card.bordered
                    prop.children [
                        Daisy.cardBody [
                            prop.className "h-full w-full flex flex-col"
                            prop.children [
                                match step with
                                | Steps.DataInput ->
                                    Main.DataInput(data, setData)
                                | Steps.Settings ->
                                    Main.Settings()
                                | Steps.Running ->
                                    Main.Running()
                                | _ -> Html.div "not implemted"
                                Main.Steps(step, setStep)
                            ]
                        ]
                    ]
                ]
            ]
        ]

