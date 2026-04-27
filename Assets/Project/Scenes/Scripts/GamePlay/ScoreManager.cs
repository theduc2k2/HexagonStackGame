using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Slider scoreProgressBar;

    public int CurrentScore { get; private set; } = 0;

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

    private void Start()
    {
        InitLevel();
    }

    public void InitLevel()
    {
        CurrentScore = 0;

        int currentLevelIndex = LevelController.Instance != null ? LevelController.Instance.currentLevel : 0;
        SetTargetScore(currentLevelIndex);
        UpdateUI();
    }

    public void AddPoints(int amount)
    {
        CurrentScore += amount;

        UpdateUI();

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
        if (scoreText != null)
            scoreText.text = $"{CurrentScore} / {currentTargetScore}";

        if (scoreProgressBar != null)
        {
            scoreProgressBar.maxValue = currentTargetScore;
            scoreProgressBar.value = CurrentScore;
        }
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