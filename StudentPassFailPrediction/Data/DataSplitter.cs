using System;
using System.Collections.Generic;

namespace BinaryClassification.Data
{
    public sealed class DataSplit
    {
        public List<StudentRecord> Training { get; private set; }
        public List<StudentRecord> Testing { get; private set; }

        public DataSplit(List<StudentRecord> training, List<StudentRecord> testing)
        {
            Training = training;
            Testing = testing;
        }
    }

    public static class DataSplitter
    {
        public static DataSplit StratifiedSplit(IList<StudentRecord> records, double trainingPercentage, int seed)
        {
            if (records == null || records.Count == 0)
                throw new ArgumentException("Records cannot be empty.", "records");
            if (trainingPercentage <= 0.0 || trainingPercentage >= 1.0)
                throw new ArgumentOutOfRangeException("trainingPercentage");

            List<StudentRecord> pass = new List<StudentRecord>();
            List<StudentRecord> fail = new List<StudentRecord>();
            foreach (StudentRecord record in records)
            {
                if (record.Label == 1) pass.Add(record);
                else fail.Add(record);
            }

            Random random = new Random(seed);
            Shuffle(pass, random);
            Shuffle(fail, random);

            int passTrainingCount = TrainingCount(pass.Count, trainingPercentage);
            int failTrainingCount = TrainingCount(fail.Count, trainingPercentage);
            List<StudentRecord> training = new List<StudentRecord>();
            List<StudentRecord> testing = new List<StudentRecord>();
            AddPartition(pass, passTrainingCount, training, testing);
            AddPartition(fail, failTrainingCount, training, testing);
            Shuffle(training, random);
            Shuffle(testing, random);
            return new DataSplit(training, testing);
        }

        private static int TrainingCount(int count, double percentage)
        {
            if (count <= 1) return count;
            return Math.Min(count - 1, Math.Max(1, (int)Math.Round(count * percentage)));
        }

        private static void AddPartition(List<StudentRecord> source, int trainingCount, List<StudentRecord> training, List<StudentRecord> testing)
        {
            for (int i = 0; i < source.Count; ++i)
            {
                if (i < trainingCount) training.Add(source[i]);
                else testing.Add(source[i]);
            }
        }

        private static void Shuffle(List<StudentRecord> records, Random random)
        {
            for (int i = 0; i < records.Count; ++i)
            {
                int index = random.Next(i, records.Count);
                StudentRecord temporary = records[index];
                records[index] = records[i];
                records[i] = temporary;
            }
        }
    }
}
