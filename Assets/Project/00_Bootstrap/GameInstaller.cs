using UnityEngine;

namespace Project.Presentation
{
    public sealed class GameInstaller : MonoBehaviour
    {
        [Header("Migration Flags")]
        [SerializeField] private bool useNewLevelUnlockService = false;

        public bool UseNewLevelUnlockService => useNewLevelUnlockService;
    }
}
