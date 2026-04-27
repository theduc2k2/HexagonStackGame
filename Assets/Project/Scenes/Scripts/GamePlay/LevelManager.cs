using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("UI References - Level Complete")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private Animator panelAnimator;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button nextButton;

    [Header("UI References - Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Animator gameOverAnimator;
    [SerializeField] private Button retryButton;

    private bool isWaitingForNext = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ LevelManager đã được khởi tạo.");
        }
        else
        {
            Debug.LogWarning("⚠️ Một instance khác của LevelManager đã tồn tại, hủy instance này.");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
            panelAnimator = levelCompletePanel.GetComponent<Animator>();
        }
        else
        {
            Debug.LogWarning("⚠️ levelCompletePanel chưa được gán trong Inspector!");
        }

        if (levelText != null)
        {
            levelText.gameObject.SetActive(true);
            SyncLevel(LevelController.Instance != null ? LevelController.Instance.currentLevel : 0);
        }
        else
        {
            Debug.LogWarning("⚠️ levelText chưa được gán trong Inspector!");
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
            nextButton.onClick.AddListener(OnNextLevelButtonClicked);
        }
        else
        {
            Debug.LogWarning("⚠️ nextButton chưa được gán trong Inspector!");
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            gameOverAnimator = gameOverPanel.GetComponent<Animator>();
        }
        else
        {
            Debug.LogWarning("⚠️ gameOverPanel chưa được gán trong Inspector!");
        }

        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(false);
            retryButton.onClick.AddListener(OnRetryButtonClicked);
        }
        else
        {
            Debug.LogWarning("⚠️ retryButton chưa được gán trong Inspector!");
        }
    }

    public void SyncLevel(int level)
    {
        UpdateLevelText(level);
    }

    public void CompleteLevel()
    {
        if (isWaitingForNext || levelCompletePanel == null || panelAnimator == null) return;
        isWaitingForNext = true;

        levelCompletePanel.SetActive(true);
        panelAnimator.ResetTrigger("IdleNextLevel");
        panelAnimator.SetTrigger("NextLevel");

        StartCoroutine(HandleLevelCompleteUI());
    }

    public void OnLevelFailed()
    {
        if (isWaitingForNext || gameOverPanel == null || gameOverAnimator == null) return;
        isWaitingForNext = true;

        gameOverPanel.SetActive(true);
        gameOverAnimator.ResetTrigger("GameOverIdle");
        gameOverAnimator.SetTrigger("GameOver");

        StartCoroutine(HandleLevelFailedUI());
    }

    private IEnumerator HandleLevelCompleteUI()
    {
        float duration = GetAnimationClipLength("NextLevel", panelAnimator);
        yield return new WaitForSeconds(duration);

        if (panelAnimator != null)
        {
            panelAnimator.ResetTrigger("NextLevel");
            panelAnimator.SetTrigger("IdleNextLevel");
        }

        if (levelText != null)
        {
            levelText.gameObject.SetActive(false);
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);
        }
    }

    private IEnumerator HandleLevelFailedUI()
    {
        float duration = GetAnimationClipLength("GameOver", gameOverAnimator);
        yield return new WaitForSeconds(duration);

        if (gameOverAnimator != null)
        {
            gameOverAnimator.ResetTrigger("GameOver");
            gameOverAnimator.SetTrigger("GameOverIdle");
        }

        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(true);
            Debug.Log("❌ Hiển thị nút Game Over khi thua cuộc");
        }
    }

    private void OnNextLevelButtonClicked()
    {
        if (!isWaitingForNext) return;

        isWaitingForNext = false;

        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);

        if (LevelController.Instance != null)
        {
            LevelController.Instance.OnNextLevelButtonClicked();
        }

        if (levelText != null)
        {
            levelText.gameObject.SetActive(true);
            SyncLevel(LevelController.Instance != null ? LevelController.Instance.currentLevel : 0);
        }
    }

    private void OnRetryButtonClicked()
    {
        if (!isWaitingForNext) return;

        isWaitingForNext = false;

        if (retryButton != null) retryButton.gameObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (LevelController.Instance != null)
        {
            LevelController.Instance.ActivateLevel(LevelController.Instance.currentLevel);
            Debug.Log($"🔄 Quay lại Level {LevelController.Instance.currentLevel + 1}");
        }
    }

    public void RestartFromBeginning()
    {
        isWaitingForNext = false;

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (levelText != null)
        {
            levelText.gameObject.SetActive(true);
            SyncLevel(0);
        }

        if (LevelController.Instance != null)
        {
            LevelController.Instance.InitFirstLevel();
        }
    }

    private float GetAnimationClipLength(string clipName, Animator animator)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return 1f;

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip != null && clip.name == clipName)
                return clip.length;
        }

        return 1f;
    }

    private void UpdateLevelText(int level)
    {
        if (levelText != null)
        {
            levelText.text = "Level: " + (level + 1);
            Debug.Log($"✅ LevelManager: Cập nhật levelText: {levelText.text}");
        }
    }
}