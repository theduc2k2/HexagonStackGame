namespace Project.Core.Domain.Merge
{
    public readonly struct MergeRule
    {
        public int CompleteThreshold { get; }

        public MergeRule(int completeThreshold)
        {
            CompleteThreshold = completeThreshold;
        }
    }
}
