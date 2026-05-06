using System;
using UnityEngine;

public sealed class HexagonAnimationPlayer
{
    public void Vanish(Hexagon hexagon, float delay, Action onComplete)
    {
        if (hexagon == null)
            return;

        GameObject target = hexagon.gameObject;
        LeanTween.cancel(target);
        LeanTween.scale(target, Vector3.zero, 0.2f)
            .setEase(LeanTweenType.easeInBack)
            .setDelay(delay)
            .setOnComplete(onComplete);
    }

    public void VanishToScore(Hexagon hexagon, float delay, Vector3 scoreWorldPos, Action onComplete)
    {
        if (hexagon == null)
            return;

        GameObject target = hexagon.gameObject;
        LeanTween.cancel(target);
        hexagon.DisableCollider();

        Vector3 peakPos = hexagon.transform.position + Vector3.up * 2f;
        LeanTween.move(target, peakPos, 0.3f)
            .setEase(LeanTweenType.easeOutQuad)
            .setDelay(delay)
            .setOnComplete(() =>
            {
                LeanTween.move(target, scoreWorldPos, 0.5f)
                    .setEase(LeanTweenType.easeInCubic)
                    .setOnComplete(onComplete);

                LeanTween.scale(target, Vector3.one * 0.4f, 0.5f);
                LeanTween.rotateAround(target, Vector3.up, 360f, 0.5f);
            });
    }

    public void MoveToLocal(
        Hexagon hexagon,
        Vector3 targetLocalPos,
        Action onComplete = null,
        float delay = 0f,
        float duration = 0.34f)
    {
        if (hexagon == null)
            return;

        GameObject target = hexagon.gameObject;
        Transform transform = hexagon.transform;
        LeanTween.cancel(target);

        Vector3 startLocalPos = transform.localPosition;
        Quaternion startRotation = transform.localRotation;
        Vector3 moveDir = targetLocalPos - startLocalPos;
        moveDir.y = 0f;

        Vector3 flipAxis = moveDir.sqrMagnitude > 0.0001f
            ? Vector3.Cross(moveDir.normalized, Vector3.up).normalized
            : Vector3.right;

        Quaternion flipRotation = Quaternion.AngleAxis(360f, flipAxis) * startRotation;
        float flatDist = Vector3.Distance(
            new Vector3(startLocalPos.x, 0, startLocalPos.z),
            new Vector3(targetLocalPos.x, 0, targetLocalPos.z));
        float jumpHeight = Mathf.Clamp(0.2f + flatDist * 0.25f, 0.2f, 0.6f);

        LeanTween.value(target, 0f, 1f, duration)
            .setEase(LeanTweenType.easeOutQuad)
            .setDelay(delay)
            .setOnUpdate((float t) =>
            {
                Vector3 flatPos = Vector3.Lerp(startLocalPos, targetLocalPos, t);
                float arc = 4f * jumpHeight * t * (1f - t);
                transform.localPosition = new Vector3(flatPos.x, flatPos.y + arc, flatPos.z);
                transform.localRotation = Quaternion.Slerp(startRotation, flipRotation, t);
            })
            .setOnComplete(() =>
            {
                transform.localPosition = targetLocalPos;
                onComplete?.Invoke();
            });
    }
}
