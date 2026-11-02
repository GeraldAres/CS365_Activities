using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BinaryClassification.Data;
using BinaryClassification.Evaluation;
using BinaryClassification.Training;
using Network = BinaryClassification.NeuralNetwork.NeuralNetwork;

namespace BinaryClassification
{
    public partial class Form1 : Form
    {
        private List<StudentRecord> records;
        private DataSplit split;
        private DataNormalizer normalizer;
        private double[][] trainingData;
        private double[][] testingData;
        private Network model;
        private CancellationTokenSource cancellation;

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            trainButton.Click += trainButton_Click;
            experimentButton.Click += experimentButton_Click;
            predictButton.Click += predictButton_Click;
            clearButton.Click += clearButton_Click;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, "Data", "student-mat.csv");
                records = DataLoader.Load(path);
                split = DataSplitter.StratifiedSplit(records, 0.80, 365);
                normalizer = new DataNormalizer(DataLoader.SelectedFeatures.Length);
                normalizer.Fit(split.Training);
                trainingData = normalizer.Transform(split.Training);
                testingData = normalizer.Transform(split.Testing);
                datasetStatusLabel.Text = string.Format("Dataset: {0} records | Training: {1} | Testing: {2}", records.Count, trainingData.Length, testingData.Length);
                modelStatusLabel.Text = "Model status: not trained";
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Dataset error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                trainButton.Enabled = false;
                experimentButton.Enabled = false;
            }
        }

        private async void trainButton_Click(object sender, EventArgs e)
        {
            TrainingConfiguration configuration;
            if (!TryReadConfiguration(out configuration)) return;
            await TrainModelAsync(configuration);
        }

        private async Task TrainModelAsync(TrainingConfiguration configuration)
        {
            SetTrainingState(true);
            cancellation = new CancellationTokenSource();
            try
            {
                IProgress<TrainingProgress> progress = new Progress<TrainingProgress>(value =>
                {
                    trainingProgressBar.Value = Math.Min(100, (int)(100.0 * value.Epoch / value.TotalEpochs));
                    lossLabel.Text = string.Format("Epoch {0}/{1} | Loss: {2:F6}", value.Epoch, value.TotalEpochs, value.Loss);
                });
                TrainingResult result = await Task.Run(() => ModelTrainer.Train(trainingData, DataLoader.SelectedFeatures.Length, configuration, progress, cancellation.Token), cancellation.Token);
                model = result.Model;
                ModelTrainer.WriteLossLog(Path.Combine(Application.StartupPath, "TrainingLogs", "training-loss.csv"), result);
                EvaluationResult evaluation = ModelEvaluator.Evaluate(model, testingData);
                ShowEvaluation(evaluation, result);
                modelStatusLabel.Text = "Model status: trained and ready for prediction";
                predictionLabel.Text = "Prediction: enter values and click Predict";
            }
            catch (OperationCanceledException)
            {
                modelStatusLabel.Text = "Model status: training cancelled";
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Training error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cancellation.Dispose();
                cancellation = null;
                SetTrainingState(false);
            }
        }

        private async void experimentButton_Click(object sender, EventArgs e)
        {
            SetTrainingState(true);
            cancellation = new CancellationTokenSource();
            try
            {
                TrainingConfiguration[] configurations =
                {
                    new TrainingConfiguration(0.03, 5, 500, 0.01),
                    new TrainingConfiguration(0.05, 10, 1000, 0.01),
                    new TrainingConfiguration(0.10, 15, 1500, 0.01)
                };
                List<ExperimentResult> results = new List<ExperimentResult>();
                foreach (TrainingConfiguration configuration in configurations)
                {
                    TrainingResult result = await Task.Run(() => ModelTrainer.Train(trainingData, DataLoader.SelectedFeatures.Length, configuration, null, cancellation.Token), cancellation.Token);
                    EvaluationResult evaluation = ModelEvaluator.Evaluate(result.Model, testingData);
                    results.Add(new ExperimentResult(configuration, result.FinalLoss, evaluation.Accuracy));
                }
                string path = Path.Combine(Application.StartupPath, "TrainingLogs", "experiments.csv");
                ModelTrainer.WriteExperimentSummary(path, results);
                MessageBox.Show(this, "Three actual configurations were trained. Results were saved to TrainingLogs\\experiments.csv.", "Experiments complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Experiment error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cancellation.Dispose();
                cancellation = null;
                SetTrainingState(false);
            }
        }

        private void ShowEvaluation(EvaluationResult evaluation, TrainingResult result)
        {
            accuracyLabel.Text = string.Format("Accuracy: {0:P2}", evaluation.Accuracy);
            precisionLabel.Text = string.Format("Precision: {0:P2}", evaluation.Precision);
            recallLabel.Text = string.Format("Recall: {0:P2}", evaluation.Recall);
            f1Label.Text = string.Format("F1-score: {0:P2}", evaluation.F1Score);
            ConfusionMatrix matrix = evaluation.ConfusionMatrix;
            confusionLabel.Text = string.Format("TP: {0}   TN: {1}   FP: {2}   FN: {3}", matrix.TruePositive, matrix.TrueNegative, matrix.FalsePositive, matrix.FalseNegative);
            lossChart.Series[0].Points.Clear();
            for (int i = 0; i < result.Losses.Count; ++i)
                lossChart.Series[0].Points.AddXY(i + 1, result.Losses[i]);
        }

        private void predictButton_Click(object sender, EventArgs e)
        {
            if (model == null)
            {
                MessageBox.Show(this, "Train the model before making a prediction.", "Model not ready", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            double[] values;
            if (!TryReadPrediction(out values)) return;
            double output = model.ComputeOutputs(normalizer.Transform(values))[0];
            predictionLabel.Text = output >= 0.5 ? "Prediction: PASS" : "Prediction: FAIL";
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            foreach (TextBox box in predictionPanel.Controls.OfType<TextBox>()) box.Clear();
            predictionLabel.Text = "Prediction: enter values and click Predict";
        }

        private bool TryReadConfiguration(out TrainingConfiguration configuration)
        {
            configuration = null;
            double learningRate;
            int hiddenNeurons;
            int epochs;
            if (!double.TryParse(learningRateTextBox.Text, out learningRate) || !int.TryParse(hiddenTextBox.Text, out hiddenNeurons) || !int.TryParse(epochsTextBox.Text, out epochs))
            {
                MessageBox.Show(this, "Learning rate, hidden neurons, and epochs must be valid numbers.", "Invalid configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            try { configuration = new TrainingConfiguration(learningRate, hiddenNeurons, epochs, 0.01); configuration.Validate(); return true; }
            catch (Exception exception) { MessageBox.Show(this, exception.Message, "Invalid configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
        }

        private bool TryReadPrediction(out double[] values)
        {
            values = new double[5];
            TextBox[] boxes = { g1TextBox, g2TextBox, studyTimeTextBox, failuresTextBox, absencesTextBox };
            for (int i = 0; i < boxes.Length; ++i)
                if (!double.TryParse(boxes[i].Text, out values[i]))
                {
                    MessageBox.Show(this, "Enter valid numeric values for all five features.", "Invalid prediction input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            if (values[0] < 0 || values[0] > 20 || values[1] < 0 || values[1] > 20 || values[2] < 1 || values[2] > 4 || values[3] < 0 || values[4] < 0)
            {
                MessageBox.Show(this, "Use G1/G2 from 0-20, study time from 1-4, and non-negative failures/absences.", "Invalid prediction input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void SetTrainingState(bool active)
        {
            trainButton.Enabled = !active;
            experimentButton.Enabled = !active;
            predictButton.Enabled = !active && model != null;
            trainingProgressBar.Value = active ? 0 : trainingProgressBar.Value;
            modelStatusLabel.Text = active ? "Model status: training..." : modelStatusLabel.Text;
        }
    }
}
