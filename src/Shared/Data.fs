namespace Shared

open System

type DataInputItem = {
    Header: string
    Sequence: string
}

type DataInputConfig = {
    CutOff: float
} with
    static member init(?cutoff) = {
        CutOff = defaultArg cutoff 0.05
    }

type DataInput = {
    Items: DataInputItem []
    Config: DataInputConfig
} with
    static member init(data: DataInputItem [], ?config) = {
        Items = data
        Config = defaultArg config (DataInputConfig.init())
    }
    static member empty() = {
        Items = [||]
        Config = DataInputConfig.init()
    }

type LargeDataInput = {
    Data: byte []
    Config: DataInputConfig
} with
    static member init(data: byte [], ?config) = {
        Data = data
        Config = defaultArg config (DataInputConfig.init())
    }

[<RequireQualifiedAccess>]
type DataResponseStatus =
| Starting
/// Only used for large files.
| Validating
| MLRunning of int
| AnalysisRunning
| Finished
| Error of string
with
    member this.IsExitCode = match this with | Finished | Error _ -> true | _ -> false

type DataResponseItem = {
    Header: string
    Predictions: float list
} with
    static member init(header, ?prediction) = { Header = header; Predictions = defaultArg prediction [] }

type PredictionTarget =
  | Chloroplast
  | Mitochondria
  | Secretory
  | Cytoplasmic

type ResultData =
    {
        Header     : string
        Chloropred : float
        Qchloro    : float
        Mitopred   : float
        Qmito      : float
        Secrpred   : float
        Qsecr      : float
        FinalPred  : PredictionTarget []
    }

type DataResponse = {
    Id: Guid
    Status: DataResponseStatus
    InitData: DataInput
    PredictionData: DataResponseItem list
    ResultData: ResultData list
} with
    member this.ItemCount = this.InitData.Items.Length
    member this.AllItemsProcessed = this.PredictionData.Length = this.InitData.Items.Length && not this.PredictionData.IsEmpty
    static member init(guid, data) = {
        Id = guid
        Status = DataResponseStatus.Starting
        InitData = data
        PredictionData = []
        ResultData = []
    }
    member this.IsExited = this.Status.IsExitCode

type DataResponseDTO = {
    Data: ResultData list
}