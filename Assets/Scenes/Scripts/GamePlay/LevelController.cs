using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance;

    [Header("Level Configuration")]
    public GameObject[] levels;
    public int currentLevel = 0;

    [Header("UI Elements")]
    public Button nextButton;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject canvasGameplay;
    [SerializeField] private GameObject canvasMenuGame;

    private Animator panelAnimator;
    private bool isWaitingForNext = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ LevelController đã được khởi tạo.");
        }
        else
        {
            Debug.LogWarning("⚠️ Một instance khác của LevelController đã tồn tại, hủy instance này.");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        int selectedLevel = PlayerPrefs.GetInt("SelectedLevel", 1) - 1;
        currentLevel = Mathf.Clamp(selectedLevel, 0, levels.Length - 1);
        ActivateLevel(currentLevel);
        ScoreManager.Instance?.InitLevel();

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextLevelButtonClicked);
            nextButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("⚠️ NextButton chưa được gán trong Inspector!");
        }

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
            panelAnimator = levelCompletePanel.GetComponent<Animator>();
        }
        else
        {
            Debug.LogWarning("⚠️ LevelCompletePanel chưa được gán trong Inspector!");
        }

        if (levelText != null)
        {
            levelText.gameObject.SetActive(true);
            UpdateLevelText();
            Debug.Log("✅ levelText khởi tạo và hiển thị: " + levelText.text);
        }
        else
        {
            Debug.LogWarning("⚠️ levelText chưa được gán trong Inspector!");
        }

        if (canvasGameplay != null)
        {
            canvasGameplay.SetActive(true);
        }
        else
        {
            Debug.LogWarning("⚠️ canvasGameplay chưa được gán trong Inspector!");
        }

        if (canvasMenuGame != null)
        {
            canvasMenuGame.SetActive(false);
        }
        else
        {
            Debug.LogWarning("⚠️ canvasMenuGame chưa được gán trong Inspector!");
        }
    }

    public void ActivateLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Length)
        {
            Debug.LogError($"⚠️ Level index {levelIndex + 1} ngoài phạm vi mảng levels!");
            return;
        }

        currentLevel = levelIndex;
        Debug.Log($"Activating Level {levelIndex + 1}");
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null)
            {
                levels[i].SetActive(i == levelIndex);
                Debug.Log($"Level {i + 1} set active: {i == levelIndex}");
            }
            else
            {
                Debug.LogError($"⚠️ Level {i + 1} trong mảng levels chưa được gán!");
            }
        }

        if (levelText != null)
        {
            levelText.gameObject.SetActive(true);
            UpdateLevelText();
            Debug.Log("✅ levelText cập nhật trong ActivateLevel: " + levelText.text);
        }

        ScoreManager.Instance?.InitLevel();
        ResetLevelState();

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SyncLevel(currentLevel);
            Debug.Log("✅ Đã đồng bộ currentLevel với LevelManager.");
        }
    }

    private void ResetLevelState()
    {
        GridCell[] gridCells = levels[currentLevel].GetComponentsInChildren<GridCell>();
        foreach (GridCell cell in gridCells)
        {
            if (cell.IsOccupied)
            {
                cell.ClearHexStack();
            }
        }
    }

    public void OnLevelCompleted()
    {
        if (levelCompletePanel == null || panelAnimator == null)
        {
            Debug.LogWarning("⚠️ LevelCompletePanel hoặc Animator chưa được gán!");
            return;
        }

        isWaitingForNext = true;

        levelCompletePanel.SetActive(true);
        panelAnimator.ResetTrigger("IdleNextLevel");
        panelAnimator.SetTrigger("NextLevel");

        StartCoroutine(HandleLevelCompleteUI());
    }

    private IEnumerator HandleLevelCompleteUI()
    {
        float duration = GetAnimationClipLength("NextLevel");
        yield return new WaitForSeconds(duration);

        if (panelAnimator != null)
        {
            panelAnimator.ResetTrigger("NextLevel");
            panelAnimator.SetTrigger("IdleNextLevel");
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);
            if (levelText != null)
            {
                levelText.gameObject.SetActive(false);
                Debug.Log("✅ levelText ẩn khi nút Next Level xuất hiện");
            }
        }
    }

    public void OnNextLevelButtonClicked()
    {
        if (!isWaitingForNext)
            return;

        isWaitingForNext = false;

        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);

        currentLevel++;
        if (currentLevel < levels.Length)
        {
            ActivateLevel(currentLevel);
            int levelToUnlock = currentLevel + 1;
            PlayerPrefs.SetInt($"LevelUnlocked_{levelToUnlock}", 1);
            PlayerPrefs.Save();

            if (LevelManagerMenu.Instance != null)
            {
                LevelManagerMenu.Instance.UpdateLockState(currentLevel, false);
                Debug.Log($"🔓 Đã mở khóa Level {levelToUnlock}");
            }
            else
            {
                Debug.LogWarning($"⚠️ LevelManagerMenu.Instance không tồn tại khi mở khóa Level {levelToUnlock}!");
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.SyncLevel(currentLevel);
                Debug.Log("✅ Đã đồng bộ currentLevel với LevelManager sau khi nhấn Next.");
            }
        }
        else
        {
            Debug.Log("🎉 Bạn đã hoàn thành tất cả các màn!");
            currentLevel = levels.Length - 1;
            if (levelText != null)
            {
                levelText.gameObject.SetActive(true);
                UpdateLevelText();
                Debug.Log("✅ levelText hiển thị khi hoàn thành tất cả: " + levelText.text);
            }
        }
    }

    public void SwitchToMenu()
    {
        if (canvasGameplay != null) canvasGameplay.SetActive(false);
        if (canvasMenuGame != null) canvasMenuGame.SetActive(true);
        Debug.Log("✅ Đã chuyển sang Canvas Menu Game");
    }

    public void SwitchToGameplay()
    {
        if (canvasMenuGame != null) canvasMenuGame.SetActive(false);
        if (canvasGameplay != null) canvasGameplay.SetActive(true);
        Debug.Log("✅ Đã chuyển về Canvas Gameplay");
    }

    public void InitFirstLevel()
    {
        currentLevel = 0;
        ActivateLevel(currentLevel);
        if (levelText != null)
        {
            levelText.gameObject.SetActive(true);
            UpdateLevelText();
        }
    }

    private float GetAnimationClipLength(string clipName)
    {
        if (panelAnimator == null || panelAnimator.runtimeAnimatorController == null)
            return 1f;

        foreach (AnimationClip clip in panelAnimator.runtimeAnimatorController.animationClips)
        {
            if (clip != null && clip.name == clipName)
                return clip.length;
        }

        return 1f;
    }

    public void CheckGameOver(bool isMerged)
    {
        GridCell[] gridCells = levels[currentLevel].GetComponentsInChildren<GridCell>();
        bool allOccupied = gridCells.All(cell => cell.IsOccupied);
        int currentScore = ScoreManager.Instance?.CurrentScore ?? 0;
        int targetScore = ScoreManager.Instance?.GetTargetScore() ?? 0;

        Debug.Log($"🔍 CheckGameOver - AllOccupied: {allOccupied}, IsMerged: {isMerged}, Score: {currentScore}/{targetScore}");
        Debug.Log($"Total GridCells: {gridCells.Length}, Occupied: {gridCells.Count(cell => cell.IsOccupied)}");

        if (allOccupied && !isMerged && currentScore < targetScore)
        {
            Debug.Log("❌ GAME OVER: Tất cả ô đã bị chiếm, không có hợp nhất mới và chưa đạt điểm mục tiêu!");
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelFailed();
            }
            else
            {
                Debug.LogError("⚠️ LevelManager.Instance chưa được khởi tạo!");
            }
        }
    }

    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = "Level: " + (currentLevel + 1);
            Debug.Log("✅ UpdateLevelText gọi, hiển thị: " + levelText.text);
        }
    }
}