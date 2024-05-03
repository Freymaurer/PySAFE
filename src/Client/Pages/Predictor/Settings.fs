namespace Pages.Predictor

open Fable.Core
open Feliz
open Feliz.DaisyUI
open Components
open Shared

type Settings =
    [<ReactComponent>]
    static member Main(data: Shared.DataInput, setData: Shared.DataInput -> unit, setStep: Steps -> unit) =
        let radioConfig1Id = "radio_config1"
        Html.div [
            prop.className "flex flex-col flex-grow gap-3"
            prop.children [
                Html.div [
                    prop.className "flex justify-between"
                    prop.children [
                        Daisy.cardTitle "Settings"
                    ]
                ]
                Html.div [
                    prop.className "prose text-justify"
                    prop.children [
                        Daisy.formControl [
                            Daisy.label [
                                prop.className "gap-8"
                                prop.children [
                                    Daisy.labelText [
                                        Html.h4 "Config 1"
                                        Daisy.help [
                                            Html.span """This configuration setting toggles between two states: "active" and "not active." When active, the feature is enabled, providing its intended functionality. Conversely, when not active, the feature is disabled, withholding its functionality. Adjust this setting based on your needs to control the feature's behavior within the system."""
                                        ]
                                    ]
                                    Daisy.toggle [
                                        let config = data.Config.SomeConfig
                                        prop.id radioConfig1Id
                                        if config then toggle.primary
                                        prop.onChange(fun (b:bool) -> setData {data with DataInput.Config.SomeConfig = b} )
                                        prop.defaultChecked config
                                    ]
                                ]
                            ]
                            
                        ]
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