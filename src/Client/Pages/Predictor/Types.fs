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

type DataSrc =
    | File of string
    | Text of string
    | LargeFile of Browser.Types.Blob
    member this.Value =
        match this with
        | File s -> s
        | Text s -> s
        | LargeFile _ -> "Large file detected. No Preview available."
