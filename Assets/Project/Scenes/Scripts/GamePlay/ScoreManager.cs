using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Text unityScoreText;
    [SerializeField] private Slider scoreProgressBar;
    [SerializeField] private Transform scoreIconTarget;
    [SerializeField] private ScoreFillFxController scoreFillFxController;

    [SerializeField] private float countSpeed = 100f;

    [System.Serializable]
    public class LevelScorePair
    {
        public GameObject levelMap;
        public int targetScore;
    }

    [Header("Level Settings")]
    [SerializeField] private List<LevelScorePair> levelScores = new List<LevelScorePair>();

    private readonly ScoreState scoreState = new ScoreState();
    private float displayedScore;
    private bool hasCompletedLevel;
    private bool loggedMissingScoreFxWarning;

    public int CurrentScore => scoreState.CurrentScore;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            EnsureScoreFillFxController();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitLevel();
    }

    private void Update()
    {
        if (displayedScore >= scoreState.CurrentScore)
            return;

        displayedScore = Mathf.MoveTowards(displayedScore, scoreState.CurrentScore, Time.deltaTime * countSpeed);
        UpdateScoreText();
    }

    public void InitLevel()
    {
        int currentLevelIndex = LevelController.Instance != null ? LevelController.Instance.currentLevel : 0;
        int targetScore = ResolveTargetScore(currentLevelIndex);

        scoreState.Reset(targetScore);
        displayedScore = 0;
        hasCompletedLevel = false;
        UpdateUI();
    }

    public void AddPoints(int amount)
    {
        scoreState.Add(amount);
        UpdateProgressBar();
        UpdateScoreText();
        AnimateScoreFeedback();

        if (!hasCompletedLevel && scoreState.IsComplete)
        {
            hasCompletedLevel = true;
            LevelController.Instance?.OnLevelCompleted();
        }
    }

    public Vector3 GetScoreWorldPosition()
    {
        if (scoreIconTarget != null) return scoreIconTarget.position;
        if (scoreText != null) return scoreText.transform.position;
        if (unityScoreText != null) return unityScoreText.transform.position;
        return Vector3.up * 10f;
    }

    public int? GetTargetScore()
    {
        return scoreState.TargetScore > 0 ? scoreState.TargetScore : (int?)null;
    }

    public void PlayScoreFillFxSequence(int burstCount, float interval, float startDelay = 0f)
    {
        EnsureScoreFillFxController();
        if (scoreFillFxController == null)
        {
            if (!loggedMissingScoreFxWarning)
            {
                Debug.LogWarning("ScoreFillFxController is missing. Attach it to the score icon FX root or assign it in ScoreManager.");
                loggedMissingScoreFxWarning = true;
            }
            return;
        }

        scoreFillFxController.PlayBurstSequence(burstCount, interval, startDelay);
    }

    private void EnsureScoreFillFxController()
    {
        bool attachedToScoreIcon = scoreFillFxController != null
            && scoreIconTarget != null
            && (scoreFillFxController.transform == scoreIconTarget || scoreFillFxController.transform.IsChildOf(scoreIconTarget));

        if (scoreFillFxController != null && scoreFillFxController.gameObject.activeInHierarchy && attachedToScoreIcon)
            return;

        ScoreFillFxController inactiveTemplateController = null;
        ScoreFillFxController[] allControllers = Resources.FindObjectsOfTypeAll<ScoreFillFxController>();
        foreach (ScoreFillFxController controller in allControllers)
        {
            if (controller == null || !controller.gameObject.scene.IsValid())
                continue;

            if (controller.gameObject.activeInHierarchy)
            {
                scoreFillFxController = controller;
                return;
            }

            if (inactiveTemplateController == null && controller.HasTemplates)
                inactiveTemplateController = controller;
        }

        GameObject host = scoreIconTarget != null ? scoreIconTarget.gameObject : gameObject;
        ScoreFillFxController runtimeController = host.GetComponent<ScoreFillFxController>();
        if (runtimeController == null)
            runtimeController = host.AddComponent<ScoreFillFxController>();

        if (scoreFillFxController != null && runtimeController != scoreFillFxController)
            runtimeController.SetParticleTemplates(scoreFillFxController.GetParticleTemplates());
        else if (inactiveTemplateController != null && runtimeController != inactiveTemplateController)
            runtimeController.SetParticleTemplates(inactiveTemplateController.GetParticleTemplates());

        scoreFillFxController = runtimeController;
    }

    private void UpdateUI()
    {
        UpdateScoreText();
        UpdateProgressBar();
    }

    private void UpdateProgressBar()
    {
        if (scoreProgressBar == null)
            return;

        scoreProgressBar.maxValue = scoreState.TargetScore;
        scoreProgressBar.value = scoreState.CurrentScore;
    }

    private void UpdateScoreText()
    {
        string textValue = $"{(int)displayedScore} / {scoreState.TargetScore}";

        if (scoreText != null)
            scoreText.text = textValue;

        if (unityScoreText != null)
            unityScoreText.text = textValue;
    }

    private void AnimateScoreFeedback()
    {
        if (scoreText != null)
            AnimateScalePulse(scoreText.gameObject);

        if (unityScoreText != null)
            AnimateScalePulse(unityScoreText.gameObject);
    }

    private static void AnimateScalePulse(GameObject target)
    {
        LeanTween.cancel(target);
        target.transform.localScale = Vector3.one;
        LeanTween.scale(target, Vector3.one * 1.2f, 0.15f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
            {
                LeanTween.scale(target, Vector3.one, 0.15f)
                    .setEase(LeanTweenType.easeInQuad);
            });
    }

    private int ResolveTargetScore(int levelIndex)
    {
        if (levelScores == null || levelScores.Count == 0)
        {
            Debug.LogError("levelScores is empty. Using fallback target score 1000.");
            return 1000;
        }

        if (levelIndex < 0 || levelIndex >= levelScores.Count)
        {
            Debug.LogWarning($"Level index {levelIndex} is invalid for score targets. Falling back to level 0.");
            return levelScores[0].targetScore;
        }

        return levelScores[levelIndex].targetScore;
    }
}
