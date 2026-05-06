using UnityEngine;

public sealed class StackSpawnAnimator
{
    private const float SpawnDuration = 0.45f;
    private const float SpawnDelayStep = 0.1f;

    public void PlaySpawn(HexStack stack, int slotIndex)
    {
        if (stack == null)
            return;

        stack.transform.localScale = Vector3.zero;
        LeanTween.scale(stack.gameObject, Vector3.one, SpawnDuration)
            .setEase(LeanTweenType.easeOutBack)
            .setDelay(slotIndex * SpawnDelayStep);
    }
}
