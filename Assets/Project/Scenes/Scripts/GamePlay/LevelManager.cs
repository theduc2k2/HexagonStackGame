using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("UI References - Level Complete")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private Animator panelAnimator;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Text unityLevelText;
    [SerializeField] private Button nextButton;

    [Header("UI References - Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Animator gameOverAnimator;
    [SerializeField] private Button retryButton;

    private bool isWaitingForPlayerAction;
    private LevelTextPresenter levelTextPresenter;
    private AnimatedPanelPresenter levelCompletePresenter;
    private AnimatedPanelPresenter gameOverPresenter;
    private LevelFlowButtonPresenter nextButtonPresenter;
    private LevelFlowButtonPresenter retryButtonPresenter;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Another LevelManager instance already exists. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        CreatePresenters();
        InitializeViews();
        BindButtons();
    }

    public void SyncLevel(int level)
    {
        levelTextPresenter?.SetLevel(level);
    }

    public void CompleteLevel()
    {
        if (isWaitingForPlayerAction || levelCompletePresenter == null || !levelCompletePresenter.IsReady)
            return;

        isWaitingForPlayerAction = true;
        levelCompletePresenter.ShowAndPlay();
        StartCoroutine(HandleLevelCompleteUI());
    }

    public void OnLevelFailed()
    {
        if (isWaitingForPlayerAction || gameOverPresenter == null || !gameOverPresenter.IsReady)
            return;

        isWaitingForPlayerAction = true;
        gameOverPresenter.ShowAndPlay();
        StartCoroutine(HandleLevelFailedUI());
    }

    public void RestartFromBeginning()
    {
        isWaitingForPlayerAction = false;
        levelCompletePresenter?.Hide();
        gameOverPresenter?.Hide();
        nextButtonPresenter?.SetVisible(false);
        retryButtonPresenter?.SetVisible(false);
        levelTextPresenter?.SetVisible(true);
        SyncLevel(0);
        LevelController.Instance?.InitFirstLevel();
    }

    private void CreatePresenters()
    {
        if (levelCompletePanel != null && panelAnimator == null)
            panelAnimator = levelCompletePanel.GetComponent<Animator>();

        if (gameOverPanel != null && gameOverAnimator == null)
            gameOverAnimator = gameOverPanel.GetComponent<Animator>();

        levelTextPresenter = new LevelTextPresenter(levelText, unityLevelText);
        levelCompletePresenter = new AnimatedPanelPresenter(levelCompletePanel, panelAnimator, "NextLevel", "IdleNextLevel");
        gameOverPresenter = new AnimatedPanelPresenter(gameOverPanel, gameOverAnimator, "GameOver", "GameOverIdle");
        nextButtonPresenter = new LevelFlowButtonPresenter(nextButton);
        retryButtonPresenter = new LevelFlowButtonPresenter(retryButton);
    }

    private void InitializeViews()
    {
        levelCompletePresenter.Hide();
        gameOverPresenter.Hide();
        nextButtonPresenter.SetVisible(false);
        retryButtonPresenter.SetVisible(false);

        if (levelTextPresenter.HasAnyText)
        {
            levelTextPresenter.SetVisible(true);
            SyncLevel(LevelController.Instance != null ? LevelController.Instance.currentLevel : 0);
        }
        else
        {
            Debug.LogWarning("No level text is assigned in LevelManager.");
        }

        if (!levelCompletePresenter.IsReady)
            Debug.LogWarning("Level complete panel or animator is not assigned in LevelManager.");

        if (!gameOverPresenter.IsReady)
            Debug.LogWarning("Game over panel or animator is not assigned in LevelManager.");

        if (!nextButtonPresenter.IsAssigned)
            Debug.LogWarning("nextButton is not assigned in LevelManager.");

        if (!retryButtonPresenter.IsAssigned)
            Debug.LogWarning("retryButton is not assigned in LevelManager.");
    }

    private void BindButtons()
    {
        nextButtonPresenter.Bind(OnNextLevelButtonClicked);
        retryButtonPresenter.Bind(OnRetryButtonClicked);
    }

    private IEnumerator HandleLevelCompleteUI()
    {
        yield return new WaitForSeconds(levelCompletePresenter.GetShowAnimationLength());
        levelCompletePresenter.SwitchToIdle();
        levelTextPresenter.SetVisible(false);
        nextButtonPresenter.SetVisible(true);
    }

    private IEnumerator HandleLevelFailedUI()
    {
        yield return new WaitForSeconds(gameOverPresenter.GetShowAnimationLength());
        gameOverPresenter.SwitchToIdle();
        retryButtonPresenter.SetVisible(true);
    }

    private void OnNextLevelButtonClicked()
    {
        if (!isWaitingForPlayerAction)
            return;

        isWaitingForPlayerAction = false;
        nextButtonPresenter.SetVisible(false);
        levelCompletePresenter.Hide();
        LevelController.Instance?.OnNextLevelButtonClicked();
        levelTextPresenter.SetVisible(true);
        SyncLevel(LevelController.Instance != null ? LevelController.Instance.currentLevel : 0);
    }

    private void OnRetryButtonClicked()
    {
        if (!isWaitingForPlayerAction)
            return;

        isWaitingForPlayerAction = false;
        retryButtonPresenter.SetVisible(false);
        gameOverPresenter.Hide();

        if (LevelController.Instance != null)
            LevelController.Instance.ActivateLevel(LevelController.Instance.currentLevel);
    }
}
