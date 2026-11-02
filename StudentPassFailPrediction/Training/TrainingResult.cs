using System.Collections.Generic;
using Network = BinaryClassification.NeuralNetwork.NeuralNetwork;

namespace BinaryClassification.Training
{
    public sealed class TrainingResult
    {
        public Network Model { get; private set; }
        public TrainingConfiguration Configuration { get; private set; }
        public List<double> Losses { get; private set; }

        public TrainingResult(Network model, TrainingConfiguration configuration, List<double> losses)
        {
            Model = model;
            Configuration = configuration;
            Losses = losses;
        }

        public double FinalLoss
        {
            get { return Losses.Count == 0 ? 0.0 : Losses[Losses.Count - 1]; }
        }
    }
}
