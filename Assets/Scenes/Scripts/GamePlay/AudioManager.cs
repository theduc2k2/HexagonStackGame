using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Background Music")]
    [SerializeField] private AudioClip[] bgMusicClips; // Mảng nhạc nền cho từng level (index 0 cho Level 1, index 1 cho Level 2, ...)
    [SerializeField] private AudioSource bgMusicSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip coinSound; // Âm thanh khi cộng tiền
    [SerializeField] private AudioClip clickLevelSound; // Âm thanh khi click vào level
    [SerializeField] private AudioClip dragHexagonSound; // Âm thanh khi kéo khối hexagon
    [SerializeField] private AudioClip mergeHexagonSound; // Âm thanh khi các khối hợp nhất
    [SerializeField] private AudioSource sfxSource; // AudioSource cho hiệu ứng âm thanh

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ AudioManager tồn tại qua các scene
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Khởi tạo AudioSource nếu chưa có
        if (bgMusicSource == null)
        {
            bgMusicSource = gameObject.AddComponent<AudioSource>();
            bgMusicSource.loop = true;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        // Phát nhạc nền cho Level 1 ban đầu
        PlayBackgroundMusic(0);
    }

    // Phát nhạc nền cho level hiện tại
    public void PlayBackgroundMusic(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < bgMusicClips.Length && bgMusicClips[levelIndex] != null)
        {
            bgMusicSource.clip = bgMusicClips[levelIndex];
            bgMusicSource.Play();
            Debug.Log($"🎵 Đang phát nhạc nền cho Level {levelIndex + 1}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Không tìm thấy nhạc nền cho Level {levelIndex + 1}!");
        }
    }

    // Phát hiệu ứng âm thanh cho cộng tiền
    public void PlayCoinSound()
    {
        if (coinSound != null)
        {
            sfxSource.PlayOneShot(coinSound);
            Debug.Log("💰 Phát âm thanh cộng tiền");
        }
    }

    // Phát hiệu ứng âm thanh khi click vào level
    public void PlayClickLevelSound()
    {
        if (clickLevelSound != null)
        {
            sfxSource.PlayOneShot(clickLevelSound);
            Debug.Log("👆 Phát âm thanh click level");
        }
    }

    // Phát hiệu ứng âm thanh khi kéo khối hexagon
    public void PlayDragHexagonSound()
    {
        if (dragHexagonSound != null)
        {
            sfxSource.PlayOneShot(dragHexagonSound);
            Debug.Log("🔄 Phát âm thanh kéo khối hexagon");
        }
    }

    // Phát hiệu ứng âm thanh khi các khối hợp nhất
    public void PlayMergeHexagonSound()
    {
        if (mergeHexagonSound != null)
        {
            sfxSource.PlayOneShot(mergeHexagonSound);
            Debug.Log("🔗 Phát âm thanh hợp nhất khối hexagon");
        }
    }

    // Tắt/mở nhạc nền
    public void ToggleBackgroundMusic(bool isOn)
    {
        bgMusicSource.mute = !isOn;
        Debug.Log($"🎶 Nhạc nền {(isOn ? "bật" : "tắt")}");
    }

    // Tắt/mở hiệu ứng âm thanh
    public void ToggleSoundEffects(bool isOn)
    {
        sfxSource.mute = !isOn;
        Debug.Log($"🔊 Hiệu ứng âm thanh {(isOn ? "bật" : "tắt")}");
    }
}