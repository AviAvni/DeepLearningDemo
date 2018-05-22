module NeuralNetwork

open CNTK.FSharp

open CNTK
open CNTK.FSharp.Sequential
open System.IO
open System.ComponentModel
open System

let trainingFilePath = "training/training_data_1.txt"

let cntkModel = "training/mario_kart_modelv1"

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

let input = CNTKLib.InputVariable(shape [imageDims], DataType.Float)
let labels = CNTKLib.InputVariable(shape [numClasses], DataType.Float)

let network : Computation = 
    Recurrent.LSTMSequenceClassifierNet 7400 numClasses 2000 2000

let spec = {
    Features = input
    Labels = labels
    Model = network
    Loss = SquaredError
    Eval = ClassificationError
    }

let config = {
    MinibatchSize = 64
    Epochs = 50
    Device = DeviceDescriptor.CPUDevice
    Schedule = { Rate = 0.00001; MinibatchSize = 1; Type = SGDLearner }
    }


let train (f:Action<Minibatch.TrainingSummary>) =
    let trainer = Learner()
    trainer.MinibatchProgress.Add(fun x -> f.Invoke(x))

    let predictor = trainer.learn minibatchSource (featureStreamName,labelsStreamName) config spec
    let modelFile = Path.Combine(__SOURCE_DIRECTORY__,"LSTM.model")

    predictor.Save(modelFile)