module Analysis

open Shared

let runAnalysis (data: Shared.DataResponse) =
    async {
        System.Threading.Thread.Sleep(30000)
        let nextData = {
            data with Status = DataResponseStatus.Finished
        }
        return nextData
    }