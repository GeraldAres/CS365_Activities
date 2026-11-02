using System;
using System.Collections.Generic;

namespace BinaryClassification.Data
{
    public sealed class DataNormalizer
    {
        public double[] Minimums { get; private set; }
        public double[] Maximums { get; private set; }

        public DataNormalizer(int featureCount)
        {
            if (featureCount <= 0) throw new ArgumentOutOfRangeException("featureCount");
            Minimums = new double[featureCount];
            Maximums = new double[featureCount];
        }

        public void Fit(IList<StudentRecord> records)
        {
            if (records == null || records.Count == 0)
                throw new ArgumentException("Training records cannot be empty.", "records");

            for (int feature = 0; feature < Minimums.Length; ++feature)
            {
                Minimums[feature] = double.MaxValue;
                Maximums[feature] = double.MinValue;
            }

            foreach (StudentRecord record in records)
            {
                if (record.Features.Length != Minimums.Length)
                    throw new ArgumentException("A record has an unexpected feature count.", "records");
                for (int feature = 0; feature < Minimums.Length; ++feature)
                {
                    Minimums[feature] = Math.Min(Minimums[feature], record.Features[feature]);
                    Maximums[feature] = Math.Max(Maximums[feature], record.Features[feature]);
                }
            }
        }

        public double[] Transform(double[] features)
        {
            if (features == null || features.Length != Minimums.Length)
                throw new ArgumentException("The feature vector has an unexpected length.", "features");

            double[] normalized = new double[features.Length];
            for (int i = 0; i < features.Length; ++i)
            {
                double range = Maximums[i] - Minimums[i];
                normalized[i] = range == 0.0 ? 0.0 : (features[i] - Minimums[i]) / range;
            }
            return normalized;
        }

        public double[][] Transform(IList<StudentRecord> records)
        {
            double[][] result = new double[records.Count][];
            for (int i = 0; i < records.Count; ++i)
            {
                double[] normalized = Transform(records[i].Features);
                result[i] = new double[normalized.Length + 1];
                Array.Copy(normalized, result[i], normalized.Length);
                result[i][normalized.Length] = records[i].Label;
            }
            return result;
        }
    }
}
