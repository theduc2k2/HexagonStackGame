using System.Collections;
using System.Collections.Generic;
using Project.Core.Domain.Merge;
using UnityEngine;

public class MergeManager : MonoBehaviour
{
    private readonly List<GridCell> updatedCells = new List<GridCell>();

    [Header("Merge Animation")]
    [SerializeField] private float mergeLaunchInterval = 0.075f;
    [SerializeField] private float mergeFlipDuration = 0.34f;

    [Header("Merge Logic")]
    [SerializeField] private float mergeSearchRadius = 2f;
    [SerializeField] private int completeStackThreshold = 10;

    private Coroutine mergeCoroutine;
    private GridCellNeighborFinder neighborFinder;
    private MergeCandidateSelector candidateSelector;
    private MergeRule mergeRule;

    public static bool IsMerging { get; private set; }

    private void Awake()
    {
        neighborFinder = new GridCellNeighborFinder(mergeSearchRadius);
        candidateSelector = new MergeCandidateSelector();
        mergeRule = new MergeRule(completeStackThreshold);
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

        List<GridCell> neighborGridCells = neighborFinder.FindOccupiedNeighbors(gridCell);
        if (neighborGridCells.Count == 0)
            yield break;

        Color topColor = gridCell.Stack.GetTopHexagonColor();
        List<GridCell> similarNeighbors = candidateSelector.FindSimilarTopColorNeighbors(topColor, neighborGridCells);
        if (similarNeighbors.Count == 0)
            yield break;

        updatedCells.AddRange(similarNeighbors);

        List<Hexagon> hexagonsToAdd = candidateSelector.FindTopMatchingHexagons(topColor, similarNeighbors);
        RemoveHexagonsFromStacks(hexagonsToAdd, similarNeighbors);
        yield return StartCoroutine(MoveHexagons(gridCell, hexagonsToAdd));

        yield return new WaitForSeconds(0.2f);
        yield return CheckForCompleteStack(gridCell, topColor);
    }

    private static void RemoveHexagonsFromStacks(List<Hexagon> hexagonsToAdd, List<GridCell> sourceCells)
    {
        foreach (GridCell sourceCell in sourceCells)
        {
            HexStack stack = sourceCell.Stack;
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
        if (gridCell.Stack.Count < mergeRule.CompleteThreshold)
            yield break;

        List<Hexagon> similarHexagons = GetTopColorRun(gridCell.Stack, topColor);
        int similarHexagonCount = similarHexagons.Count;
        if (!mergeRule.IsComplete(similarHexagonCount))
            yield break;

        ScoreManager.Instance?.AddPoints(similarHexagonCount * 10);
        ScoreManager.Instance?.PlayScoreFillFxSequence(similarHexagonCount, 0.05f, 0.8f);

        float delay = 0f;
        Vector3 scorePos = ScoreManager.Instance != null ? ScoreManager.Instance.GetScoreWorldPosition() : Vector3.up * 10f;

        while (similarHexagons.Count > 0)
        {
            Hexagon hexagon = similarHexagons[0];
            hexagon.SetParent(null);
            hexagon.VanishToScore(delay, scorePos);
            delay += 0.05f;

            gridCell.Stack.Remove(hexagon);
            similarHexagons.RemoveAt(0);
        }

        updatedCells.Add(gridCell);
        yield return new WaitForSeconds(0.2f + (similarHexagonCount + 1) * 0.01f);
    }

    private static List<Hexagon> GetTopColorRun(HexStack stack, Color topColor)
    {
        List<Hexagon> similarHexagons = new List<Hexagon>();
        for (int i = stack.Hexagons.Count - 1; i >= 0; i--)
        {
            Hexagon hexagon = stack.Hexagons[i];
            if (hexagon.color != topColor)
                break;

            similarHexagons.Add(hexagon);
        }

        return similarHexagons;
    }
}
