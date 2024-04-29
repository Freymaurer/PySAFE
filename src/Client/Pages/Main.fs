namespace Pages

open Feliz
open Feliz.DaisyUI
open Fable.Core

open Pages.Predictor

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
                                    Pages.Predictor.DataInput.Main(setStep)
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

