using System.Linq;
using UnityEngine;

public sealed class ResourcesLevelDataProvider : ILevelDataProvider
{
    private readonly string resourcesPath;

    public ResourcesLevelDataProvider(string resourcesPath)
    {
        this.resourcesPath = resourcesPath;
    }

    public LevelData[] LoadLevels()
    {
        LevelData[] loadedLevels = Resources.LoadAll<LevelData>(resourcesPath);
        return loadedLevels == null
            ? new LevelData[0]
            : loadedLevels.OrderBy(level => level.levelID).ToArray();
    }
}
