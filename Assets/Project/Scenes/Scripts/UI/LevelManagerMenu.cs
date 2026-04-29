using UnityEngine;
using UnityEngine.UI;
using Project.Application.Interfaces;
using Project.Infrastructure.Persistence;

public class LevelManagerMenu : MonoBehaviour
{
    [System.Serializable]
    public class LevelButtonData
    {
        public Button levelButton;
        public Image lockImage;
    }

    [Header("Level Buttons and Locks")]
    [SerializeField] private LevelButtonData[] levelButtonsData;

    private bool isLoading;
    private ILevelUnlockService levelUnlockService;

    public static LevelManagerMenu Instance { get; private set; }

    private void Awake()
    {
        levelUnlockService = new PlayerPrefsLevelUnlockService();

#if UNITY_EDITOR
        levelUnlockService.ResetToFirstLevel(levelButtonsData.Length);
#endif

        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        for (int i = 1; i <= levelButtonsData.Length; i++)
            Debug.Log($"LevelUnlocked_{i}: {(levelUnlockService.IsUnlocked(i) ? 1 : 0)}");

        for (int i = 0; i < levelButtonsData.Length; i++)
        {
            int levelIndex = i + 1;
            if (levelButtonsData[i].levelButton == null)
            {
                Debug.LogWarning($"Button for level {levelIndex} is not assigned.");
                continue;
            }

            int buttonIndex = i;
            levelButtonsData[buttonIndex].levelButton.onClick.AddListener(() => LoadLevel(buttonIndex + 1));

            bool isUnlocked = levelUnlockService.IsUnlocked(levelIndex);
            UpdateLockState(buttonIndex, !isUnlocked);
        }
    }

    private void LoadLevel(int levelIndex)
    {
        if (isLoading)
        {
            Debug.LogWarning("Already loading level.");
            return;
        }

        bool isUnlocked = levelUnlockService.IsUnlocked(levelIndex);
        if (!isUnlocked)
        {
            Debug.LogWarning($"Level {levelIndex} is still locked.");
            return;
        }

        isLoading = true;

        if (LevelController.Instance == null)
        {
            Debug.LogError("LevelController.Instance is null.");
            isLoading = false;
            return;
        }

        if (levelIndex - 1 < 0 || levelIndex - 1 >= LevelController.Instance.levelDatas.Length)
        {
            Debug.LogError($"Level index {levelIndex} is out of range.");
            isLoading = false;
            return;
        }

        LevelController.Instance.currentLevel = levelIndex - 1;
        LevelController.Instance.ActivateLevel(levelIndex - 1);
        LevelController.Instance.SwitchToGameplay();

        PlayerPrefs.SetInt("SelectedLevel", levelIndex);
        PlayerPrefs.Save();

        isLoading = false;
    }

    public void UpdateLockState(int buttonIndex, bool isLocked)
    {
        if (buttonIndex < 0 || buttonIndex >= levelButtonsData.Length)
        {
            Debug.LogError($"Button index {buttonIndex} is out of range.");
            return;
        }

        if (levelButtonsData[buttonIndex].levelButton == null || levelButtonsData[buttonIndex].lockImage == null)
        {
            Debug.LogWarning($"Button or lock image for level {buttonIndex + 1} is not assigned.");
            return;
        }

        levelButtonsData[buttonIndex].lockImage.enabled = isLocked;
        levelButtonsData[buttonIndex].levelButton.interactable = !isLocked;
    }
}
