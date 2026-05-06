using Project.Application.Interfaces;
using Project.Core.Domain.Stack;
using UnityEngine;

public sealed class HexStackFactory
{
    private const float HexHeightStep = 0.2f;
    private const int AbsoluteMinHexCount = 1;
    private const int AbsoluteMaxHexCount = 50;

    private readonly Hexagon hexagonPrefab;
    private readonly HexStack hexStackPrefab;
    private readonly IColorPairProvider colorPairProvider;

    public HexStackFactory(
        Hexagon hexagonPrefab,
        HexStack hexStackPrefab,
        IColorPairProvider colorPairProvider)
    {
        this.hexagonPrefab = hexagonPrefab;
        this.hexStackPrefab = hexStackPrefab;
        this.colorPairProvider = colorPairProvider;
    }

    public bool TryCreate(Transform slot, Color[] colors, StackConfig countRange, out HexStack hexStack)
    {
        hexStack = null;

        if (slot == null)
            return false;

        if (hexagonPrefab == null || hexStackPrefab == null)
        {
            Debug.LogError("Stack factory missing prefab reference.");
            return false;
        }

        if (colors == null || colors.Length < 2)
        {
            Debug.LogError("Need at least 2 colors to create a stack.");
            return false;
        }

        if (!colorPairProvider.TryGetPair(colors, out Color firstColor, out Color secondColor))
        {
            Debug.LogError("Failed to select random color pair.");
            return false;
        }

        StackConfig normalizedRange = countRange.Normalized(AbsoluteMinHexCount, AbsoluteMaxHexCount);
        int amount = Random.Range(normalizedRange.MinCount, normalizedRange.MaxCount + 1);
        int firstColorHexagonCount = Random.Range(0, amount);

        if (HexagonPool.Instance == null)
        {
            Debug.LogError("HexagonPool.Instance is not initialized.");
            return false;
        }

        hexStack = Object.Instantiate(hexStackPrefab, slot.position, Quaternion.identity, slot);
        hexStack.name = $"Stack{slot.GetSiblingIndex()}";

        for (int i = 0; i < amount; i++)
        {
            Vector3 worldPos = slot.position + Vector3.up * i * HexHeightStep;
            Hexagon hexagon = HexagonPool.Instance.GetHexagon(worldPos, hexagonPrefab.transform.rotation, hexStack.transform);
            if (hexagon == null)
                continue;

            hexagon.color = i < firstColorHexagonCount ? firstColor : secondColor;
            hexStack.Add(hexagon);
        }

        return true;
    }
}
