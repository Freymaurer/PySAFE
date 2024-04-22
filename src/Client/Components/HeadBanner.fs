namespace Components


open Feliz
open Feliz.DaisyUI

type HeadBanner =

    static member Main() =
        let rptu_logo = StaticFile.import("../public/rptu_web_logo_schwarz.svg")
        Html.nav [
            prop.className "bg-white px-6 py-3"
            prop.children [
                Html.img [prop.src rptu_logo; prop.style [style.width 192; style.height 74]]
                Daisy.button.a [
                    button.link 
                    prop.className "p-0 no-underline hover:underline"
                    prop.children [
                        Html.h2 [
                            prop.className "text-lg font-bold text-black"
                            prop.text "Department of Biology"
                        ]
                    ]
                ]
            ]
        ]