namespace BinaryClassification.Evaluation
{
    public sealed class ConfusionMatrix
    {
        public int TruePositive { get; private set; }
        public int TrueNegative { get; private set; }
        public int FalsePositive { get; private set; }
        public int FalseNegative { get; private set; }

        public void Add(int actual, int predicted)
        {
            if (actual == 1 && predicted == 1) ++TruePositive;
            else if (actual == 0 && predicted == 0) ++TrueNegative;
            else if (actual == 0 && predicted == 1) ++FalsePositive;
            else if (actual == 1 && predicted == 0) ++FalseNegative;
        }

        public int Total
        {
            get { return TruePositive + TrueNegative + FalsePositive + FalseNegative; }
        }
    }
}
