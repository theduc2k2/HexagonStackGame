using UnityEngine;

namespace Project.Application.Interfaces
{
    public interface IColorPairProvider
    {
        bool TryGetPair(Color[] source, out Color first, out Color second);
    }
}
