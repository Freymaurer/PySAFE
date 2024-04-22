module Routing

type Pages =
| Main
| About
| PrivacyPolicy
| Contact
| NotFound

    member this.ToRoute() = 
        match this with
        | Main -> [||]
        | About -> [| "about" |]
        | PrivacyPolicy -> [| "privacy" |]
        | Contact -> [|"contact"|]
        | NotFound -> [|"404"|]

    static member fromRoute (route: string list) =
        match route with
        | [ "about" ] -> About
        | [ "privacy" ] -> PrivacyPolicy
        | [ "contact" ] -> Contact
        | [ "predictor" ] | ["main"] | [] | ["/"] -> Main
        | _ -> NotFound

    member this.ToStringRdb() =
        match this with
        | Main -> "Predictor"
        | About -> "About"
        | PrivacyPolicy -> "Privacy Policy"
        | Contact -> "Contact"
        | NotFound -> "404"

