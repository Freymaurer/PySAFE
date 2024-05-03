namespace Components

open Feliz
open Feliz.DaisyUI
open Fable.Core

[<AutoOpen>]
module Custom =

    type Daisy with
        static member help(children: ReactElement list) =
            Html.span [
                prop.className "text-base-500 text-sm"
                prop.children children
            ]
    
