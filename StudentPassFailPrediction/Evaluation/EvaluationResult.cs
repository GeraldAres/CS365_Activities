namespace BinaryClassification.Evaluation
{
    public sealed class EvaluationResult
    {
        public ConfusionMatrix ConfusionMatrix { get; private set; }
        public double Accuracy { get; private set; }
        public double Precision { get; private set; }
        public double Recall { get; private set; }
        public double F1Score { get; private set; }

        public EvaluationResult(ConfusionMatrix confusionMatrix)
        {
            ConfusionMatrix = confusionMatrix;
            Accuracy = SafeDivide(confusionMatrix.TruePositive + confusionMatrix.TrueNegative, confusionMatrix.Total);
            Precision = SafeDivide(confusionMatrix.TruePositive, confusionMatrix.TruePositive + confusionMatrix.FalsePositive);
            Recall = SafeDivide(confusionMatrix.TruePositive, confusionMatrix.TruePositive + confusionMatrix.FalseNegative);
            F1Score = SafeDivide(2.0 * Precision * Recall, Precision + Recall);
        }

        private static double SafeDivide(double numerator, double denominator)
        {
            return denominator == 0.0 ? 0.0 : numerator / denominator;
        }
    }
}
