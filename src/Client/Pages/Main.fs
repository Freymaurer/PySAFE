namespace Pages

open Feliz
open Feliz.DaisyUI
open Fable.Core
open Fable.Core.JsInterop
open Shared

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
    static member Main() =
        let (data: Shared.DataInput), setData = React.useState(DataInput.empty)
        let (datasrc: DataSrc option), setDatasrc = React.useState(None)
        let step, setStep0 = React.useState(Steps.DataInput)
        let reset() =
            setData <| DataInput.empty()
            setStep0 Steps.DataInput
            setDatasrc None
        let setStep (step: Steps) =
            if step = Steps.Canceled then
                reset()
            else setStep0 step
        Html.div [
            prop.id "main-predictor"
            prop.className "flex flex-grow p-2 sm:p-10 justify-center"
            prop.children [
                Daisy.card [
                    prop.className "bg-white/[.95] text-black h-fit min-w-[350px] md:w-[70%] min-h-[80%]"
                    card.bordered
                    prop.children [
                        Daisy.cardBody [
                            prop.className "h-full w-full flex flex-col"
                            prop.children [
                                match step with
                                | Steps.DataInput ->
                                    Pages.Predictor.DataInput.Main(data, setData, setStep, datasrc, setDatasrc)
                                | Steps.Settings ->
                                    Pages.Predictor.Settings.Main(data, setData, setStep)
                                | Steps.Running ->
                                    Pages.Predictor.Runner.Main(data, setData, setStep)
                                | _ -> Html.div "not implemted"
                                Main.Steps(step, setStep)
                            ]
                        ]
                    ]
                ]
            ]
        ]

