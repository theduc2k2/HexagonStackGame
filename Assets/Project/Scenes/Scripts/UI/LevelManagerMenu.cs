using UnityEngine;
using UnityEngine.UI;
using Project.Application.Interfaces;
using Project.Application.UseCases;
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

#if UNITY_EDITOR
    [Header("Editor Testing")]
    [SerializeField] private bool resetUnlocksToFirstLevelOnAwake;
#endif

    private bool isLoading;
    private ILevelUnlockService levelUnlockService;
    private LevelAccessUseCase levelAccessUseCase;

    public static LevelManagerMenu Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        levelUnlockService = new PlayerPrefsLevelUnlockService();
        levelAccessUseCase = new LevelAccessUseCase(levelUnlockService);

#if UNITY_EDITOR
        if (resetUnlocksToFirstLevelOnAwake)
            levelUnlockService.ResetToFirstLevel(levelButtonsData.Length);
#endif
    }

    private void Start()
    {
        for (int i = 0; i < levelButtonsData.Length; i++)
        {
            int buttonIndex = i;
            int level1Based = i + 1;
            LevelButtonData buttonData = levelButtonsData[i];

            if (buttonData.levelButton == null)
            {
                Debug.LogWarning($"Button for level {level1Based} is not assigned.");
                continue;
            }

            buttonData.levelButton.onClick.AddListener(() => LoadLevel(buttonIndex + 1));
            UpdateLockState(buttonIndex, !levelAccessUseCase.CanOpenLevel(level1Based));
        }
    }

    private void LoadLevel(int level1Based)
    {
        if (isLoading)
        {
            Debug.LogWarning("Already loading level.");
            return;
        }

        if (!levelAccessUseCase.CanOpenLevel(level1Based))
        {
            Debug.LogWarning($"Level {level1Based} is still locked.");
            return;
        }

        if (LevelController.Instance == null)
        {
            Debug.LogError("LevelController.Instance is null.");
            return;
        }

        isLoading = true;
        bool opened = LevelController.Instance.TryOpenLevel(level1Based);
        isLoading = false;

        if (!opened)
            Debug.LogError($"Failed to open level {level1Based}.");
    }

    public void UpdateLockState(int buttonIndex, bool isLocked)
    {
        if (buttonIndex < 0 || buttonIndex >= levelButtonsData.Length)
        {
            Debug.LogError($"Button index {buttonIndex} is out of range.");
            return;
        }

        LevelButtonData buttonData = levelButtonsData[buttonIndex];
        if (buttonData.levelButton == null || buttonData.lockImage == null)
        {
            Debug.LogWarning($"Button or lock image for level {buttonIndex + 1} is not assigned.");
            return;
        }

        buttonData.lockImage.enabled = isLocked;
        buttonData.levelButton.interactable = !isLocked;
    }
}
