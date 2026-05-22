using UnityEngine;

public sealed class AnimatedPanelPresenter
{
    private readonly GameObject panel;
    private readonly Animator animator;
    private readonly string showTrigger;
    private readonly string idleTrigger;

    public AnimatedPanelPresenter(GameObject panel, Animator animator, string showTrigger, string idleTrigger)
    {
        this.panel = panel;
        this.animator = animator;
        this.showTrigger = showTrigger;
        this.idleTrigger = idleTrigger;
    }

    public bool IsReady => panel != null;

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public void ShowAndPlay()
    {
        if (!IsReady)
            return;

        panel.SetActive(true);
        if (animator == null)
            return;

        animator.ResetTrigger(idleTrigger);
        animator.SetTrigger(showTrigger);
    }

    public void SwitchToIdle()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(showTrigger);
        animator.SetTrigger(idleTrigger);
    }

    public float GetShowAnimationLength(float fallback = 1f)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return fallback;

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip != null && clip.name == showTrigger)
                return clip.length;
        }

        return fallback;
    }
}
