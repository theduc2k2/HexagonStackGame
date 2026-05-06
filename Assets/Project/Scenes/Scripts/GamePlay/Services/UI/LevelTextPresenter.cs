using TMPro;
using UnityEngine.UI;

public sealed class LevelTextPresenter
{
    private readonly TextMeshProUGUI tmpText;
    private readonly Text unityText;

    public LevelTextPresenter(TextMeshProUGUI tmpText, Text unityText)
    {
        this.tmpText = tmpText;
        this.unityText = unityText;
    }

    public bool HasAnyText => tmpText != null || unityText != null;

    public void SetVisible(bool visible)
    {
        if (tmpText != null)
            tmpText.gameObject.SetActive(visible);

        if (unityText != null)
            unityText.gameObject.SetActive(visible);
    }

    public void SetLevel(int zeroBasedLevel)
    {
        string levelValue = "Level: " + (zeroBasedLevel + 1);

        if (tmpText != null)
            tmpText.text = levelValue;

        if (unityText != null)
            unityText.text = levelValue;
    }
}
