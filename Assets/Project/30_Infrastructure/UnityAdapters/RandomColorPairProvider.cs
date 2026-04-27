using UnityEngine;
using Project.Application.Interfaces;

namespace Project.Infrastructure.UnityAdapters
{
    public sealed class RandomColorPairProvider : IColorPairProvider
    {
        public bool TryGetPair(Color[] source, out Color first, out Color second)
        {
            first = default;
            second = default;

            if (source == null || source.Length < 2)
                return false;

            int a = Random.Range(0, source.Length);
            int b;
            do
            {
                b = Random.Range(0, source.Length);
            } while (b == a);

            first = source[a];
            second = source[b];
            return true;
        }
    }
}
