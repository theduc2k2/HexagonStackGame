using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Win UI")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private Animator panelAnimator;
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject[] winPoupIcon;
    [SerializeField] private float iconAppearDelat = 1.0f;
    [SerializeField] private float iconAppearDuration = 0.5f;
    [SerializeField] private GameObject winFxRoot;
    [SerializeField] private ParticleSystem[] winFxParticles;

    [Header("Lose UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Animator gameOverAnimator;
    [SerializeField] private Button retryButton;

    [Header("Level Text")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Text unityLevelText;

    private bool isWaitingForPlayerAction;
    private Coroutine showWinIconsCoroutine;

    private readonly Dictionary<int, Vector3> winIconOriginalScales = new Dictionary<int, Vector3>();

    private LevelTextPresenter levelTextPresenter;
    private AnimatedPanelPresenter levelCompletePresenter;
    private AnimatedPanelPresenter gameOverPresenter;
    private LevelFlowButtonPresenter nextButtonPresenter;
    private LevelFlowButtonPresenter retryButtonPresenter;

    private RectTransform nextButtonRectTransform;
    private Animator nextButtonAnimator;
    private Vector2 nextButtonOriginalAnchoredPosition;
    private Vector2 nextButtonOriginalSizeDelta;
    private Vector3 nextButtonOriginalLocalScale;
    private bool hasCachedNextButtonLayout;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }

        Debug.LogWarning("Another LevelManager instance already exists. Destroying duplicate.");
        Destroy(gameObject);
    }

    private void Start()
    {
        BuildPresenters();
        CacheNextButtonLayout();
        CacheWinIconScales();
        BindButtons();
        InitializeViews();
    }

    public void SyncLevel(int level)
    {
        levelTextPresenter?.SetLevel(level);
    }

    public bool TryCompleteLevel()
    {
        if (isWaitingForPlayerAction || levelCompletePresenter == null || !levelCompletePresenter.IsReady)
            return false;

        isWaitingForPlayerAction = true;
        levelCompletePresenter.ShowAndPlay();
        StartCoroutine(HandleLevelCompleteUI());
        return true;
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
        StopWinSequence();
        SetNextButtonVisible(false);
        retryButtonPresenter?.SetVisible(false);
        levelTextPresenter?.SetVisible(true);
        SyncLevel(0);
        LevelController.Instance?.InitFirstLevel();
    }

    private void BuildPresenters()
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

    private void BindButtons()
    {
        nextButtonPresenter?.Bind(OnNextLevelButtonClicked);
        retryButtonPresenter?.Bind(OnRetryButtonClicked);
    }

    private void InitializeViews()
    {
        levelCompletePresenter?.Hide();
        gameOverPresenter?.Hide();
        StopWinSequence();
        SetNextButtonVisible(false);
        retryButtonPresenter?.SetVisible(false);

        if (levelTextPresenter != null && levelTextPresenter.HasAnyText)
        {
            levelTextPresenter.SetVisible(true);
            SyncLevel(LevelController.Instance != null ? LevelController.Instance.currentLevel : 0);
        }

        if (levelCompletePresenter == null || !levelCompletePresenter.IsReady)
            Debug.LogWarning("Level complete panel is not assigned in LevelManager.");

        if (gameOverPresenter == null || !gameOverPresenter.IsReady)
            Debug.LogWarning("Game over panel is not assigned in LevelManager.");
    }

    private IEnumerator HandleLevelCompleteUI()
    {
        ResetWinPopupIcons();
        yield return new WaitForSeconds(levelCompletePresenter.GetShowAnimationLength());

        levelCompletePresenter.SwitchToIdle();
        PlayWinFx();
        StartWinPopupIcons();

        levelTextPresenter?.SetVisible(false);
        SetNextButtonVisible(true);
    }

    private IEnumerator HandleLevelFailedUI()
    {
        yield return new WaitForSeconds(gameOverPresenter.GetShowAnimationLength());
        gameOverPresenter.SwitchToIdle();
        retryButtonPresenter?.SetVisible(true);
    }

    private void OnNextLevelButtonClicked()
    {
        if (!isWaitingForPlayerAction)
            return;

        isWaitingForPlayerAction = false;
        SetNextButtonVisible(false);
        levelCompletePresenter.Hide();
        StopWinSequence();

        LevelController.Instance?.OnNextLevelButtonClicked();
        levelTextPresenter?.SetVisible(true);
        SyncLevel(LevelController.Instance != null ? LevelController.Instance.currentLevel : 0);
    }

    private void OnRetryButtonClicked()
    {
        if (!isWaitingForPlayerAction)
            return;

        isWaitingForPlayerAction = false;
        retryButtonPresenter?.SetVisible(false);
        gameOverPresenter.Hide();

        if (LevelController.Instance != null)
            LevelController.Instance.ActivateLevel(LevelController.Instance.currentLevel);
    }

    private void SetNextButtonVisible(bool visible)
    {
        RestoreNextButtonLayout();
        if (nextButtonAnimator != null)
            nextButtonAnimator.enabled = false;
        nextButtonPresenter?.SetVisible(visible);
    }

    private void CacheNextButtonLayout()
    {
        if (nextButton == null)
            return;

        nextButtonRectTransform = nextButton.GetComponent<RectTransform>();
        nextButtonAnimator = nextButton.GetComponent<Animator>();
        if (nextButtonAnimator != null)
            nextButtonAnimator.enabled = false;

        if (nextButtonRectTransform == null)
            return;

        nextButtonOriginalAnchoredPosition = nextButtonRectTransform.anchoredPosition;
        nextButtonOriginalSizeDelta = nextButtonRectTransform.sizeDelta;
        nextButtonOriginalLocalScale = nextButtonRectTransform.localScale;
        hasCachedNextButtonLayout = true;
    }

    private void RestoreNextButtonLayout()
    {
        if (!hasCachedNextButtonLayout || nextButtonRectTransform == null)
            return;

        nextButtonRectTransform.anchoredPosition = nextButtonOriginalAnchoredPosition;
        nextButtonRectTransform.sizeDelta = nextButtonOriginalSizeDelta;
        nextButtonRectTransform.localScale = nextButtonOriginalLocalScale;
    }

    private void CacheWinIconScales()
    {
        if (winPoupIcon == null)
            return;

        foreach (GameObject icon in winPoupIcon)
        {
            if (icon == null)
                continue;

            int id = icon.GetInstanceID();
            if (!winIconOriginalScales.ContainsKey(id))
                winIconOriginalScales[id] = icon.transform.localScale;
        }
    }

    private void StartWinPopupIcons()
    {
        if (showWinIconsCoroutine != null)
            StopCoroutine(showWinIconsCoroutine);

        showWinIconsCoroutine = StartCoroutine(ShowWinPopupIcons());
    }

    private IEnumerator ShowWinPopupIcons()
    {
        if (winPoupIcon == null)
            yield break;

        foreach (GameObject icon in winPoupIcon)
        {
            if (icon == null)
                continue;

            icon.SetActive(true);

            CanvasGroup canvasGroup = icon.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = icon.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;

            int id = icon.GetInstanceID();
            Vector3 targetScale = winIconOriginalScales.TryGetValue(id, out Vector3 scale) ? scale : Vector3.one;

            LeanTween.alphaCanvas(canvasGroup, 1f, iconAppearDuration);
            LeanTween.scale(icon, targetScale, iconAppearDuration).setEase(LeanTweenType.easeOutBack);
            yield return new WaitForSeconds(iconAppearDelat);
        }
    }

    private void ResetWinPopupIcons()
    {
        if (winPoupIcon == null)
            return;

        foreach (GameObject icon in winPoupIcon)
        {
            if (icon == null)
                continue;

            LeanTween.cancel(icon);
            icon.SetActive(false);
            icon.transform.localScale = Vector3.zero;

            CanvasGroup canvasGroup = icon.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = icon.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
        }
    }

    private void PlayWinFx()
    {
        if (winFxRoot != null)
            winFxRoot.SetActive(true);

        if (winFxParticles == null)
            return;

        foreach (ParticleSystem particle in winFxParticles)
        {
            if (particle == null)
                continue;

            particle.Clear(true);
            particle.Play(true);
        }
    }

    private void StopWinFx()
    {
        if (winFxParticles != null)
        {
            foreach (ParticleSystem particle in winFxParticles)
            {
                if (particle == null)
                    continue;

                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        if (winFxRoot != null)
            winFxRoot.SetActive(false);
    }

    private void StopWinSequence()
    {
        if (showWinIconsCoroutine != null)
        {
            StopCoroutine(showWinIconsCoroutine);
            showWinIconsCoroutine = null;
        }

        ResetWinPopupIcons();
        StopWinFx();
    }
}
