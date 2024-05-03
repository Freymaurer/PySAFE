namespace Components

open Feliz
open Feliz.DaisyUI
open Feliz.Router

type Navbar =

    static member private NavbarLink (page: Routing.Pages) =
        let segs = page.ToRoute() |> List.ofArray
        let isCurrentPage = Router.currentUrl() = segs
        Html.li [
            prop.children [
                Html.a [
                    prop.className [
                        if isCurrentPage then
                            "active";
                            "!text-white"
                        "text-black"
                        "rounded-none"
                    ]
                    prop.href (page.ToRoute() |> Router.format)
                    prop.text (page.ToStringRdb())
                ]
            ]
        ]

    [<ReactComponent>]
    static member Main() =
        let csb_logo = StaticFile.import("../public/logo_small_dark.png")

        let items = [
            Routing.Pages.Main
            Routing.Pages.DataAccess
            Routing.Pages.About
            Routing.Pages.PrivacyPolicy
            Routing.Pages.Contact
        ]

        Daisy.navbar [
            prop.className "bg-white sticky z-50 top-0"
            prop.children [
                Daisy.navbarStart [
                    Daisy.dropdown [
                        Html.div [
                            prop.tabIndex 0
                            prop.className "btn btn-ghost lg:hidden"
                            prop.role "button"
                            prop.children [
                                Svg.svg [
                                    svg.xmlns "http://www.w3.org/2000/svg"
                                    svg.className "h-5 w-5"
                                    svg.fill "none"
                                    svg.viewBox(0,0,24,24)
                                    svg.stroke "currentColor"
                                    svg.children [
                                        Svg.path [
                                            svg.strokeLineCap "round"
                                            svg.strokeLineJoin "round"
                                            svg.strokeWidth 2
                                            svg.d "M4 6h16M4 12h8m-8 6h16"
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Daisy.dropdownContent [
                            prop.className "menu menu-sm mt-3 z-[1] p-2 shadow rounded-box w-52 bg-white"
                            prop.tabIndex 0
                            prop.children [
                                for item in items do Navbar.NavbarLink item
                            ]
                        ]
                    ]
                    Daisy.button.a [
                        button.ghost
                        prop.children [
                            Html.img [prop.src csb_logo]
                        ]
                    ]
                ]

                Daisy.navbarCenter [
                    prop.className "hidden lg:flex"
                    prop.children [
                        Daisy.menu [
                            prop.className "px-1 gap-2"
                            menu.horizontal
                            items
                            |> List.map (Navbar.NavbarLink)
                            |> prop.children
                        ]
                    ]
                ]

                Daisy.navbarEnd [
                    Daisy.button.button [
                        Html.i [prop.classes [fa.faBrands; fa.faGithub; fa.faXl]]
                    ]
                ]
            ]
        ]
        //    <div class="dropdown">
        //      <div tabindex="0" role="button" class="btn btn-ghost lg:hidden">
        //      </div>
        //      <ul tabindex="0" class="menu menu-sm dropdown-content mt-3 z-[1] p-2 shadow bg-base-100 rounded-box w-52">
        //        <li><a>Item 1</a></li>
        //        <li>
        //          <a>Parent</a>
        //          <ul class="p-2">
        //            <li><a>Submenu 1</a></li>
        //            <li><a>Submenu 2</a></li>
        //          </ul>
        //        </li>
        //        <li><a>Item 3</a></li>
        //      </ul>
        //    </div>
        //    <a class="btn btn-ghost text-xl">daisyUI</a>
        //  </div>
        //  <div class="navbar-center hidden lg:flex">
        //    <ul class="menu menu-horizontal px-1">
        //      <li><a>Item 1</a></li>
        //      <li>
        //        <details>
        //          <summary>Parent</summary>
        //          <ul class="p-2">
        //            <li><a>Submenu 1</a></li>
        //            <li><a>Submenu 2</a></li>
        //          </ul>
        //        </details>
        //      </li>
        //      <li><a>Item 3</a></li>
        //    </ul>
        //  </div>
        //  <div class="navbar-end">
        //    <a class="btn">Button</a>
        //  </div>
        //</div>

