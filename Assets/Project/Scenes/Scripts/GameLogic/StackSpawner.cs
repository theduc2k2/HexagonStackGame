using UnityEngine;
using NaughtyAttributes;
using Project.Application.Interfaces;
using Project.Infrastructure.UnityAdapters;

public class StackSpawner : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private Hexagon hexagonPrefab;
    [SerializeField] private HexStack hexagonStackPrefab;
    [SerializeField] private Transform stackPositionParent;

    [Header("Settings")]
    [SerializeField] private Color[] colors;
    [MinMaxSlider(2, 8)]
    [SerializeField] private Vector2Int minMaxHexCount;

    private int stackCounter;
    private IColorPairProvider colorPairProvider;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        colorPairProvider = new RandomColorPairProvider();
        StackController.onStackPlaced += StackPlacedCallback;
    }

    private void OnDestroy()
    {
        StackController.onStackPlaced -= StackPlacedCallback;
    }

    private void Start()
    {
        GenerateStacks();
    }

    private void StackPlacedCallback(GridCell gridCell)
    {
        stackCounter++;
        if (stackCounter < 3)
            return;

        stackCounter = 0;
        GenerateStacks();
    }

    private void GenerateStacks()
    {
        if (stackPositionParent == null)
        {
            Debug.LogError("stackPositionParent is not assigned.");
            return;
        }

        for (int i = 0; i < stackPositionParent.childCount; i++)
            GenerateStack(stackPositionParent.GetChild(i));
    }

    private void GenerateStack(Transform parent)
    {
        if (hexagonPrefab == null || hexagonStackPrefab == null)
        {
            Debug.LogError("Hexagon prefab or HexStack prefab is not assigned.");
            return;
        }

        Color[] colorPair = GetRandomColors();
        if (colorPair == null || colorPair.Length < 2)
            return;

        int minHexCount = Mathf.Min(minMaxHexCount.x, minMaxHexCount.y);
        int maxHexCount = Mathf.Max(minMaxHexCount.x, minMaxHexCount.y);
        int amount = Random.Range(minHexCount, maxHexCount + 1);
        int firstColorHexagonCount = Random.Range(0, amount);

        HexStack hexStack = Instantiate(hexagonStackPrefab, parent.position, Quaternion.identity, parent);
        hexStack.name = $"Stack{parent.GetSiblingIndex()}";

        for (int i = 0; i < amount; i++)
        {
            Vector3 localPos = Vector3.up * i * 0.2f;
            Vector3 worldPos = hexStack.transform.TransformPoint(localPos);
            Hexagon hexagonInstance = Instantiate(hexagonPrefab, worldPos, hexagonPrefab.transform.rotation, hexStack.transform);

            hexagonInstance.color = i < firstColorHexagonCount ? colorPair[0] : colorPair[1];
            hexagonInstance.Configure(hexStack);
            hexStack.Add(hexagonInstance);
        }
    }

    private Color[] GetRandomColors()
    {
        if (colors == null || colors.Length < 2)
        {
            Debug.LogError("Need at least 2 colors in StackSpawner.");
            return null;
        }

        if (!colorPairProvider.TryGetPair(colors, out Color firstColor, out Color secondColor))
        {
            Debug.LogError("Failed to select random color pair.");
            return null;
        }

        return new[] { firstColor, secondColor };
    }
}
