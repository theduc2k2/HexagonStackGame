using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance;

    [Header("Level Data")]
    public LevelData[] levelDatas;
    [SerializeField] private GridCell gridCellPrefab;

    [Header("Auto Scale Settings")]
    [SerializeField] private float maxMapWidth = 15f; // Chiều rộng tối đa an toàn trên màn hình dọc
    [SerializeField] private float maxMapHeight = 15f; // Chiều cao tối đa an toàn trên màn hình dọc
    
    [Header("Pooling")]
    public Transform gridParent;

    private List<GridCell> activeGridCells = new List<GridCell>();
    private Queue<GridCell> gridCellPool = new Queue<GridCell>();

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
            AutoLoadLevelData();
        }
        else
        {
            Debug.LogWarning("⚠️ Một instance khác của LevelController đã tồn tại, hủy instance này.");
            Destroy(gameObject);
            return;
        }
    }

    [ContextMenu("Auto Load Levels")]
    private void AutoLoadLevelData()
    {
        LevelData[] loadedLevels = Resources.LoadAll<LevelData>("Levels");
        if (loadedLevels != null && loadedLevels.Length > 0)
        {
            levelDatas = loadedLevels.OrderBy(l => l.levelID).ToArray();
            Debug.Log($"✅ Đã tự động load {levelDatas.Length} levels từ thư mục Resources/Levels.");
        }
        else
        {
            Debug.LogWarning("⚠️ Không tìm thấy file LevelData nào trong thư mục Resources/Levels!");
        }
    }

    private void Start()
    {
        int selectedLevel = PlayerPrefs.GetInt("SelectedLevel", 1) - 1;
        currentLevel = Mathf.Clamp(selectedLevel, 0, (levelDatas != null && levelDatas.Length > 0) ? levelDatas.Length - 1 : 0);
        
        // Tạo gridParent nếu chưa gán
        if (gridParent == null)
        {
            GameObject gp = new GameObject("GridParent");
            gp.transform.SetParent(this.transform);
            gridParent = gp.transform;
        }
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
        if (levelDatas == null || levelDatas.Length == 0)
        {
            Debug.LogError("⚠️ Mảng levelDatas chưa được gán data! Hãy kéo thả các file LevelData vào LevelController.");
            return;
        }

        if (levelIndex < 0 || levelIndex >= levelDatas.Length)
        if (gridCellPrefab == null)
        {
            Debug.LogError("⚠️ gridCellPrefab chưa được gán trong LevelController! Hãy kéo prefab GridCell vào đây.");
            return;
        }

        ClearCurrentGrid();

        LevelData data = levelDatas[levelIndex];
        Debug.Log($"Activating Level Data {levelIndex + 1} with {data.cells.Count} cells");

        Vector3 minBounds = new Vector3(float.MaxValue, 0, float.MaxValue);
        Vector3 maxBounds = new Vector3(float.MinValue, 0, float.MinValue);

        foreach (CellData cellData in data.cells)
        {
            GridCell cell = GetGridCellFromPool();
            cell.transform.SetParent(gridParent);
            cell.transform.localPosition = cellData.localPosition;
            cell.gameObject.SetActive(true);
            activeGridCells.Add(cell);

            // Tìm giới hạn của map để căn giữa
            if (cellData.localPosition.x < minBounds.x) minBounds.x = cellData.localPosition.x;
            if (cellData.localPosition.z < minBounds.z) minBounds.z = cellData.localPosition.z;
            if (cellData.localPosition.x > maxBounds.x) maxBounds.x = cellData.localPosition.x;
            if (cellData.localPosition.z > maxBounds.z) maxBounds.z = cellData.localPosition.z;
        }

        // Tự động scale và căn giữa map
        if (data.cells.Count > 0)
        {
            // Tính toán kích thước thật của Map (cộng thêm 1 chút margin do kích thước của 1 ô)
            float mapWidth = (maxBounds.x - minBounds.x) + 2f; 
            float mapHeight = (maxBounds.z - minBounds.z) + 2.5f;

            // Tính tỉ lệ thu nhỏ nếu map vượt quá giới hạn màn hình
            float scaleX = maxMapWidth / mapWidth;
            float scaleZ = maxMapHeight / mapHeight;
            float finalScale = Mathf.Min(1f, scaleX, scaleZ); // Chỉ thu nhỏ, không phóng to quá 1

            // Thu nhỏ Parent
            gridParent.localScale = new Vector3(finalScale, finalScale, finalScale);

            // Căn giữa sau khi đã scale
            Vector3 centerOffset = (minBounds + maxBounds) / 2f;
            gridParent.localPosition = -centerOffset * finalScale;
        }

        ResetLevelState();

        currentLevel = levelIndex;

        if (levelText != null)
        {
            levelText.gameObject.SetActive(true);
            UpdateLevelText();
            Debug.Log("✅ levelText cập nhật trong ActivateLevel: " + levelText.text);
        }

        ScoreManager.Instance?.InitLevel();

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SyncLevel(currentLevel);
            Debug.Log("✅ Đã đồng bộ currentLevel với LevelManager.");
        }
    }

    private GridCell GetGridCellFromPool()
    {
        if (gridCellPool.Count > 0)
        {
            return gridCellPool.Dequeue();
        }
        return Instantiate(gridCellPrefab);
    }

    private void ClearCurrentGrid()
    {
        foreach (GridCell cell in activeGridCells)
        {
            if (cell.IsOccupied)
            {
                cell.ClearHexStack();
            }
            cell.gameObject.SetActive(false);
            gridCellPool.Enqueue(cell);
        }
        activeGridCells.Clear();
    }

    private void ResetLevelState()
    {
        foreach (GridCell cell in activeGridCells)
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
        if (currentLevel < levelDatas.Length)
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
            currentLevel = levelDatas.Length - 1;
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
        bool allOccupied = activeGridCells.All(cell => cell.IsOccupied);
        int currentScore = ScoreManager.Instance?.CurrentScore ?? 0;
        int targetScore = ScoreManager.Instance?.GetTargetScore() ?? 0;

        Debug.Log($"🔍 CheckGameOver - AllOccupied: {allOccupied}, IsMerged: {isMerged}, Score: {currentScore}/{targetScore}");
        Debug.Log($"Total GridCells: {activeGridCells.Count}, Occupied: {activeGridCells.Count(cell => cell.IsOccupied)}");

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