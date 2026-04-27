using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScene : MonoBehaviour
{
    [SerializeField] private Button startButton; // Tham chiếu đến Button trong menu

    void Start()
    {
        // Gán sự kiện onClick cho Button
        if (startButton != null)
        {
            startButton.onClick.AddListener(LoadGameScene);
        }
        else
        {
            Debug.LogWarning("⚠️ startButton chưa được gán trong Inspector!");
        }
    }

    // Hàm được gọi khi bấm nút để tải Scene 1
    public void LoadGameScene()
    {
        SceneManager.LoadScene(1); // Tải scene có index 1 trong Build Settings
    }
}