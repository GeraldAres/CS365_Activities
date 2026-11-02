using System;

namespace BinaryClassification.NeuralNetwork
{
    public class NeuralNetwork
    {
        private readonly int numInput;
        private readonly int numHidden;
        private readonly int numOutput;
        private readonly double[] inputs;
        private readonly double[][] ihWeights;
        private readonly double[] hBiases;
        private readonly double[] hOutputs;
        private readonly double[][] hoWeights;
        private readonly double[] oBiases;
        private readonly double[] outputs;
        private readonly Random rnd;

        public bool UseCrossEntropy { get; set; }
        public bool UseBinaryOutput { get; set; }

        public NeuralNetwork(int numInput, int numHidden, int numOutput)
        {
            if (numInput <= 0 || numHidden <= 0 || numOutput <= 0)
                throw new ArgumentOutOfRangeException("Network dimensions must be positive.");

            this.numInput = numInput;
            this.numHidden = numHidden;
            this.numOutput = numOutput;
            inputs = new double[numInput];
            ihWeights = MakeMatrix(numInput, numHidden, 0.0);
            hBiases = new double[numHidden];
            hOutputs = new double[numHidden];
            hoWeights = MakeMatrix(numHidden, numOutput, 0.0);
            oBiases = new double[numOutput];
            outputs = new double[numOutput];
            rnd = new Random(0);
            UseCrossEntropy = true;
            UseBinaryOutput = numOutput == 1;
            InitializeWeights();
        }

        public double[] ComputeOutputs(double[] xValues)
        {
            if (xValues == null || xValues.Length != numInput)
                throw new ArgumentException("The input vector has an incorrect length.", "xValues");

            double[] hSums = new double[numHidden];
            double[] oSums = new double[numOutput];

            Array.Copy(xValues, inputs, numInput);
            for (int j = 0; j < numHidden; ++j)
            {
                for (int i = 0; i < numInput; ++i)
                    hSums[j] += inputs[i] * ihWeights[i][j];
                hOutputs[j] = HyperTan(hSums[j] + hBiases[j]);
            }

            for (int j = 0; j < numOutput; ++j)
            {
                for (int i = 0; i < numHidden; ++i)
                    oSums[j] += hOutputs[i] * hoWeights[i][j];
                oSums[j] += oBiases[j];
            }

            double[] finalOutputs = UseBinaryOutput && numOutput == 1
                ? new[] { Sigmoid(oSums[0]) }
                : Softmax(oSums);

            Array.Copy(finalOutputs, outputs, numOutput);
            return (double[])outputs.Clone();
        }

        public double[] Train(double[][] trainData, int maxEpochs, double learnRate, double momentum, Action<int, double> epochCompleted)
        {
            ValidateTrainingArguments(trainData, maxEpochs, learnRate, momentum);

            double[][] hoGrads = MakeMatrix(numHidden, numOutput, 0.0);
            double[][] ihGrads = MakeMatrix(numInput, numHidden, 0.0);
            double[][] ihPrevWeightsDelta = MakeMatrix(numInput, numHidden, 0.0);
            double[][] hoPrevWeightsDelta = MakeMatrix(numHidden, numOutput, 0.0);
            double[] obGrads = new double[numOutput];
            double[] hbGrads = new double[numHidden];
            double[] oSignals = new double[numOutput];
            double[] hSignals = new double[numHidden];
            double[] hPrevBiasesDelta = new double[numHidden];
            double[] oPrevBiasesDelta = new double[numOutput];
            double[] xValues = new double[numInput];
            double[] tValues = new double[numOutput];
            int[] sequence = new int[trainData.Length];

            for (int i = 0; i < sequence.Length; ++i)
                sequence[i] = i;

            for (int epoch = 1; epoch <= maxEpochs; ++epoch)
            {
                Shuffle(sequence);

                for (int ii = 0; ii < trainData.Length; ++ii)
                {
                    int index = sequence[ii];
                    Array.Copy(trainData[index], xValues, numInput);
                    Array.Copy(trainData[index], numInput, tValues, 0, numOutput);
                    ComputeOutputs(xValues);

                    for (int k = 0; k < numOutput; ++k)
                    {
                        double errorSignal = tValues[k] - outputs[k];
                        if (UseBinaryOutput && numOutput == 1)
                            oSignals[k] = UseCrossEntropy ? errorSignal : errorSignal * outputs[k] * (1.0 - outputs[k]);
                        else if (UseCrossEntropy)
                            oSignals[k] = errorSignal;
                        else
                            oSignals[k] = errorSignal * outputs[k] * (1.0 - outputs[k]);
                    }

                    for (int j = 0; j < numHidden; ++j)
                        for (int k = 0; k < numOutput; ++k)
                            hoGrads[j][k] = oSignals[k] * hOutputs[j];
                    for (int k = 0; k < numOutput; ++k)
                        obGrads[k] = oSignals[k];

                    for (int j = 0; j < numHidden; ++j)
                    {
                        double derivative = (1.0 + hOutputs[j]) * (1.0 - hOutputs[j]);
                        double sum = 0.0;
                        for (int k = 0; k < numOutput; ++k)
                            sum += oSignals[k] * hoWeights[j][k];
                        hSignals[j] = derivative * sum;
                    }

                    for (int i = 0; i < numInput; ++i)
                        for (int j = 0; j < numHidden; ++j)
                            ihGrads[i][j] = hSignals[j] * inputs[i];
                    for (int j = 0; j < numHidden; ++j)
                        hbGrads[j] = hSignals[j];

                    for (int i = 0; i < numInput; ++i)
                        for (int j = 0; j < numHidden; ++j)
                        {
                            double delta = ihGrads[i][j] * learnRate;
                            ihWeights[i][j] += delta + ihPrevWeightsDelta[i][j] * momentum;
                            ihPrevWeightsDelta[i][j] = delta;
                        }
                    for (int j = 0; j < numHidden; ++j)
                    {
                        double delta = hbGrads[j] * learnRate;
                        hBiases[j] += delta + hPrevBiasesDelta[j] * momentum;
                        hPrevBiasesDelta[j] = delta;
                    }
                    for (int j = 0; j < numHidden; ++j)
                        for (int k = 0; k < numOutput; ++k)
                        {
                            double delta = hoGrads[j][k] * learnRate;
                            hoWeights[j][k] += delta + hoPrevWeightsDelta[j][k] * momentum;
                            hoPrevWeightsDelta[j][k] = delta;
                        }
                    for (int k = 0; k < numOutput; ++k)
                    {
                        double delta = obGrads[k] * learnRate;
                        oBiases[k] += delta + oPrevBiasesDelta[k] * momentum;
                        oPrevBiasesDelta[k] = delta;
                    }
                }

                epochCompleted?.Invoke(epoch, Loss(trainData));
            }

            return GetWeights();
        }

        public double[] Train(double[][] trainData, int maxEpochs, double learnRate, double momentum)
        {
            return Train(trainData, maxEpochs, learnRate, momentum, null);
        }

        public double Loss(double[][] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("The data set cannot be empty.", "data");

            return UseCrossEntropy ? CrossEntropyError(data) : Error(data);
        }

        public double Accuracy(double[][] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("The data set cannot be empty.", "data");

            int correct = 0;
            double[] xValues = new double[numInput];
            for (int i = 0; i < data.Length; ++i)
            {
                Array.Copy(data[i], xValues, numInput);
                double prediction = ComputeOutputs(xValues)[0];
                int predictedLabel = UseBinaryOutput && numOutput == 1
                    ? prediction >= 0.5 ? 1 : 0
                    : MaxIndex(ComputeOutputs(xValues));
                int actualLabel = UseBinaryOutput && numOutput == 1
                    ? data[i][numInput] >= 0.5 ? 1 : 0
                    : MaxIndex(GetTarget(data[i]));
                if (predictedLabel == actualLabel)
                    ++correct;
            }
            return (double)correct / data.Length;
        }

        public void SetWeights(double[] weights)
        {
            int expected = WeightCount();
            if (weights == null || weights.Length != expected)
                throw new ArgumentException("Bad weights array in SetWeights.", "weights");

            int k = 0;
            for (int i = 0; i < numInput; ++i)
                for (int j = 0; j < numHidden; ++j)
                    ihWeights[i][j] = weights[k++];
            for (int i = 0; i < numHidden; ++i)
                hBiases[i] = weights[k++];
            for (int i = 0; i < numHidden; ++i)
                for (int j = 0; j < numOutput; ++j)
                    hoWeights[i][j] = weights[k++];
            for (int i = 0; i < numOutput; ++i)
                oBiases[i] = weights[k++];
        }

        public double[] GetWeights()
        {
            double[] result = new double[WeightCount()];
            int k = 0;
            for (int i = 0; i < numInput; ++i)
                for (int j = 0; j < numHidden; ++j)
                    result[k++] = ihWeights[i][j];
            for (int i = 0; i < numHidden; ++i)
                result[k++] = hBiases[i];
            for (int i = 0; i < numHidden; ++i)
                for (int j = 0; j < numOutput; ++j)
                    result[k++] = hoWeights[i][j];
            for (int i = 0; i < numOutput; ++i)
                result[k++] = oBiases[i];
            return result;
        }

        private void InitializeWeights()
        {
            double[] weights = new double[WeightCount()];
            for (int i = 0; i < weights.Length; ++i)
                weights[i] = (0.001 - 0.0001) * rnd.NextDouble() + 0.0001;
            SetWeights(weights);
        }

        private int WeightCount()
        {
            return numInput * numHidden + numHidden * numOutput + numHidden + numOutput;
        }

        private static double[][] MakeMatrix(int rows, int cols, double value)
        {
            double[][] result = new double[rows][];
            for (int i = 0; i < rows; ++i)
            {
                result[i] = new double[cols];
                for (int j = 0; j < cols; ++j)
                    result[i][j] = value;
            }
            return result;
        }

        private static double HyperTan(double value)
        {
            if (value < -20.0) return -1.0;
            if (value > 20.0) return 1.0;
            return Math.Tanh(value);
        }

        private static double Sigmoid(double value)
        {
            if (value < -45.0) return 0.0;
            if (value > 45.0) return 1.0;
            return 1.0 / (1.0 + Math.Exp(-value));
        }

        private static double[] Softmax(double[] values)
        {
            double max = values[0];
            for (int i = 1; i < values.Length; ++i)
                if (values[i] > max) max = values[i];

            double sum = 0.0;
            double[] result = new double[values.Length];
            for (int i = 0; i < values.Length; ++i)
            {
                result[i] = Math.Exp(values[i] - max);
                sum += result[i];
            }
            for (int i = 0; i < values.Length; ++i)
                result[i] /= sum;
            return result;
        }

        private double Error(double[][] data)
        {
            double sumSquaredError = 0.0;
            double[] xValues = new double[numInput];
            for (int i = 0; i < data.Length; ++i)
            {
                Array.Copy(data[i], xValues, numInput);
                double[] prediction = ComputeOutputs(xValues);
                double[] target = GetTarget(data[i]);
                for (int j = 0; j < numOutput; ++j)
                {
                    double error = target[j] - prediction[j];
                    sumSquaredError += error * error;
                }
            }
            return sumSquaredError / data.Length;
        }

        private double CrossEntropyError(double[][] data)
        {
            double sum = 0.0;
            double[] xValues = new double[numInput];
            for (int i = 0; i < data.Length; ++i)
            {
                Array.Copy(data[i], xValues, numInput);
                double[] prediction = ComputeOutputs(xValues);
                double[] target = GetTarget(data[i]);
                for (int j = 0; j < numOutput; ++j)
                {
                    double output = Math.Max(1.0e-15, Math.Min(1.0 - 1.0e-15, prediction[j]));
                    sum += UseBinaryOutput && numOutput == 1
                        ? -(target[j] * Math.Log(output) + (1.0 - target[j]) * Math.Log(1.0 - output))
                        : -target[j] * Math.Log(output);
                }
            }
            return sum / data.Length;
        }

        private double[] GetTarget(double[] row)
        {
            double[] target = new double[numOutput];
            Array.Copy(row, numInput, target, 0, numOutput);
            return target;
        }

        private void Shuffle(int[] sequence)
        {
            for (int i = 0; i < sequence.Length; ++i)
            {
                int r = rnd.Next(i, sequence.Length);
                int temporary = sequence[r];
                sequence[r] = sequence[i];
                sequence[i] = temporary;
            }
        }

        private void ValidateTrainingArguments(double[][] data, int epochs, double rate, double momentum)
        {
            if (data == null || data.Length == 0) throw new ArgumentException("The training data cannot be empty.", "trainData");
            if (epochs <= 0) throw new ArgumentOutOfRangeException("maxEpochs");
            if (rate <= 0.0) throw new ArgumentOutOfRangeException("learnRate");
            if (momentum < 0.0 || momentum >= 1.0) throw new ArgumentOutOfRangeException("momentum");
            for (int i = 0; i < data.Length; ++i)
                if (data[i] == null || data[i].Length != numInput + numOutput)
                    throw new ArgumentException("Each training row must contain inputs followed by targets.", "trainData");
        }

        private static int MaxIndex(double[] values)
        {
            int index = 0;
            for (int i = 1; i < values.Length; ++i)
                if (values[i] > values[index]) index = i;
            return index;
        }
    }
}
