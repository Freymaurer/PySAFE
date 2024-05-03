module Shared.ValidateData

open System

let Main(dataRaw: string) : Async<Result<DataInputItem [], exn>>=
    async {
        do! Async.Sleep(500)
        let data : DataInputItem [] = [|for c in dataRaw do yield { Number = int c }|]
        return Ok data
    }