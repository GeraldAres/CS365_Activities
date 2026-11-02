namespace BinaryClassification.Training
{
    public sealed class TrainingConfiguration
    {
        public double LearningRate { get; set; }
        public int HiddenNeurons { get; set; }
        public int Epochs { get; set; }
        public double Momentum { get; set; }

        public TrainingConfiguration(double learningRate, int hiddenNeurons, int epochs, double momentum)
        {
            LearningRate = learningRate;
            HiddenNeurons = hiddenNeurons;
            Epochs = epochs;
            Momentum = momentum;
        }

        public void Validate()
        {
            if (LearningRate <= 0.0 || LearningRate > 1.0)
                throw new System.ArgumentOutOfRangeException("LearningRate", "Learning rate must be greater than 0 and no greater than 1.");
            if (HiddenNeurons < 1 || HiddenNeurons > 200)
                throw new System.ArgumentOutOfRangeException("HiddenNeurons", "Hidden neurons must be between 1 and 200.");
            if (Epochs < 1 || Epochs > 100000)
                throw new System.ArgumentOutOfRangeException("Epochs", "Epochs must be between 1 and 100000.");
            if (Momentum < 0.0 || Momentum >= 1.0)
                throw new System.ArgumentOutOfRangeException("Momentum", "Momentum must be at least 0 and less than 1.");
        }
    }
}
