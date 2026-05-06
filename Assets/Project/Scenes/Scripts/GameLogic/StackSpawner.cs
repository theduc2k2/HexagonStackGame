using UnityEngine;
using NaughtyAttributes;
using Project.Application.Interfaces;
using Project.Core.Domain.Stack;
using Project.Infrastructure.UnityAdapters;

public class StackSpawner : MonoBehaviour
{
    private const int StacksPlacedBeforeRespawn = 3;

    [Header("Elements")]
    [SerializeField] private Hexagon hexagonPrefab;
    [SerializeField] private HexStack hexagonStackPrefab;
    [SerializeField] private Transform stackPositionParent;

    [Header("Settings")]
    [SerializeField] private Color[] colors;
    [MinMaxSlider(2, 8)]
    [SerializeField] private Vector2Int minMaxHexCount;
    [SerializeField] private float slotOccupiedRadius = 0.25f;

    private int placedStackCount;
    private IColorPairProvider colorPairProvider;
    private StackSpawnSlotQuery slotQuery;
    private HexStackFactory stackFactory;
    private StackSpawnAnimator stackSpawnAnimator;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        colorPairProvider = new RandomColorPairProvider();
        stackFactory = new HexStackFactory(hexagonPrefab, hexagonStackPrefab, colorPairProvider);
        stackSpawnAnimator = new StackSpawnAnimator();
        CreateSlotQuery();
        StackController.onStackPlaced += StackPlacedCallback;
    }

    private void OnDestroy()
    {
        StackController.onStackPlaced -= StackPlacedCallback;
    }

    private void Start()
    {
        CreateSlotQuery();
        slotQuery?.ValidateSpawnPoints();
        GenerateStacks();
    }

    private void StackPlacedCallback(GridCell gridCell)
    {
        placedStackCount++;
        if (placedStackCount < StacksPlacedBeforeRespawn)
            return;

        placedStackCount = 0;
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

        CreateSlotQuery();
        StackConfig stackConfig = new StackConfig(minMaxHexCount.x, minMaxHexCount.y);

        for (int i = 0; i < stackPositionParent.childCount; i++)
        {
            Transform slot = stackPositionParent.GetChild(i);
            if (!slotQuery.IsSlotAvailable(slot))
                continue;

            if (stackFactory.TryCreate(slot, colors, stackConfig, out HexStack stack))
                stackSpawnAnimator.PlaySpawn(stack, slot.GetSiblingIndex());
        }
    }

    public bool IsSpawnSlot(Transform target)
    {
        CreateSlotQuery();
        return slotQuery != null && slotQuery.IsSpawnSlot(target);
    }

    private void CreateSlotQuery()
    {
        if (stackPositionParent == null)
            return;

        slotQuery = new StackSpawnSlotQuery(stackPositionParent, slotOccupiedRadius);
    }
}
