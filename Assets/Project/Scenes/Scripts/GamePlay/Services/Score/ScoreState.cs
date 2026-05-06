public sealed class ScoreState
{
    public int CurrentScore { get; private set; }
    public int TargetScore { get; private set; }
    public bool IsComplete => TargetScore > 0 && CurrentScore >= TargetScore;

    public void Reset(int targetScore)
    {
        CurrentScore = 0;
        TargetScore = targetScore;
    }

    public void Add(int amount)
    {
        if (amount <= 0)
            return;

        CurrentScore += amount;
    }
}
