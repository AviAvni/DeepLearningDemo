module NeuralNetwork

open CNTK.FSharp

open CNTK
open CNTK.FSharp.Sequential
open System

let trainingFilePath = "training/training_data_1.txt"

let numClasses = 5

let minibatchSize = 64uL
let maxEpochs = 500000uL

let featureStreamName = "features"
let labelsStreamName = "label"

let imageDim = [100; 74; 1]

let imageDims = imageDim.[0] * imageDim.[1] * imageDim.[2]
let streamConfig = [| new StreamConfiguration(featureStreamName, imageDims); new StreamConfiguration(labelsStreamName, numClasses) |]

let minibatchSource = MinibatchSource.TextFormatMinibatchSource(trainingFilePath, streamConfig, maxEpochs * minibatchSize,true)

let imageStreamInfo = minibatchSource.StreamInfo(featureStreamName)
let labelStreamInfo = minibatchSource.StreamInfo(labelsStreamName)

let input = CNTKLib.InputVariable(shape imageDim, DataType.Float)
let labels = CNTKLib.InputVariable(shape [numClasses], DataType.Float, false, labelsStreamName, new AxisVector([| CNTK.Axis.DefaultBatchAxis() |]))

let network : Computation = 
    Layer.scale (float32 (1./255.))
    |> Layer.add (Layer.dense 3200)
    |> Layer.add (Layer.dense 1600)
    |> Layer.add (Layer.dense 800)
    |> Layer.add (Layer.dense 400)
    |> Layer.add (Layer.dense 200)
    |> Layer.add (Recurrent.LSTMSequenceClassifierNet numClasses 200 800 800)

let spec = {
    Features = input
    Labels = labels
    Model = network
    Loss = SquaredError
    Eval = ClassificationError
    }

let config = {
    MinibatchSize = 128
    Epochs = 400
    Device = DeviceDescriptor.CPUDevice
    Schedule = { Rate = 0.00002; MinibatchSize = 1; Type = MomentumSGDLearner 256. }
    }


let train (f:Action<Minibatch.TrainingSummary>) =
    let trainer = Learner()
    trainer.MinibatchProgress.Add(fun x -> f.Invoke(x))

    let predictor = trainer.learn minibatchSource (featureStreamName,labelsStreamName) config spec

    predictor.Save("LSTM.model")