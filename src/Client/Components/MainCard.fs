namespace Components


open Fable.Core
open Feliz
open Feliz.DaisyUI

type MainCard =
    static member Main(children: ReactElement list) =
        Html.div [
            prop.id "main-predictor"
            prop.className "flex flex-grow p-[0.5] sm:p-10 justify-center"
            prop.children [
                Daisy.card [
                    prop.className "bg-white/[.95] text-black h-fit w-[100%] md:w-[70%] lg:w-[50%] min-h-[80%] max-sm:rounded-none max-sm:min-h-[100%]"
                    card.bordered
                    prop.children [
                        Daisy.cardBody [
                            prop.className "h-full w-full flex flex-col"
                            prop.children children
                        ]
                    ]
                ]
            ]
        ]

