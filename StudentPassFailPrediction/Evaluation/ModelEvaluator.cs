using System;
using Network = BinaryClassification.NeuralNetwork.NeuralNetwork;

namespace BinaryClassification.Evaluation
{
    public static class ModelEvaluator
    {
        public static EvaluationResult Evaluate(Network model, double[][] testData)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (testData == null || testData.Length == 0)
                throw new ArgumentException("Test data cannot be empty.", "testData");

            ConfusionMatrix matrix = new ConfusionMatrix();
            int featureCount = testData[0].Length - 1;
            double[] features = new double[featureCount];
            foreach (double[] row in testData)
            {
                Array.Copy(row, features, featureCount);
                int actual = row[featureCount] >= 0.5 ? 1 : 0;
                int predicted = model.ComputeOutputs(features)[0] >= 0.5 ? 1 : 0;
                matrix.Add(actual, predicted);
            }
            return new EvaluationResult(matrix);
        }
    }
}
