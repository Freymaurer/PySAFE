namespace Pages.Predictor

open Fable.Core
open Feliz
open Feliz.DaisyUI

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

type DataInput =
    [<ReactComponent>]
    static member Main(setStep: Steps -> unit)  =
        let (data: DataSrc option), setData = React.useState(None)
        let activeTab, setActiveTab = React.useState(Tabs.Text)
        let dataNotEmpty = data.IsSome
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
                    Daisy.textarea [
                        textarea.bordered
                        // prop.valueOrDefault data
                        if dataNotEmpty then
                            prop.valueOrDefault data.Value.Value
                        prop.placeholder ".. add fasta information"
                        prop.className "w-full h-full flex-grow"
                        prop.onChange(fun (s: string) ->
                            DataSrc.Text s
                            |> Some
                            |> setData
                        )
                    ]
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
                        let maxN = 5000
                        let getNChars (data: string) = System.Math.Min(data.Length-1, maxN)
                        let n = getNChars(data.Value.Value)
                        Daisy.textarea [
                            prop.readOnly true
                            prop.className "w-full h-full flex-grow select-none"
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
                            prop.onClick(fun _ -> setStep Steps.Settings)
                            prop.text "Continue"
                        ]
                    ]
                ]
            ]
        ]