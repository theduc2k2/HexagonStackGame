using System;
using UnityEngine.UI;

public sealed class LevelFlowButtonPresenter
{
    private readonly Button button;

    public LevelFlowButtonPresenter(Button button)
    {
        this.button = button;
    }

    public bool IsAssigned => button != null;

    public void Bind(Action action)
    {
        if (button == null || action == null)
            return;

        button.onClick.AddListener(() => action());
    }

    public void SetVisible(bool visible)
    {
        if (button != null)
            button.gameObject.SetActive(visible);
    }
}
