module Analysis

open Shared

let runAnalysis (data: Shared.DataResponse) =
    async {
        let nextData = {
            data with Status = DataResponseStatus.Finished
        }
        return nextData
    }