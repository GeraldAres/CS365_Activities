# BackPropCalculator — Student Pass or Fail Prediction

## Project overview

BackPropCalculator is a Windows Forms application for predicting whether a student is likely to pass or fail. It applies the custom 3-layer Multi-Layer Perceptron (MLP) from the previous Back Propagation Activity instead of using an external machine-learning framework.

The project targets .NET Framework 4.8 and is compatible with Visual Studio Community.

## Neural-network architecture

```text
Input layer (5 features) -> Hidden layer (configurable tanh neurons) -> Output layer (1 sigmoid neuron)
```

The existing network implementation was retained in `NeuralNetwork/NeuralNetwork.cs`. Its online backpropagation and momentum weight-update mechanism are reused. The original implementation used softmax for multi-class output. Because a one-neuron softmax is always 1, the smallest mathematically necessary extension is a sigmoid output mode for binary classification.

The binary output uses:

- Hidden activation: hyperbolic tangent
- Output activation: sigmoid
- Loss: binary cross-entropy
- Optimizer: sample-by-sample backpropagation with momentum
- Prediction threshold: output >= 0.5 means PASS; otherwise FAIL

## Dataset

Source: [UCI Student Performance Dataset](https://archive.ics.uci.edu/dataset/320/student+performance)

The included file is `Data/student-mat.csv`. It contains 395 mathematics-course records from two Portuguese schools. The complete UCI collection contains 649 records across mathematics and Portuguese-language files.

The final grade `G3` is the target and is not supplied to the model as an input:

- `G3 >= 10`: PASS (`1`)
- `G3 < 10`: FAIL (`0`)

The selected input features are:

| Feature | Meaning |
|---|---|
| `G1` | First-period grade |
| `G2` | Second-period grade |
| `studytime` | Weekly study-time category from 1 to 4 |
| `failures` | Number/category of previous class failures |
| `absences` | Number of school absences |

`G1` and `G2` are earlier-period grades, not the final `G3` outcome. Their strong relationship with `G3` should be acknowledged when interpreting results.

## Preprocessing

1. The semicolon-separated CSV is parsed by `DataLoader`.
2. Numeric values are parsed using invariant culture.
3. Invalid or incomplete rows cause a clear data-validation error rather than being silently converted.
4. Records are split reproducibly with seed `365`.
5. The split is stratified by PASS/FAIL label and uses approximately 80% training and 20% testing records.
6. Min-Max minimum and maximum values are fitted only from the training records.
7. Those training parameters are applied unchanged to the test records and prediction inputs.

## Training

The training panel accepts:

- Learning rate
- Hidden-neuron count
- Epoch count

The default configuration is:

```text
Learning rate: 0.05
Hidden neurons: 10
Epochs: 1000
Momentum: 0.01
```

The training process runs asynchronously so that the Windows Forms interface remains responsive. Actual loss values are recorded after every epoch and written to:

```text
TrainingLogs/training-loss.csv
```

The **Run Experiments** button trains three actual configurations and writes their final loss and held-out test accuracy to:

```text
TrainingLogs/experiments.csv
```

No training loss or evaluation result is fabricated by the application.

## Evaluation

Evaluation is performed only on the held-out testing set. The application displays:

- Accuracy
- Precision
- Recall
- F1-score
- True positives
- True negatives
- False positives
- False negatives
- Training-loss chart

Division-by-zero cases return zero rather than producing an invalid value.

In this project:

- True positive: a student who passes and is predicted PASS
- True negative: a student who fails and is predicted FAIL
- False positive: a student who fails but is predicted PASS
- False negative: a student who passes but is predicted FAIL

## Building and running

1. Open `Robot/BinaryClassification/BinaryClassification.slnx` in Visual Studio Community.
2. Select `Debug` and `Any CPU`.
3. Rebuild the solution.
4. Start the `BinaryClassification` project.
5. Wait for the dataset count to appear.
6. Set the learning rate, hidden-neuron count, and epoch count.
7. Click **Train Model**.
8. Review the actual loss chart and held-out evaluation results.
9. Enter `G1`, `G2`, `studytime`, `failures`, and `absences` values.
10. Click **Predict**.

Prediction is disabled until training completes successfully. The application displays only PASS or FAIL; it does not display an arbitrary confidence percentage.

## Project organization

```text
BinaryClassification/
├── Data/
│   ├── DataLoader.cs
│   ├── DataNormalizer.cs
│   ├── DataSplitter.cs
│   └── student-mat.csv
├── Evaluation/
│   ├── ConfusionMatrix.cs
│   ├── EvaluationResult.cs
│   └── ModelEvaluator.cs
├── NeuralNetwork/
│   └── NeuralNetwork.cs
├── Training/
│   ├── ModelTrainer.cs
│   ├── TrainingConfiguration.cs
│   └── TrainingResult.cs
├── Form1.cs
├── Form1.Designer.cs
├── Program.cs
└── README.md
```

## Technologies and restrictions

- C#
- Windows Forms
- .NET Framework 4.8
- Standard .NET Framework libraries
- Custom MLP and backpropagation implementation

The project does not use ML.NET, TensorFlow, Accord.NET, Python, or another external machine-learning framework.
