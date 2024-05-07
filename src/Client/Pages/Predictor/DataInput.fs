namespace Pages.Predictor

open Fable.Core
open Feliz
open Feliz.DaisyUI

[<RequireQualifiedAccess>]
type private Tabs =
    | Text
    | File

type private ValidationStatus =
    | Idle
    | Validating
    | Invalid of exn
    | Valid

type DataInput =

    static member private TextArea(data0: DataSrc option, ?disabled:bool, ?onchange: string -> unit, ?restrictSize: int, ?keyboardconfirm: string -> unit) =
        let disabled = defaultArg disabled false
        let getNChars maxN (data: string) =
            let n = System.Math.Min(data.Length-1, maxN)
            data.Substring(0,n)
        let data = if restrictSize.IsSome then data0 |> Option.map (fun data -> getNChars restrictSize.Value data.Value) else data0 |> Option.map _.Value
        Html.div [
            prop.className "w-full h-full flex-grow flex flex-col relative"
            prop.children [
                Daisy.textarea [
                    textarea.bordered
                    if data.IsSome then
                        prop.valueOrDefault data.Value
                    else
                        prop.valueOrDefault ""
                    prop.disabled disabled
                    prop.placeholder ".. add fasta information"
                    prop.className "flex-grow"
                    if onchange.IsSome then
                        prop.onChange onchange.Value
                    if keyboardconfirm.IsSome then
                        // show keyboard shortcut to confirm
                        prop.onKeyDown(fun e ->
                            if (e.ctrlKey || e.metaKey) && e.which = 13. then
                                keyboardconfirm.Value data.Value
                        )
                ]
                if keyboardconfirm.IsSome then
                    let isMac = navigator.userAgent.Contains("Mac");
                    let key = if isMac then "⌘" else "Ctrl"
                    Html.span [
                        prop.className "absolute bottom-0 right-0 text-xs text-gray-500 px-2 py-1"
                        prop.children [
                            Daisy.kbd [ kbd.xs; prop.text key ]
                            Html.span " + "
                            Daisy.kbd [ kbd.xs; prop.text "Enter" ]
                            Html.span " to confirm"
                        ]
                    ]
            ]
        ]

    [<ReactComponent>]
    static member Main(data: Shared.DataInput, setData: Shared.DataInput -> unit, setStep: Steps -> unit, datasrc: DataSrc option, setDatasrc)  =
        
        let activeTab, setActiveTab = React.useState(Tabs.Text)
        let validating, setValidating = React.useState(ValidationStatus.Idle)
        let dataNotEmpty = datasrc.IsSome
        let confirm (value: string) =
            async {
                // start validation
                setValidating ValidationStatus.Validating
                let! result = Shared.ValidateData.Main(value)
                match result with
                | Ok validatedData ->
                    setValidating ValidationStatus.Valid
                    do! Async.Sleep(500)
                    let nextData = { data with Items = validatedData }
                    setData nextData
                    setStep Steps.Settings
                | Error exn ->
                    setValidating (ValidationStatus.Invalid exn)
                                    
            }
            |> Async.StartImmediate
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
                        if not dataNotEmpty then
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
                    let onchange = fun (s: string) ->
                        DataSrc.Text s
                        |> Some
                        |> setDatasrc
                    DataInput.TextArea(datasrc, onchange=onchange, keyboardconfirm=confirm)
                | Tabs.File ->
                    if dataNotEmpty then // only show result when file upload complete. Use reset to upload new file
                        DataInput.TextArea(datasrc, disabled=true, restrictSize=5000)
                    else // if no data, show file input
                        Daisy.file [
                            file.ghost
                            file.bordered
                            prop.className "lg:file-input-lg cursor-pointer"
                            prop.onChange (fun (file: Browser.Types.File) ->
                                file.text().``then``(fun r ->
                                    DataSrc.File r
                                    |> Some
                                    |> setDatasrc
                                )
                                |> Async.AwaitPromise
                                |> Async.StartImmediate
                            )
                        ]
                Daisy.cardActions [
                    prop.className "mt-auto"
                    prop.children [
                        Daisy.button.button [
                            button.warning
                            prop.onClick(fun _ -> setDatasrc None)
                            prop.text "Reset"
                        ]

                        Daisy.button.button [
                            prop.className "ml-auto"
                            button.success
                            if not dataNotEmpty then
                                button.disabled
                            prop.onClick(fun _ ->
                                confirm(datasrc.Value.Value)
                            )
                            prop.children [
                                match validating with
                                | Idle -> Html.span "Continue"
                                | Validating -> Daisy.loading [loading.bars; color.textAccent]
                                | Invalid exn -> Html.span "Error"
                                | Valid -> Html.i [
                                    prop.className [fa.faSolid; fa.faCheck; fa.faLg]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]