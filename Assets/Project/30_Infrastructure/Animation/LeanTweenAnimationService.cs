using System;
using UnityEngine;
using Project.Application.Interfaces;

namespace Project.Infrastructure.Animation
{
    public sealed class LeanTweenAnimationService : IAnimationService
    {
        public void Cancel(GameObject target)
        {
            // Placeholder for migration phase. Hook LeanTween here after moving tween package
            // into an asmdef-compatible assembly.
        }

        public void MoveLocal(GameObject target, Vector3 localTarget, float duration, float delay = 0f, Action onComplete = null)
        {
            if (target == null) return;
            target.transform.localPosition = localTarget;
            onComplete?.Invoke();
        }

        public void ScaleToZeroAndDestroy(GameObject target, float duration, float delay = 0f, Action onComplete = null)
        {
            if (target == null) return;
            target.transform.localScale = Vector3.zero;
            onComplete?.Invoke();
            UnityEngine.Object.Destroy(target);
        }
    }
}
