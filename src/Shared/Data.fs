namespace Shared

open System

type DataInputItem = {
    Number: int
}

type DataInputConfig = {
    SomeConfig: bool
} with
    static member init() = {
        SomeConfig = true
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

type DataResponseStatus =
| Starting
| MLRunning of int
| AnalysisRunning
| Finished
| Error of string

type DataResponseItem = {
    Number: int
} with
    static member init(number) = { Number = number }

type DataResponse = {
    Id: Guid
    Status: DataResponseStatus
    InitData: DataInput
    ResultData: DataResponseItem list
} with
    member this.ItemCount = this.InitData.Items.Length
    member this.AllItemsProcessed = this.ResultData.Length = this.InitData.Items.Length
    static member init(guid, data) = {
        Id = guid
        Status = Starting
        InitData = data
        ResultData = []
    }