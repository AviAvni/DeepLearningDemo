﻿module NeuralNetwork

open CNTK
open CNTK.FSharp
open CNTK.FSharp.Sequential
 
open System.IO
open System

// take a look at the data

let numClasses = 10
let input = CNTKLib.InputVariable(shape [ 28; 28; 1 ], DataType.Float)
let labels = CNTKLib.InputVariable(shape [ numClasses ], DataType.Float)

(*
    Model is now a sequence of "standard layer"
    that are composed together into a function
*)

let conv : Conv2D.Conv2D = 
    {    
        Kernel = { Width = 3; Height = 3 } 
        Filters = 1
        Initializer = Custom(CNTKLib.GlorotUniformInitializer(0.26, -1, 2))
        Strides = { Horizontal = 1; Vertical = 1 }
    }

let pool : Conv2D.Pool2D = 
    {
        PoolingType = PoolingType.Max
        Window = { Width = 3; Height = 3 }
        Strides = { Horizontal = 2; Vertical = 2 }
        Padding = true
    }

let network : Computation =
    Layer.scale (float32 (1./255.))
    |> Layer.add (Conv2D.convolution { conv with Filters = 4 })
    |> Layer.add Activation.ReLU
    |> Layer.add (Conv2D.pooling pool)
    |> Layer.add (Conv2D.convolution { conv with Filters = 8 })
    |> Layer.add Activation.ReLU
    |> Layer.add (Conv2D.pooling pool)
    |> Layer.add (Layer.dense numClasses)

let spec = {
    Features = input
    Labels = labels
    Model = network
    Loss = CrossEntropyWithSoftmax
    Eval = ClassificationError
    }

(*
    Specifying how to learn, and
    connecting a model to data,
    is simplified
*)

let ImageDataFolder = __SOURCE_DIRECTORY__
let featureStreamName = "features"
let labelsStreamName = "labels"

// specify how the learning should happen
// note: can specify what device to learn on
let config = {
    MinibatchSize = 64
    Epochs = 50
    Device = DeviceDescriptor.CPUDevice
    Schedule = { Rate = 0.003125; MinibatchSize = 1; Type = SGDLearner }
    }

let source : TextFormatSource = {
    FilePath = Path.Combine(ImageDataFolder, "training")
    Features = featureStreamName
    Labels = labelsStreamName
    }

let minibatchSource : MinibatchSource = TextFormat.source (source.Mappings spec)

let modelFile = Path.Combine(__SOURCE_DIRECTORY__,"MNISTConvolution.model")

let train (f:Action<Minibatch.TrainingSummary>) =
    let trainer = Learner()
    trainer.MinibatchProgress.Add(fun x -> f.Invoke(x))

    let predictor = trainer.learn minibatchSource (featureStreamName,labelsStreamName) config spec

    predictor.Save(modelFile)

(*
    Using the model we just trained
    We load model from disk and test it on another set of pictures
*)

let testingSource = {
    FilePath = Path.Combine(ImageDataFolder, "validation")
    Features = featureStreamName
    Labels = labelsStreamName
    }

let test () =
    let (total, errors) = Utilities.evaluate modelFile testingSource DeviceDescriptor.CPUDevice
    sprintf "Total: %d, Errors: %d" total errors

let visualize () =
    Utilities.predict modelFile testingSource DeviceDescriptor.CPUDevice
    |> Seq.map (fun (pixels,expected,predicted) -> 
        pixels,
        sprintf "Real:%i, Pred:%i" expected predicted,
        expected = predicted)
    |> Seq.toArray
    |> Seq.map (fun (pixels,label, ok) ->
        Visualizer.draw pixels label ok)

let predict pixels =
    let predicted = Utilities.predictOne modelFile pixels DeviceDescriptor.CPUDevice
    let label = sprintf "Pred:%i" predicted
    Visualizer.drawF pixels label true
