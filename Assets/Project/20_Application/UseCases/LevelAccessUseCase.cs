using Project.Application.Interfaces;

namespace Project.Application.UseCases
{
    public sealed class LevelAccessUseCase
    {
        private readonly ILevelUnlockService _unlockService;

        public LevelAccessUseCase(ILevelUnlockService unlockService)
        {
            _unlockService = unlockService;
        }

        public bool CanOpenLevel(int level1Based)
        {
            return _unlockService.IsUnlocked(level1Based);
        }

        public void CompleteLevelAndUnlockNext(int completedLevel1Based)
        {
            _unlockService.Unlock(completedLevel1Based + 1);
        }
    }
}
