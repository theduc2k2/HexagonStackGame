using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Text unityScoreText; // Thêm hỗ trợ Text thường của Unity
    [SerializeField] private Slider scoreProgressBar;
    [SerializeField] private Transform scoreIconTarget; // Ô icon hoặc vị trí cụ thể để hex bay tới

    public int CurrentScore { get; private set; } = 0;
    private float displayedScore = 0;
    [SerializeField] private float countSpeed = 100f;

    [System.Serializable]
    public class LevelScorePair
    {
        public GameObject levelMap;    // GameObject map
        public int targetScore;        // Điểm mục tiêu cho map này
    }

    [Header("Level Settings")]
    [SerializeField] private List<LevelScorePair> levelScores = new List<LevelScorePair>(); // Danh sách các cặp map và điểm

    private int currentTargetScore;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Reset trạng thái khi play trong Editor để test, nhưng giữ nguyên khi build
#if UNITY_EDITOR
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("LevelUnlocked_1", 1); // Mặc định mở Level 1
        PlayerPrefs.Save();
#endif
    }

    private void Update()
    {
        if (displayedScore < CurrentScore)
        {
            displayedScore = Mathf.MoveTowards(displayedScore, CurrentScore, Time.deltaTime * countSpeed);
            UpdateScoreText();
        }
    }

    private void Start()
    {
        InitLevel();
    }

    public void InitLevel()
    {
        CurrentScore = 0;
        displayedScore = 0;

        int currentLevelIndex = LevelController.Instance != null ? LevelController.Instance.currentLevel : 0;
        SetTargetScore(currentLevelIndex);
        UpdateUI();
    }

    public void AddPoints(int amount)
    {
        CurrentScore += amount;

        if (scoreProgressBar != null)
        {
            scoreProgressBar.maxValue = currentTargetScore;
            scoreProgressBar.value = CurrentScore;
        }

        UpdateScoreText();

        if (scoreText != null)
        {
            LeanTween.cancel(scoreText.gameObject);
            scoreText.transform.localScale = Vector3.one;
            LeanTween.scale(scoreText.gameObject, Vector3.one * 1.2f, 0.15f)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnComplete(() =>
                {
                    LeanTween.scale(scoreText.gameObject, Vector3.one, 0.15f)
                        .setEase(LeanTweenType.easeInQuad);
                });
        }

        if (unityScoreText != null)
        {
            LeanTween.cancel(unityScoreText.gameObject);
            unityScoreText.transform.localScale = Vector3.one;
            LeanTween.scale(unityScoreText.gameObject, Vector3.one * 1.2f, 0.15f)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnComplete(() =>
                {
                    LeanTween.scale(unityScoreText.gameObject, Vector3.one, 0.15f)
                        .setEase(LeanTweenType.easeInQuad);
                });
        }

        if (CurrentScore >= currentTargetScore)
        {
            Debug.Log($"🎯 Level {LevelController.Instance?.currentLevel + 1} Complete!");

            if (LevelController.Instance != null)
            {
                LevelController.Instance.OnLevelCompleted();
            }
        }
    }

    private void UpdateUI()
    {
        UpdateScoreText();

        if (scoreProgressBar != null)
        {
            scoreProgressBar.maxValue = currentTargetScore;
            scoreProgressBar.value = CurrentScore;
        }
    }

    private void UpdateScoreText()
    {
        string textValue = $"{(int)displayedScore} / {currentTargetScore}";

        if (scoreText != null)
            scoreText.text = textValue;
        
        if (unityScoreText != null)
            unityScoreText.text = textValue;
    }

    public Vector3 GetScoreWorldPosition()
    {
        // Ưu tiên dùng scoreIconTarget nếu được gán trong Inspector
        if (scoreIconTarget != null) return scoreIconTarget.position;
        
        // Nếu không thì dùng vị trí của text điểm số (TMP hoặc Legacy Text)
        if (scoreText != null) return scoreText.transform.position;
        if (unityScoreText != null) return unityScoreText.transform.position;
        
        return Vector3.up * 10f;
    }

    private void SetTargetScore(int levelIndex)
    {
        if (levelScores == null || levelScores.Count == 0)
        {
            Debug.LogError("⚠️ Danh sách levelScores chưa được gán hoặc rỗng!");
            currentTargetScore = 1000; // Giá trị mặc định
        }
        else if (levelIndex < 0 || levelIndex >= levelScores.Count)
        {
            Debug.LogWarning($"Level index {levelIndex} không hợp lệ, đặt về 0");
            currentTargetScore = levelScores[0].targetScore;
        }
        else
        {
            currentTargetScore = levelScores[levelIndex].targetScore;
            Debug.Log($"🎯 Mục tiêu điểm Level {levelIndex + 1}: {currentTargetScore}");
        }
    }

    // Thêm phương thức public để lấy target score
    public int? GetTargetScore()
    {
        return currentTargetScore > 0 ? currentTargetScore : (int?)null;
    }
}