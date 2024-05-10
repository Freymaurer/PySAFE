namespace Pages

open Feliz
open Feliz.DaisyUI
open Fable.Core
open Fable.Core.JsInterop
open Shared
open Feliz.Router

open Pages.Predictor

type Main =
    [<ReactComponent>]
    static member private Step(step0: Steps, currentStep: Steps, setStep) =
        let active, setActive = React.useState(false)
        let ref = React.useElementRef()
        Daisy.step [
            prop.ref ref
            prop.classes ["select-none"]
            if step0 <= currentStep then
                step.primary;
                prop.className "cursor-pointer transition-transform active:scale-90"
                prop.onMouseDown(fun _ ->
                    ref.current.Value?style?zIndex <- "2"
                    setActive true
                )
                prop.onMouseUp(fun _ -> setActive false)
                prop.onClick (fun _ ->
                    setStep step0
                )
                prop.onTransitionEnd(fun e ->
                    if not active then
                        ref.current.Value?style?zIndex <- null
                )
            prop.text (Steps.toStringRdbl step0)
        ]

    static member private Steps(step, setStep) =
        Html.div [
            prop.className "text-black mt-auto"
            prop.children [
                Daisy.divider []
                Html.div [
                    prop.className "flex justify-between items-center gap-3 max-sm:flex-col-reverse"
                    prop.children [
                        Daisy.tooltip [
                            prop.custom("data-tip", "Access Commissioned Data")
                            prop.className "max-sm:w-full"
                            prop.children [
                                Daisy.button.a [
                                    prop.className "max-sm:w-[80%]"
                                    button.neutral
                                    prop.text "Data"
                                    prop.href (Routing.DataAccess.ToRoute() |> Router.format)
                                ]
                            ]
                        ]
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
            ]
        ]

    [<ReactComponent>]
    static member Main() =
        let (data: DataInputItem []), setData = React.useState([||])
        let (config: Shared.DataInputConfig), setConfig = React.useState(Shared.DataInputConfig.init)
        let (datasrc: DataSrc option), setDatasrc = React.useState(None)
        let step, setStep0 = React.useState(Steps.DataInput)
        let isRunning, setIsRunning = React.useState(false)
        let reset() =
            setData [||]
            setStep0 Steps.DataInput
            setConfig (Shared.DataInputConfig.init())
            setDatasrc None
            setIsRunning false
        let setStep (step: Steps) =
            if step = Steps.Canceled then
                reset()
            else setStep0 step
        Components.MainCard.Main [
            match step with
            | Steps.DataInput ->
                Pages.Predictor.DataInput.Main(data, setData, setStep, datasrc, setDatasrc)
            | Steps.Settings ->
                Pages.Predictor.Settings.Main(config, setConfig, setStep)
            | Steps.Running ->
                Pages.Predictor.Runner.Main(datasrc, data, config, reset, setIsRunning)
            | _ -> Html.div "not implemted"
            if not isRunning then // only show go back steps while not running
                Main.Steps(step, setStep)
        ]

