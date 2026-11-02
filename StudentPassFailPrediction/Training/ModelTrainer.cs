using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Network = BinaryClassification.NeuralNetwork.NeuralNetwork;

namespace BinaryClassification.Training
{
    public sealed class TrainingProgress
    {
        public int Epoch { get; private set; }
        public int TotalEpochs { get; private set; }
        public double Loss { get; private set; }

        public TrainingProgress(int epoch, int totalEpochs, double loss)
        {
            Epoch = epoch;
            TotalEpochs = totalEpochs;
            Loss = loss;
        }
    }

    public static class ModelTrainer
    {
        public static TrainingResult Train(
            double[][] trainingData,
            int inputCount,
            TrainingConfiguration configuration,
            IProgress<TrainingProgress> progress,
            CancellationToken cancellationToken)
        {
            if (trainingData == null || trainingData.Length == 0)
                throw new ArgumentException("Training data cannot be empty.", "trainingData");
            if (inputCount <= 0)
                throw new ArgumentOutOfRangeException("inputCount");
            configuration.Validate();

            Network model = new Network(inputCount, configuration.HiddenNeurons, 1)
            {
                UseBinaryOutput = true,
                UseCrossEntropy = true
            };
            List<double> losses = new List<double>(configuration.Epochs);
            model.Train(trainingData, configuration.Epochs, configuration.LearningRate, configuration.Momentum,
                (epoch, loss) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    losses.Add(loss);
                    progress?.Report(new TrainingProgress(epoch, configuration.Epochs, loss));
                });

            return new TrainingResult(model, configuration, losses);
        }

        public static void WriteLossLog(string path, TrainingResult result)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("A log path is required.", "path");
            string directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            using (StreamWriter writer = new StreamWriter(path, false))
            {
                writer.WriteLine("LearningRate,HiddenNeurons,Epochs,Momentum,Epoch,TrainingLoss");
                for (int i = 0; i < result.Losses.Count; ++i)
                {
                    writer.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "{0},{1},{2},{3},{4},{5:R}",
                        result.Configuration.LearningRate,
                        result.Configuration.HiddenNeurons,
                        result.Configuration.Epochs,
                        result.Configuration.Momentum,
                        i + 1,
                        result.Losses[i]));
                }
            }
        }

        public static string WriteExperimentSummary(string path, IList<ExperimentResult> experiments)
        {
            string directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                writer.WriteLine("LearningRate,HiddenNeurons,Epochs,FinalLoss,TestAccuracy");
                foreach (ExperimentResult experiment in experiments)
                {
                    writer.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "{0},{1},{2},{3:R},{4:R}",
                        experiment.Configuration.LearningRate,
                        experiment.Configuration.HiddenNeurons,
                        experiment.Configuration.Epochs,
                        experiment.FinalLoss,
                        experiment.TestAccuracy));
                }
            }
            return path;
        }
    }

    public sealed class ExperimentResult
    {
        public TrainingConfiguration Configuration { get; private set; }
        public double FinalLoss { get; private set; }
        public double TestAccuracy { get; private set; }

        public ExperimentResult(TrainingConfiguration configuration, double finalLoss, double testAccuracy)
        {
            Configuration = configuration;
            FinalLoss = finalLoss;
            TestAccuracy = testAccuracy;
        }
    }
}
