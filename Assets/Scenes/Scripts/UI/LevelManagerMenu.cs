using UnityEngine;
using UnityEngine.UI;

public class LevelManagerMenu : MonoBehaviour
{
    [System.Serializable]
    public class LevelButtonData
    {
        public Button levelButton;  // Nút button cho level
        public Image lockImage;     // Ảnh khóa riêng biệt (GameObject con trong Button)
    }

    [Header("Level Buttons and Locks")]
    [SerializeField] private LevelButtonData[] levelButtonsData; // Danh sách cặp button và ảnh khóa

    private bool isLoading = false; // Biến kiểm soát để tránh tải scene nhiều lần

    public static LevelManagerMenu Instance { get; private set; }

    private void Awake()
    {
        // Reset trạng thái khi play trong Editor để test, nhưng giữ nguyên khi build
#if UNITY_EDITOR
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("LevelUnlocked_1", 1); // Mặc định mở Level 1
        for (int i = 2; i <= levelButtonsData.Length; i++)
        {
            PlayerPrefs.SetInt($"LevelUnlocked_{i}", 0); // Khóa các level từ 2 trở lên
        }
        PlayerPrefs.Save();
#endif

        // Gán Instance
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // In trạng thái PlayerPrefs để kiểm tra
        for (int i = 1; i <= levelButtonsData.Length; i++)
        {
            Debug.Log($"LevelUnlocked_{i}: {PlayerPrefs.GetInt($"LevelUnlocked_{i}", i == 1 ? 1 : 0)}");
        }

        // Gán sự kiện onClick và kiểm tra trạng thái khóa cho từng nút level
        for (int i = 0; i < levelButtonsData.Length; i++)
        {
            int levelIndex = i + 1; // Level 1-based
            if (levelButtonsData[i].levelButton != null)
            {
                int buttonIndex = i; // Tạo bản sao để tránh vấn đề closure
                levelButtonsData[buttonIndex].levelButton.onClick.AddListener(() => LoadLevel(buttonIndex + 1));
                Debug.Log($"Button Level {levelIndex} được gán sự kiện, interactable: {levelButtonsData[buttonIndex].levelButton.interactable}");

                // Kiểm tra và áp dụng trạng thái khóa
                bool isUnlocked = PlayerPrefs.GetInt($"LevelUnlocked_{levelIndex}", levelIndex == 1 ? 1 : 0) == 1;
                UpdateLockState(buttonIndex, !isUnlocked);
            }
            else
            {
                Debug.LogWarning($"⚠️ Button cho Level {i + 1} chưa được gán trong Inspector!");
            }
        }
    }

    private void LoadLevel(int levelIndex)
    {
        if (isLoading)
        {
            Debug.LogWarning("⏳ Đang tải, không thể chuyển level!");
            return;
        }

        // Kiểm tra xem level có được mở khóa không
        bool isUnlocked = PlayerPrefs.GetInt($"LevelUnlocked_{levelIndex}", levelIndex == 1 ? 1 : 0) == 1;
        Debug.Log($"Level {levelIndex} isUnlocked: {isUnlocked}");
        if (!isUnlocked)
        {
            Debug.LogWarning($"⚠️ Level {levelIndex} vẫn bị khóa!");
            return;
        }

        isLoading = true;
        Debug.Log($"Attempting to load Level {levelIndex}");

        if (LevelController.Instance != null)
        {
            // Kiểm tra xem levelIndex có hợp lệ và chỉ tải nếu đã mở khóa
            if (levelIndex - 1 >= 0 && levelIndex - 1 < LevelController.Instance.levels.Length)
            {
                LevelController.Instance.currentLevel = levelIndex - 1; // Chuyển từ 1-based sang 0-based
                LevelController.Instance.ActivateLevel(levelIndex - 1);
                LevelController.Instance.SwitchToGameplay();
                Debug.Log($"✅ Đã chọn và kích hoạt Level {levelIndex} từ menu");
                PlayerPrefs.SetInt("SelectedLevel", levelIndex); // Lưu level được chọn
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogError($"⚠️ Level index {levelIndex} ngoài phạm vi mảng levels!");
            }
        }
        else
        {
            Debug.LogError("⚠️ LevelController.Instance không tồn tại!");
        }

        isLoading = false;
    }

    public void UpdateLockState(int buttonIndex, bool isLocked)
    {
        if (buttonIndex >= 0 && buttonIndex < levelButtonsData.Length)
        {
            if (levelButtonsData[buttonIndex].levelButton != null && levelButtonsData[buttonIndex].lockImage != null)
            {
                levelButtonsData[buttonIndex].lockImage.enabled = isLocked; // Ẩn/hiện ảnh khóa riêng biệt
                levelButtonsData[buttonIndex].levelButton.interactable = !isLocked; // Vô hiệu hóa button nếu khóa
                Debug.Log($"🔒 Cập nhật trạng thái Level {buttonIndex + 1}: {(isLocked ? "Đã khóa" : "Đã mở")}, Interactable: {levelButtonsData[buttonIndex].levelButton.interactable}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Lock Image hoặc Button cho Level {buttonIndex + 1} chưa được gán trong Inspector!");
            }
        }
        else
        {
            Debug.LogError($"⚠️ ButtonIndex {buttonIndex} ngoài phạm vi mảng levelButtonsData!");
        }
    }
}