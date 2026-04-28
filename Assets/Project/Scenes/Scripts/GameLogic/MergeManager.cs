using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeManager : MonoBehaviour
{
    [Header("Elements")]
    private readonly List<GridCell> updatedCells = new List<GridCell>();

    [Header("Merge Animation")]
    [SerializeField] private float mergeLaunchInterval = 0.075f;
    [SerializeField] private float mergeFlipDuration = 0.34f;

    private Coroutine mergeCoroutine;

    public static bool IsMerging { get; private set; }

    private void Awake()
    {
        StackController.onStackPlaced += StackPlacedCallback;
    }

    private void OnDestroy()
    {
        StackController.onStackPlaced -= StackPlacedCallback;
        IsMerging = false;
    }

    private void StackPlacedCallback(GridCell gridCell)
    {
        if (gridCell == null)
            return;

        updatedCells.Add(gridCell);

        if (mergeCoroutine == null)
            mergeCoroutine = StartCoroutine(ProcessMergeQueue());
    }

    private IEnumerator ProcessMergeQueue()
    {
        IsMerging = true;

        while (updatedCells.Count > 0)
            yield return CheckForMerge(updatedCells[0]);

        IsMerging = false;
        mergeCoroutine = null;
    }

    private IEnumerator CheckForMerge(GridCell gridCell)
    {
        updatedCells.Remove(gridCell);
        if (!gridCell.IsOccupied)
            yield break;

        List<GridCell> neighborGridCells = GetNeighborGridCells(gridCell);
        if (neighborGridCells.Count <= 0)
            yield break;

        Color gridCellTopHexagonColor = gridCell.Stack.GetTopHexagonColor();
        List<GridCell> similarNeighborGridCells = GetSimilarNeighborGridCells(gridCellTopHexagonColor, neighborGridCells.ToArray());
        updatedCells.AddRange(similarNeighborGridCells);

        List<Hexagon> hexagonsToAdd = GetHexagonToAdd(gridCellTopHexagonColor, similarNeighborGridCells.ToArray());
        RemoveHexagonsFromStacks(hexagonsToAdd, similarNeighborGridCells.ToArray());
        yield return StartCoroutine(MoveHexagons(gridCell, hexagonsToAdd));

        yield return new WaitForSeconds(0.2f);
        yield return CheckForCompleteStack(gridCell, gridCellTopHexagonColor);
    }

    private List<GridCell> GetNeighborGridCells(GridCell gridCell)
    {
        LayerMask gridCellMask = 1 << gridCell.gameObject.layer;
        List<GridCell> neighborGridCells = new List<GridCell>();

        Collider[] neighborGridCellColliders = Physics.OverlapSphere(gridCell.transform.position, 2f, gridCellMask);
        foreach (Collider gridCellCollider in neighborGridCellColliders)
        {
            GridCell neighborGridCell = gridCellCollider.GetComponent<GridCell>();
            if (neighborGridCell == null || !neighborGridCell.IsOccupied || neighborGridCell == gridCell)
                continue;

            neighborGridCells.Add(neighborGridCell);
        }

        return neighborGridCells;
    }

    private List<GridCell> GetSimilarNeighborGridCells(Color gridCellTopHexagonColor, GridCell[] neighborGridCells)
    {
        List<GridCell> similarNeighborGridCells = new List<GridCell>();

        foreach (GridCell neighborGridCell in neighborGridCells)
        {
            Color neighborGridCellTopHexagonColor = neighborGridCell.Stack.GetTopHexagonColor();
            if (gridCellTopHexagonColor == neighborGridCellTopHexagonColor)
                similarNeighborGridCells.Add(neighborGridCell);
        }

        return similarNeighborGridCells;
    }

    private List<Hexagon> GetHexagonToAdd(Color gridCellTopHexagonColor, GridCell[] neighborGridCells)
    {
        List<Hexagon> hexagonsToAdd = new List<Hexagon>();

        foreach (GridCell neighborCell in neighborGridCells)
        {
            HexStack neighborHexStack = neighborCell.Stack;
            for (int i = neighborHexStack.Hexagons.Count - 1; i >= 0; i--)
            {
                Hexagon hexagon = neighborHexStack.Hexagons[i];
                if (hexagon.color != gridCellTopHexagonColor)
                    break;

                hexagonsToAdd.Add(hexagon);
                hexagon.SetParent(null);
            }
        }

        return hexagonsToAdd;
    }

    private void RemoveHexagonsFromStacks(List<Hexagon> hexagonsToAdd, GridCell[] similarNeighborGridCells)
    {
        foreach (GridCell neighborCell in similarNeighborGridCells)
        {
            HexStack stack = neighborCell.Stack;
            foreach (Hexagon hexagon in hexagonsToAdd)
            {
                if (stack.Contains(hexagon))
                    stack.Remove(hexagon);
            }
        }
    }

    private IEnumerator MoveHexagons(GridCell gridCell, List<Hexagon> hexagonsToAdd)
    {
        if (hexagonsToAdd.Count == 0)
            yield break;

        float initialY = gridCell.Stack.Hexagons.Count * 0.2f;
        int completed = 0;

        for (int i = 0; i < hexagonsToAdd.Count; i++)
        {
            Hexagon hexagon = hexagonsToAdd[i];
            float targetY = initialY + i * 0.2f;
            Vector3 targetPosition = Vector3.up * targetY;

            gridCell.Stack.Add(hexagon);
            float launchDelay = i * mergeLaunchInterval;
            hexagon.MoveToLocal(targetPosition, () => completed++, launchDelay, mergeFlipDuration);
        }

        while (completed < hexagonsToAdd.Count)
            yield return null;
    }

    private IEnumerator CheckForCompleteStack(GridCell gridCell, Color topColor)
    {
        if (gridCell.Stack.Hexagons.Count < 10)
            yield break;

        List<Hexagon> similarHexagons = new List<Hexagon>(0);
        for (int i = gridCell.Stack.Hexagons.Count - 1; i >= 0; i--)
        {
            Hexagon hexagon = gridCell.Stack.Hexagons[i];
            if (hexagon.color != topColor)
                break;

            similarHexagons.Add(hexagon);
        }

        int similarHexagonCount = similarHexagons.Count;
        if (similarHexagons.Count < 10)
            yield break;

        ScoreManager.Instance?.AddPoints(similarHexagonCount * 10);

        float delay = 0f;
        Vector3 scorePos = ScoreManager.Instance != null ? ScoreManager.Instance.GetScoreWorldPosition() : Vector3.up * 10f;

        while (similarHexagons.Count > 0)
        {
            similarHexagons[0].SetParent(null);
            similarHexagons[0].VanishToScore(delay, scorePos);
            delay += 0.05f;

            gridCell.Stack.Remove(similarHexagons[0]);
            similarHexagons.RemoveAt(0);
        }

        updatedCells.Add(gridCell);
        yield return new WaitForSeconds(0.2f + (similarHexagonCount + 1) * 0.01f);
    }
}
