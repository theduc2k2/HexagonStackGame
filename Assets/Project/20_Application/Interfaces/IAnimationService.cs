using System;
using UnityEngine;

namespace Project.Application.Interfaces
{
    public interface IAnimationService
    {
        void Cancel(GameObject target);
        void MoveLocal(GameObject target, Vector3 localTarget, float duration, float delay = 0f, Action onComplete = null);
        void ScaleToZeroAndDestroy(GameObject target, float duration, float delay = 0f, Action onComplete = null);
    }
}
