using UnityEngine;

namespace Project.Application.Interfaces
{
    public interface ILevelUnlockService
    {
        bool IsUnlocked(int level1Based);
        void Unlock(int level1Based);
        void ResetToFirstLevel(int maxLevel);
    }
}
