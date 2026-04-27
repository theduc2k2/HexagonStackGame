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
    }
}
