namespace App

open Elmish
open Fable.Remoting.Client
open Shared
open Feliz
open Feliz.DaisyUI


//let private fastapi (model: Model) dispatch =
//    Html.div [
//        prop.className "bg-white/80 rounded-md shadow-md p-4 w-5/6 lg:w-3/4 lg:max-w-2xl mb-2"
//        prop.children [
//            Html.button [
//                prop.className
//                    "flex-no-shrink p-2 px-12 rounded bg-teal-600 outline-none focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed"
//                prop.onClick (fun _ -> dispatch GetFastAPIMessage)
//                prop.text "Get fast api message"
//            ]
//            Html.span model.FastAPIMessage
//        ]
//    ]


//[<ReactComponent>]
//let private FastapiBridge (model: Model) dispatch =
//    let ws = React.useMemo (fun () -> Bridge.FastAPI.create(EndPoints.fastApiBrideEndpoint))
//    ws.onmessage <- (fun event ->
//        let content = {message = string event.data}
//        GetFastAPIMessageResponse content |> dispatch
//    )
//    let start() = ws.send (box "start")
//    let stop() = ws.send (box "stop")
//    Html.div [
//        prop.className "bg-white/80 rounded-md shadow-md p-4 w-5/6 lg:w-3/4 lg:max-w-2xl mb-2"
//        prop.children [
//            Html.span (ws.readyState.ToString())
//            Html.button [
//                prop.className
//                    "flex-no-shrink p-2 px-12 rounded bg-teal-600 outline-none focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed"
//                prop.onClick (fun _ -> start() )
//                prop.text "Start fastapi timer"
//            ]
//            Html.button [
//                prop.className
//                    "flex-no-shrink p-2 px-12 rounded bg-teal-600 outline-none focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed"
//                prop.onClick (fun _ -> stop() )
//                prop.text "Stop fastapi timer"
//            ]
//            Html.span model.FastAPIMessage
//        ]
//    ]

open Feliz.Router
open Routing

type View =

    [<ReactComponent>]
    static member Main model dispatch =
        let page, updatePage = React.useState(Router.currentUrl() |> Pages.fromRoute)
        let vanta, setVanta = React.useState(None)
        let ref = React.useElementRef()
        React.useEffect((fun _ ->
            if vanta.IsNone then
                setVanta(
                    Components.Vanta.net {|
                        el = ref.current
                        mouseControls= true
                        touchControls= true
                        gyroControls= false
                        minHeight= 200.00
                        minWidth= 200.00
                        scale= 1.00
                        scaleMobile= 1.00
                        color = 0xe1e51e
                        backgroundColor = 0xa021a
                    |}
                    |> Some
                )
            vanta
            ), [|box vanta|]
        )
        React.router [
            router.onUrlChanged (Pages.fromRoute >> updatePage)
            router.children [
                Html.section [
                    prop.className "h-screen w-screen flex flex-col min-h-screen min-w-screen overflow-x-hidden"
                    prop.ref ref
                    prop.children [
                        Components.HeadBanner.Main()
                        Components.Navbar.Main()
                        Html.div [
                            prop.className "flex flex-col flex-grow"
                            prop.children [
                                //Daisy.join [
                                //    Daisy.button.button [
                                //        prop.onClick(fun e ->
                                //            Api.appApi.Log ()
                                //            |> Async.StartImmediate
                                //        )
                                //        prop.text "log"
                                //    ]
                                //    Daisy.button.button [
                                //        prop.onClick(fun e ->
                                //            async {
                                //                let data = DataInput.init [|for i in 0 .. 77 do yield {Number = i}|]
                                //                let! guid = Api.predictionApi.StartEvaluation data
                                //                log(guid)
                                //            }
                                //            |> Async.StartImmediate
                                //        )
                                //        prop.text "Test"
                                //    ]
                                //]
                                match page with
                                | Main -> Pages.Main.Main()
                                | DataAccess -> Pages.DataAccess.Main()
                                | About -> Html.div "About"
                                | Contact -> Html.div "Contact"
                                | PrivacyPolicy -> Html.div "Privacy Policy"
                                | NotFound -> Html.h1 "404 - Not Found"
                            ]
                        ]
                    ]
                ]
            ]
        ]