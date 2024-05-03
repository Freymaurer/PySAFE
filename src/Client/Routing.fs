module Routing

type Pages =
/// Predictor
| Main
| DataAccess
| About
| PrivacyPolicy
| Contact
| NotFound

    member this.ToRoute() = 
        match this with
        | Main -> [||]
        | DataAccess -> [| "data" |]
        | About -> [| "about" |]
        | PrivacyPolicy -> [| "privacy" |]
        | Contact -> [|"contact"|]
        | NotFound -> [|"404"|]

    static member fromRoute (route: string list) =
        match route with
        | [ "about" ] -> About
        | [ "data" ] -> DataAccess
        | [ "privacy" ] -> PrivacyPolicy
        | [ "contact" ] -> Contact
        | [ "predictor" ] | ["main"] | [] | ["/"] -> Main
        | _ -> NotFound

    member this.ToStringRdb() =
        match this with
        | Main -> "Predictor"
        | DataAccess -> "Data Access"
        | About -> "About"
        | PrivacyPolicy -> "Privacy Policy"
        | Contact -> "Contact"
        | NotFound -> "404"

