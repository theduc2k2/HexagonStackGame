namespace Project.Core.Domain.Stack
{
    public readonly struct StackConfig
    {
        public int MinCount { get; }
        public int MaxCount { get; }

        public StackConfig(int minCount, int maxCount)
        {
            MinCount = minCount;
            MaxCount = maxCount;
        }

        public StackConfig Normalized(int absoluteMin, int absoluteMax)
        {
            int min = Clamp(MinCount <= MaxCount ? MinCount : MaxCount, absoluteMin, absoluteMax);
            int max = Clamp(MaxCount >= MinCount ? MaxCount : MinCount, min, absoluteMax);
            return new StackConfig(min, max);
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
