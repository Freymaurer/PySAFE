namespace Pages.Predictor

type Steps =
    | Canceled = 0
    | DataInput = 1
    | Settings = 2
    | Running = 3

module Steps =
    let toStringRdbl(this: Steps) =
        match this with
        | Steps.DataInput -> "Add Data"
        | Steps.Settings  -> "Settings"
        | Steps.Running   -> "Run"
        | _ -> ""