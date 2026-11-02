using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace BinaryClassification.Data
{
    public sealed class StudentRecord
    {
        public double[] Features { get; private set; }
        public int Label { get; private set; }

        public StudentRecord(double[] features, int label)
        {
            Features = features;
            Label = label;
        }
    }

    public static class DataLoader
    {
        public static readonly string[] SelectedFeatures = { "G1", "G2", "studytime", "failures", "absences" };
        public const string TargetColumn = "G3";
        public const int PassingGrade = 10;

        public static List<StudentRecord> Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("The student dataset was not found.", path);

            string[] lines = File.ReadAllLines(path);
            if (lines.Length < 2)
                throw new InvalidDataException("The student dataset does not contain any records.");

            string[] headers = lines[0].Split(';');
            Dictionary<string, int> columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; ++i)
                columns[headers[i].Trim()] = i;

            foreach (string feature in SelectedFeatures)
                if (!columns.ContainsKey(feature))
                    throw new InvalidDataException("The dataset is missing the required feature: " + feature);
            if (!columns.ContainsKey(TargetColumn))
                throw new InvalidDataException("The dataset is missing the target column: " + TargetColumn);

            List<StudentRecord> records = new List<StudentRecord>();
            for (int lineNumber = 1; lineNumber < lines.Length; ++lineNumber)
            {
                if (string.IsNullOrWhiteSpace(lines[lineNumber]))
                    continue;

                string[] values = lines[lineNumber].Split(';');
                try
                {
                    double[] features = new double[SelectedFeatures.Length];
                    for (int i = 0; i < SelectedFeatures.Length; ++i)
                    {
                        string text = values[columns[SelectedFeatures[i]]].Trim().Trim('"');
                        features[i] = double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
                    }

                    double finalGrade = double.Parse(values[columns[TargetColumn]].Trim().Trim('"'), NumberStyles.Float, CultureInfo.InvariantCulture);
                    records.Add(new StudentRecord(features, finalGrade >= PassingGrade ? 1 : 0));
                }
                catch (Exception exception) when (exception is FormatException || exception is IndexOutOfRangeException || exception is OverflowException)
                {
                    throw new InvalidDataException("Invalid or incomplete data on CSV line " + (lineNumber + 1) + ".", exception);
                }
            }

            if (records.Count == 0)
                throw new InvalidDataException("No valid student records were loaded.");
            return records;
        }
    }
}
