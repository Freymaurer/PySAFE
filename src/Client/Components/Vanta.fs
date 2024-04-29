module Components.Vanta

open Fable.Core
open Fable.Core.JsInterop
open System

let defaultConfig  (elementId: string) =
    {|
        el = "#" + elementId
        mouseControls = true
        touchControls = true
        gyroControls = false
        minHeight = 200.00
        minWidth = 200.00
        scale = 1.00
        scaleMobile = 1.00
    |}

[<AllowNullLiteral>]
type IVanta =
    member this.destroy() = nativeOnly

    interface IDisposable with
        member this.Dispose() =
            this.destroy()

let net (config: obj) : IVanta = importDefault "vanta/dist/vanta.net.min"

