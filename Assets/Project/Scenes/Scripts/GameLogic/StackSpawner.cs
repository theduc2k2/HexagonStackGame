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
    [SerializeField] private float slotOccupiedRadius = 0.25f;

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
        ValidateSpawnPoints();
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

        if (stackPositionParent.childCount <= 0)
        {
            Debug.LogWarning("stackPositionParent has no child spawn points.");
            return;
        }

        for (int i = 0; i < stackPositionParent.childCount; i++)
        {
            Transform slot = stackPositionParent.GetChild(i);
            if (HasActiveStackInSlot(slot) || IsSlotOccupiedByWorldPosition(slot))
                continue;

            GenerateStack(slot);
        }
    }

    private static bool HasActiveStackInSlot(Transform slot)
    {
        if (slot == null)
            return false;

        for (int i = 0; i < slot.childCount; i++)
        {
            Transform child = slot.GetChild(i);
            if (child != null && child.GetComponent<HexStack>() != null)
                return true;
        }

        return false;
    }

    private bool IsSlotOccupiedByWorldPosition(Transform slot)
    {
        if (slot == null)
            return false;

        HexStack[] allStacks = FindObjectsOfType<HexStack>(true);
        float sqrRadius = slotOccupiedRadius * slotOccupiedRadius;
        Vector3 slotPos = slot.position;

        for (int i = 0; i < allStacks.Length; i++)
        {
            HexStack stack = allStacks[i];
            if (stack == null || !stack.gameObject.activeInHierarchy)
                continue;

            float sqrDist = (stack.transform.position - slotPos).sqrMagnitude;
            if (sqrDist <= sqrRadius)
                return true;
        }

        return false;
    }

    private void GenerateStack(Transform parent)
    {
        if (hexagonPrefab == null || hexagonStackPrefab == null)
        {
            Debug.LogError(
                $"StackSpawner missing prefab reference. hexagonPrefab={(hexagonPrefab == null ? "NULL" : hexagonPrefab.name)}, " +
                $"hexagonStackPrefab={(hexagonStackPrefab == null ? "NULL" : hexagonStackPrefab.name)}",
                this);
            return;
        }

        Color[] colorPair = GetRandomColors();
        if (colorPair == null || colorPair.Length < 2)
            return;

        int minHexCount = Mathf.Clamp(Mathf.Min(minMaxHexCount.x, minMaxHexCount.y), 1, 50);
        int maxHexCount = Mathf.Clamp(Mathf.Max(minMaxHexCount.x, minMaxHexCount.y), minHexCount, 50);
        int amount = Random.Range(minHexCount, maxHexCount + 1);
        if (amount <= 0)
        {
            Debug.LogWarning("Hex amount resolved to 0. Check minMaxHexCount.");
            return;
        }
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

    private void ValidateSpawnPoints()
    {
        if (stackPositionParent == null)
            return;

        float minSqrDistance = 0.01f;
        for (int i = 0; i < stackPositionParent.childCount; i++)
        {
            Vector3 a = stackPositionParent.GetChild(i).position;
            for (int j = i + 1; j < stackPositionParent.childCount; j++)
            {
                Vector3 b = stackPositionParent.GetChild(j).position;
                if ((a - b).sqrMagnitude <= minSqrDistance)
                {
                    Debug.LogWarning(
                        $"Spawn points {i} and {j} are too close. This can cause visual overlap.",
                        stackPositionParent);
                }
            }
        }
    }
}
