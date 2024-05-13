module Analysis

open Shared
open System
open FSharp.Stats
open ProteomIQon

[<Literal>]
let private FolderPath = @"./Decoys"

type private BaseValues = 
    {
        Score:float
        Label:float
    }
    member this.IsDecoy = this.Label = 0.

// function to determine the optimal bandwith for the QValue function
let private getBandwith (baseValues: BaseValues []) = 
    baseValues
    |>Array.map (fun x -> x.Score)
    |> Distributions.Bandwidth.nrd0

let private readCsv (path: string) =
    let fullPath = System.IO.Path.GetFullPath(path)
    let lines0 = System.IO.File.ReadAllLines(fullPath)
    let lines = lines0 |> Array.tail
    let values = [|
      for line in lines do
        let parts = line.Split(',')
        let score = parts.[0].Trim() |> float
        let label = parts.[1].Trim() |> float
        yield { Score = score; Label = label }
    |] 
    values

let private createDecoyQ (baseValues: BaseValues []) =
    let bw = getBandwith baseValues
    FDRControl'.calculateQValueLogReg
      1.0
      bw
      baseValues
      (fun x -> x.IsDecoy)
      (fun x -> x.Score)
      (fun x -> x.Score)

let private chloroQCsv = readCsv (IO.Path.Combine(FolderPath, @"prediction_chloro_epoch13.csv"))
let private mitoQCsv = readCsv (IO.Path.Combine(FolderPath, @"prediction_mito_epoch6_min.csv"))
let private secrQCsv = readCsv (IO.Path.Combine(FolderPath, @"prediction_sp_epoch55.csv"))

let private createChloroQ = createDecoyQ chloroQCsv
let private createMitoQ = createDecoyQ mitoQCsv
let private createSecrQ = createDecoyQ secrQCsv

//function to add the QValues to the Input type to create the Output type
let private addQValuesAndPrediction (prediction: DataResponseItem) (createChloroQ: float->float) (createMitoQ: float->float) (createSecrQ: float->float) (cutoff)=
    let chloroQ = createChloroQ prediction.Predictions.[0]
    let mitoQ = createMitoQ prediction.Predictions.[1]
    let secrQ = createSecrQ prediction.Predictions.[2]
    let finalOutput = 
        if   chloroQ < cutoff && mitoQ > cutoff   && secrQ > cutoff then [|PredictionTarget.Chloroplast|]
        elif mitoQ < cutoff   && chloroQ > cutoff && secrQ > cutoff then [|PredictionTarget.Mitochondria|]
        elif secrQ < cutoff   && mitoQ > cutoff   && chloroQ > cutoff then [|PredictionTarget.Secretory|]
        elif chloroQ < cutoff && mitoQ < cutoff   && secrQ > cutoff then [|PredictionTarget.Chloroplast; PredictionTarget.Mitochondria|]
        elif chloroQ < cutoff && secrQ < cutoff   && mitoQ > cutoff then [|PredictionTarget.Chloroplast; PredictionTarget.Secretory|]
        elif mitoQ < cutoff   && secrQ < cutoff   && chloroQ > cutoff then [|PredictionTarget.Mitochondria; PredictionTarget.Secretory|]
        elif chloroQ < cutoff && mitoQ < cutoff   && secrQ < cutoff then [|PredictionTarget.Chloroplast; PredictionTarget.Mitochondria; PredictionTarget.Secretory|]
        else [|Cytoplasmic|]
    {
        Header = prediction.Header
        Chloropred = prediction.Predictions.[0]
        Mitopred = prediction.Predictions.[1]
        Secrpred = prediction.Predictions.[2]
        Qchloro = chloroQ
        Qmito = mitoQ
        Qsecr = secrQ
        FinalPred = finalOutput
    }

let private analyzePrediction (predictions: DataResponseItem list) (cutoff: float) =
    predictions
    |> List.map (fun x -> addQValuesAndPrediction x createChloroQ createMitoQ createSecrQ cutoff)

let runAnalysis (data: Shared.DataResponse) =
    async {
        let nextData = {
            data with
                Status = DataResponseStatus.Finished
                ResultData = analyzePrediction data.PredictionData data.InitData.Config.CutOff
        }
        return nextData
    }