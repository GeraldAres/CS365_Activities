using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace BinaryClassification
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private Label titleLabel;
        private Label descriptionLabel;
        private Label datasetStatusLabel;
        private Label modelStatusLabel;
        private GroupBox configurationGroup;
        private Label learningRateLabel;
        private Label hiddenLabel;
        private Label epochsLabel;
        private TextBox learningRateTextBox;
        private TextBox hiddenTextBox;
        private TextBox epochsTextBox;
        private Button trainButton;
        private Button experimentButton;
        private ProgressBar trainingProgressBar;
        private Label lossLabel;
        private GroupBox evaluationGroup;
        private Label accuracyLabel;
        private Label precisionLabel;
        private Label recallLabel;
        private Label f1Label;
        private Label confusionLabel;
        private Chart lossChart;
        private Panel predictionPanel;
        private Label predictionTitleLabel;
        private Label g1Label;
        private Label g2Label;
        private Label studyTimeLabel;
        private Label failuresLabel;
        private Label absencesLabel;
        private TextBox g1TextBox;
        private TextBox g2TextBox;
        private TextBox studyTimeTextBox;
        private TextBox failuresTextBox;
        private TextBox absencesTextBox;
        private Button predictButton;
        private Button clearButton;
        private Label predictionLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            titleLabel = new Label(); descriptionLabel = new Label(); datasetStatusLabel = new Label(); modelStatusLabel = new Label();
            configurationGroup = new GroupBox(); learningRateLabel = new Label(); hiddenLabel = new Label(); epochsLabel = new Label();
            learningRateTextBox = new TextBox(); hiddenTextBox = new TextBox(); epochsTextBox = new TextBox(); trainButton = new Button(); experimentButton = new Button();
            trainingProgressBar = new ProgressBar(); lossLabel = new Label(); evaluationGroup = new GroupBox(); accuracyLabel = new Label(); precisionLabel = new Label(); recallLabel = new Label(); f1Label = new Label(); confusionLabel = new Label();
            lossChart = new Chart(); predictionPanel = new Panel(); predictionTitleLabel = new Label(); g1Label = new Label(); g2Label = new Label(); studyTimeLabel = new Label(); failuresLabel = new Label(); absencesLabel = new Label();
            g1TextBox = new TextBox(); g2TextBox = new TextBox(); studyTimeTextBox = new TextBox(); failuresTextBox = new TextBox(); absencesTextBox = new TextBox(); predictButton = new Button(); clearButton = new Button(); predictionLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)(lossChart)).BeginInit();
            SuspendLayout();
            titleLabel.AutoSize = true; titleLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold); titleLabel.Location = new Point(20, 15); titleLabel.Text = "Student Pass or Fail Prediction";
            descriptionLabel.AutoSize = true; descriptionLabel.Location = new Point(23, 52); descriptionLabel.Text = "Custom 3-layer MLP using the UCI Student Performance dataset";
            datasetStatusLabel.AutoSize = true; datasetStatusLabel.Location = new Point(23, 78); datasetStatusLabel.Text = "Dataset: loading...";
            modelStatusLabel.AutoSize = true; modelStatusLabel.Location = new Point(23, 100); modelStatusLabel.Text = "Model status: loading...";
            configurationGroup.Text = "Training Configuration"; configurationGroup.Location = new Point(20, 135); configurationGroup.Size = new Size(300, 225);
            learningRateLabel.AutoSize = true; learningRateLabel.Location = new Point(15, 32); learningRateLabel.Text = "Learning rate"; learningRateTextBox.Location = new Point(145, 29); learningRateTextBox.Size = new Size(120, 23); learningRateTextBox.Text = "0.05";
            hiddenLabel.AutoSize = true; hiddenLabel.Location = new Point(15, 67); hiddenLabel.Text = "Hidden neurons"; hiddenTextBox.Location = new Point(145, 64); hiddenTextBox.Size = new Size(120, 23); hiddenTextBox.Text = "10";
            epochsLabel.AutoSize = true; epochsLabel.Location = new Point(15, 102); epochsLabel.Text = "Epochs"; epochsTextBox.Location = new Point(145, 99); epochsTextBox.Size = new Size(120, 23); epochsTextBox.Text = "1000";
            trainButton.Location = new Point(15, 140); trainButton.Size = new Size(120,  thirtyFive()); trainButton.Text = "Train Model";
            experimentButton.Location = new Point(145, 140); experimentButton.Size = new Size(120, 35); experimentButton.Text = "Run Experiments";
            trainingProgressBar.Location = new Point(15, 183); trainingProgressBar.Size = new Size(250, 23); lossLabel.AutoSize = true; lossLabel.Location = new Point(15, 207); lossLabel.Text = "Epoch: - | Loss: -";
            configurationGroup.Controls.AddRange(new Control[] { learningRateLabel, learningRateTextBox, hiddenLabel, hiddenTextBox, epochsLabel, epochsTextBox, trainButton, experimentButton, trainingProgressBar, lossLabel });
            evaluationGroup.Text = "Model Evaluation (held-out test set)"; evaluationGroup.Location = new Point(340, 135); evaluationGroup.Size = new Size(430, 225);
            accuracyLabel.AutoSize = true; accuracyLabel.Location = new Point(15, 32); accuracyLabel.Text = "Accuracy: -"; precisionLabel.AutoSize = true; precisionLabel.Location = new Point(15, 60); precisionLabel.Text = "Precision: -"; recallLabel.AutoSize = true; recallLabel.Location = new Point(15, 88); recallLabel.Text = "Recall: -"; f1Label.AutoSize = true; f1Label.Location = new Point(15, 116); f1Label.Text = "F1-score: -"; confusionLabel.AutoSize = true; confusionLabel.Location = new Point(15, 155); confusionLabel.Text = "TP: -   TN: -   FP: -   FN: -";
            evaluationGroup.Controls.AddRange(new Control[] { accuracyLabel, precisionLabel, recallLabel, f1Label, confusionLabel });
            lossChart.Location = new Point(790, 135); lossChart.Size = new Size(390, 225); ChartArea area = new ChartArea("Loss"); lossChart.ChartAreas.Add(area); Series series = new Series("Training Loss"); series.ChartType = SeriesChartType.Line; series.ChartArea = "Loss"; lossChart.Series.Add(series); lossChart.Titles.Add("Training Loss");
            predictionPanel.Location = new Point(20, 380); predictionPanel.Size = new Size(1160, 180); predictionPanel.BorderStyle = BorderStyle.FixedSingle;
            predictionTitleLabel.AutoSize = true; predictionTitleLabel.Font = new Font("Segoe UI", 11F, FontStyle.Bold); predictionTitleLabel.Location = new Point(15, 12); predictionTitleLabel.Text = "Student Prediction (G1, G2, studytime, failures, absences)";
            AddPredictionField(g1Label, g1TextBox, "G1", 20); AddPredictionField(g2Label, g2TextBox, "G2", 170); AddPredictionField(studyTimeLabel, studyTimeTextBox, "Study time", 320); AddPredictionField(failuresLabel, failuresTextBox, "Failures", 500); AddPredictionField(absencesLabel, absencesTextBox, "Absences", 650);
            predictButton.Location = new Point(820, 55); predictButton.Size = new Size(120, 32); predictButton.Text = "Predict"; clearButton.Location = new Point(950, 55); clearButton.Size = new Size(100, 32); clearButton.Text = "Clear"; predictionLabel.AutoSize = true; predictionLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold); predictionLabel.Location = new Point(20, 125); predictionLabel.Text = "Prediction: train the model first";
            predictionPanel.Controls.AddRange(new Control[] { predictionTitleLabel, g1Label, g1TextBox, g2Label, g2TextBox, studyTimeLabel, studyTimeTextBox, failuresLabel, failuresTextBox, absencesLabel, absencesTextBox, predictButton, clearButton, predictionLabel });
            ClientSize = new Size(1200, 590); Controls.AddRange(new Control[] { titleLabel, descriptionLabel, datasetStatusLabel, modelStatusLabel, configurationGroup, evaluationGroup, lossChart, predictionPanel }); Text = "BackPropCalculator - Student Prediction"; StartPosition = FormStartPosition.CenterScreen; AutoScaleMode = AutoScaleMode.Font;
            ((System.ComponentModel.ISupportInitialize)(lossChart)).EndInit(); ResumeLayout(false); PerformLayout();
        }

        private static int thirtyFive() { return 35; }
        private void AddPredictionField(Label label, TextBox box, string text, int x)
        {
            label.AutoSize = true; label.Location = new Point(x, 58); label.Text = text; box.Location = new Point(x, 82); box.Size = new Size(110, 23);
        }
    }
}

