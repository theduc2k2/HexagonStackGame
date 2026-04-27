using System;

namespace Project.Core.Common
{
    public static class GameEvent
    {
        public static event Action<string> Raised;

        public static void Raise(string eventName)
        {
            Raised?.Invoke(eventName);
        }
    }
}
