namespace Pages.Predictor

open Fable.Core
open Feliz
open Feliz.DaisyUI
open Components
open Shared

type Settings =
    static member ConfigElement(name: string, label: string, inputElement: ReactElement, ?isError: string) =
        Daisy.formControl [
            Daisy.label [
                prop.className "gap-8"
                prop.children [
                    Daisy.labelText [
                        prop.className "prose text-justify"
                        prop.children [
                            Html.h4 name
                            Daisy.help [
                                Html.span label
                            ]
                        ]
                    ]
                    Daisy.formControl [
                        inputElement
                        if isError.IsSome then
                            Daisy.label [
                                Daisy.labelTextAlt isError.Value
                            ]
                    ]
                ]
            ]
        ]

    [<ReactComponent>]
    static member Main(config: Shared.DataInputConfig, setConfig: Shared.DataInputConfig -> unit, setStep: Steps -> unit) =
        let isWrongInput, setIsWrongInput = React.useState false
        Html.div [
            prop.className "flex flex-col flex-grow gap-3"
            prop.children [
                Html.form [
                    prop.className "flex justify-between"
                    prop.children [
                        Daisy.cardTitle "Settings"
                    ]
                ]
                Html.div [
                    prop.children [
                        Settings.ConfigElement(
                            "Cut off",
                            """The threshold q-value below which a prediction is considered statistically significant. Set to 0.05 by default, meaning that predictions with q-values below this threshold are classified as significant. This parameter helps in distinguishing between statistically significant and non-significant predictions, reducing the chance of false-positive localizations.""",
                            Daisy.input [
                                prop.type'.number
                                prop.min 0.0
                                prop.max 1.0
                                prop.step 0.01
                                prop.defaultValue config.CutOff
                                if isWrongInput then
                                    input.error
                                prop.onChange(fun (f0: float) ->
                                    if f0 > 1.0 || f0 < 0.0 then
                                        setIsWrongInput true
                                    else
                                        setIsWrongInput false
                                    let f = System.Math.Min(1.0, System.Math.Max(0.0, f0))
                                    setConfig {config with CutOff = f}
                                )
                            ],
                            ?isError=(if isWrongInput then Some "Value must be between 0 and 1" else None)
                        )
                    ]
                ]
                Daisy.cardActions [
                    prop.className "mt-auto"
                    prop.children [
                        //Daisy.button.button [
                        //    button.warning
                        //    prop.onClick(fun _ -> setData None)
                        //    prop.text "Reset"
                        //]

                        Daisy.button.button [
                            prop.className "ml-auto"
                            button.success
                            prop.onClick(fun _ ->
                               setStep Steps.Running
                            )
                            prop.text "Continue"
                        ]
                    ]
                ]
            ]
        ]