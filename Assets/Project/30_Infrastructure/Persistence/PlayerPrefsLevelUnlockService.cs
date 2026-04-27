using UnityEngine;
using Project.Application.Interfaces;

namespace Project.Infrastructure.Persistence
{
    public sealed class PlayerPrefsLevelUnlockService : ILevelUnlockService
    {
        public bool IsUnlocked(int level1Based)
        {
            int defaultValue = level1Based == 1 ? 1 : 0;
            return PlayerPrefs.GetInt($"LevelUnlocked_{level1Based}", defaultValue) == 1;
        }

        public void Unlock(int level1Based)
        {
            if (level1Based <= 0)
                return;

            PlayerPrefs.SetInt($"LevelUnlocked_{level1Based}", 1);
            PlayerPrefs.Save();
        }

        public void ResetToFirstLevel(int maxLevel)
        {
            PlayerPrefs.SetInt("LevelUnlocked_1", 1);
            for (int i = 2; i <= maxLevel; i++)
                PlayerPrefs.SetInt($"LevelUnlocked_{i}", 0);
            PlayerPrefs.Save();
        }
    }
}
