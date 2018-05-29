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
//let labels = CNTKLib.InputVariable(shape [numClasses], DataType.Float)
let labels = CNTKLib.InputVariable(shape [numClasses], DataType.Float, false, labelsStreamName, new AxisVector([| CNTK.Axis.DefaultBatchAxis() |]))

//let conv : Conv2D.Conv2D = 
//    {    
//        Kernel = { Width = 10; Height = 10 } 
//        Filters = 1
//        Initializer = Custom(CNTKLib.GlorotUniformInitializer(0.26, -1, 2))
//        Strides = { Horizontal = 1; Vertical = 1 }
//    }

//let pool : Conv2D.Pool2D = 
//    {
//        PoolingType = PoolingType.Max
//        Window = { Width = 10; Height = 10 }
//        Strides = { Horizontal = 2; Vertical = 2 }
//        Padding = true
//    }

let network : Computation = 
    Layer.scale (float32 (1./255.))
    |> Layer.add (Recurrent.LSTMSequenceClassifierNet 500 3000 500 500)
    |> Layer.add (Layer.dense 200)
    |> Layer.add (Layer.dense 100)
    |> Layer.add (Layer.dense numClasses)

let spec = {
    Features = input
    Labels = labels
    Model = network
    Loss = SquaredError
    Eval = ClassificationError
    }

let config = {
    MinibatchSize = 64
    Epochs = 100
    Device = DeviceDescriptor.CPUDevice
    Schedule = { Rate = 0.000001; MinibatchSize = 1; Type = SGDLearner }
    }


let train (f:Action<Minibatch.TrainingSummary>) =
    let trainer = Learner()
    trainer.MinibatchProgress.Add(fun x -> f.Invoke(x))

    let predictor = trainer.learn minibatchSource (featureStreamName,labelsStreamName) config spec

    predictor.Save("LSTM.model")