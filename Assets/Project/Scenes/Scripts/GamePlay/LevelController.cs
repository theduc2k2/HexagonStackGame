using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;
using Project.Application.UseCases;
using Project.Infrastructure.Persistence;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance;

    [Header("Level Data")]
    public LevelData[] levelDatas;
    public GridCell gridCellPrefab;

    [Header("Auto Scale Settings")]
    [SerializeField] private float maxMapWidth = 15f;
    [SerializeField] private float maxMapHeight = 15f;

    [Header("Pooling")]
    public Transform gridParent;

    public int currentLevel = 0;

    [Header("UI Elements")]
    public Button nextButton;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject canvasGameplay;
    [SerializeField] private GameObject canvasMenuGame;

    private Animator panelAnimator;
    private bool isWaitingForNext;

    private ILevelDataProvider levelDataProvider;
    private LevelGridBuilder gridBuilder;
    private LevelAccessUseCase levelAccessUseCase;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            levelDataProvider = new ResourcesLevelDataProvider("Levels");
            levelAccessUseCase = new LevelAccessUseCase(new PlayerPrefsLevelUnlockService());
            AutoLoadLevelData();
        }
        else
        {
            Debug.LogWarning("Another LevelController instance already exists. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }

    [ContextMenu("Auto Load Levels")]
    private void AutoLoadLevelData()
    {
        levelDataProvider ??= new ResourcesLevelDataProvider("Levels");
        levelDatas = levelDataProvider.LoadLevels();

        if (levelDatas != null && levelDatas.Length > 0)
            Debug.Log($"Loaded {levelDatas.Length} levels from Resources/Levels.");
        else
            Debug.LogWarning("No LevelData files found in Resources/Levels.");
    }

    private void Start()
    {
        int selectedLevel = PlayerPrefs.GetInt("SelectedLevel", 1) - 1;
        currentLevel = Mathf.Clamp(selectedLevel, 0, HasLevels ? levelDatas.Length - 1 : 0);

        EnsureGridParent();
        CreateGridBuilder();
        ActivateLevel(currentLevel);
        ScoreManager.Instance?.InitLevel();

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextLevelButtonClicked);
            nextButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("NextButton is not assigned in LevelController.");
        }

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
            panelAnimator = levelCompletePanel.GetComponent<Animator>();
        }
        else
        {
            Debug.LogWarning("LevelCompletePanel is not assigned in LevelController.");
        }

        if (levelText != null)
        {
            levelText.gameObject.SetActive(true);
            UpdateLevelText();
        }
        else
        {
            Debug.LogWarning("levelText is not assigned in LevelController.");
        }

        if (canvasGameplay != null)
            canvasGameplay.SetActive(true);
        else
            Debug.LogWarning("canvasGameplay is not assigned in LevelController.");

        if (canvasMenuGame != null)
            canvasMenuGame.SetActive(false);
        else
            Debug.LogWarning("canvasMenuGame is not assigned in LevelController.");
    }

    public void ActivateLevel(int levelIndex)
    {
        if (!HasLevels)
        {
            Debug.LogError("levelDatas is empty. Assign LevelData assets or place them under Resources/Levels.");
            return;
        }

        if (levelIndex < 0 || levelIndex >= levelDatas.Length)
        {
            Debug.LogError($"Level index {levelIndex} is out of bounds.");
            return;
        }

        if (gridCellPrefab == null)
        {
            Debug.LogError("gridCellPrefab is not assigned in LevelController.");
            return;
        }

        EnsureGridParent();
        CreateGridBuilder();

        LevelData data = levelDatas[levelIndex];
        Debug.Log($"Activating Level Data {levelIndex + 1} with {data.cells.Count} cells.");

        gridBuilder.Build(data);
        ResetLevelState();

        currentLevel = levelIndex;

        if (levelText != null)
        {
            levelText.gameObject.SetActive(true);
            UpdateLevelText();
        }

        ScoreManager.Instance?.InitLevel();
        LevelManager.Instance?.SyncLevel(currentLevel);
    }

    public void OnLevelCompleted()
    {
        if (levelCompletePanel == null || panelAnimator == null)
        {
            Debug.LogWarning("LevelCompletePanel or Animator is not assigned.");
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
                levelText.gameObject.SetActive(false);
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
        if (currentLevel < levelDatas.Length)
        {
            ActivateLevel(currentLevel);

            int levelToUnlock = currentLevel + 1;
            levelAccessUseCase ??= new LevelAccessUseCase(new PlayerPrefsLevelUnlockService());
            levelAccessUseCase.CompleteLevelAndUnlockNext(levelToUnlock - 1);

            if (LevelManagerMenu.Instance != null)
                LevelManagerMenu.Instance.UpdateLockState(currentLevel, false);
            else
                Debug.LogWarning($"LevelManagerMenu.Instance does not exist while unlocking Level {levelToUnlock}.");

            LevelManager.Instance?.SyncLevel(currentLevel);
        }
        else
        {
            Debug.Log("All levels completed.");
            currentLevel = levelDatas.Length - 1;
            if (levelText != null)
            {
                levelText.gameObject.SetActive(true);
                UpdateLevelText();
            }
        }
    }

    public bool TryOpenLevel(int level1Based)
    {
        int levelIndex = level1Based - 1;
        if (levelIndex < 0 || !HasLevels || levelIndex >= levelDatas.Length)
        {
            Debug.LogError($"Level index {level1Based} is out of range.");
            return false;
        }

        currentLevel = levelIndex;
        ActivateLevel(levelIndex);
        SwitchToGameplay();

        PlayerPrefs.SetInt("SelectedLevel", level1Based);
        PlayerPrefs.Save();
        return true;
    }

    public void SwitchToMenu()
    {
        if (canvasGameplay != null) canvasGameplay.SetActive(false);
        if (canvasMenuGame != null) canvasMenuGame.SetActive(true);
    }

    public void SwitchToGameplay()
    {
        if (canvasMenuGame != null) canvasMenuGame.SetActive(false);
        if (canvasGameplay != null) canvasGameplay.SetActive(true);
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

    public void CheckGameOver(bool isMerged)
    {
        var activeCells = gridBuilder?.ActiveCells;
        bool allOccupied = activeCells != null && activeCells.Count > 0 && activeCells.All(cell => cell.IsOccupied);
        int currentScore = ScoreManager.Instance?.CurrentScore ?? 0;
        int targetScore = ScoreManager.Instance?.GetTargetScore() ?? 0;

        Debug.Log($"CheckGameOver - AllOccupied: {allOccupied}, IsMerged: {isMerged}, Score: {currentScore}/{targetScore}");
        Debug.Log($"Total GridCells: {activeCells?.Count ?? 0}, Occupied: {activeCells?.Count(cell => cell.IsOccupied) ?? 0}");

        if (allOccupied && !isMerged && currentScore < targetScore)
        {
            if (LevelManager.Instance != null)
                LevelManager.Instance.OnLevelFailed();
            else
                Debug.LogError("LevelManager.Instance is not initialized.");
        }
    }

    private void ClearCurrentGrid()
    {
        gridBuilder?.Clear();
    }

    private void ResetLevelState()
    {
        gridBuilder?.ClearOccupiedStacks();
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

    private void UpdateLevelText()
    {
        if (levelText != null)
            levelText.text = "Level: " + (currentLevel + 1);
    }

    private void EnsureGridParent()
    {
        if (gridParent != null)
            return;

        GameObject gp = new GameObject("GridParent");
        gp.transform.SetParent(transform);
        gridParent = gp.transform;
    }

    private void CreateGridBuilder()
    {
        if (gridBuilder != null || gridCellPrefab == null || gridParent == null)
            return;

        GridCellPool gridCellPool = new GridCellPool(gridCellPrefab);
        MapAutoScaler mapAutoScaler = new MapAutoScaler(maxMapWidth, maxMapHeight);
        gridBuilder = new LevelGridBuilder(gridCellPool, gridParent, mapAutoScaler);
    }

    private bool HasLevels => levelDatas != null && levelDatas.Length > 0;
}
