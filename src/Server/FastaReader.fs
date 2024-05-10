module FastaReader

open System
open System.IO
open Shared

// if you run this locally change this value to allow parsing more values at once
let maxCount = 1000

let private read (reader:TextReader) =
    let mutable noNameIterator = 0
    let mutable iterator = 0
    let rec readRecords (acc: DataInputItem list) (currentHeader: string) (currentSeq: string) =
        let nextLine = reader.ReadLine()
        match nextLine with
        | null | _ when (iterator >= maxCount) || (isNull nextLine) -> List.rev ({ Header = currentHeader; Sequence = currentSeq } :: acc)
        | line when line.StartsWith(">!") ->
            iterator <- iterator + 1
            noNameIterator <- noNameIterator + 1
            let newHeader = sprintf "Unknown%i" noNameIterator
            let line' = line.Substring(2).Trim()
            if currentHeader <> "" && currentSeq <> "" then
                readRecords ({ Header = currentHeader; Sequence = currentSeq } :: acc) newHeader line'
            else
                readRecords acc newHeader line'
        | line when line.StartsWith(">") ->
            let newHeader = line.Substring(1)
            iterator <- iterator + 1
            if currentHeader <> "" && currentSeq <> "" then
                readRecords ({ Header = currentHeader; Sequence = currentSeq } :: acc) newHeader ""
            else
                readRecords acc newHeader ""
        | line ->
            let line' = line.Trim()
            readRecords acc currentHeader (currentSeq + line')
    let res = readRecords [] "" ""
    reader.Dispose()
    res

let private readOfBytes (data:byte []) =
    use ms = new System.IO.MemoryStream(data)
    use reader = new System.IO.StreamReader(ms)
    let res = read reader
    ms.Dispose()
    res

let private readOfString (str:string) =
    use reader = new System.IO.StringReader(str)
    let res = read reader
    res

let mainReadOfString(dataRaw: string) : Async<Result<DataInputItem [], exn>>=
    async {
        let data : DataInputItem [] = readOfString dataRaw |> Array.ofList
        return Ok data
    }

let mainReadOfBytes(dataRaw: byte []) : Async<Result<DataInputItem [], exn>>=
    async {
        let data : DataInputItem [] = readOfBytes dataRaw |> Array.ofList
        return Ok data
    }