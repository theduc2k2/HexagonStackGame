using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class MergeManager : MonoBehaviour
{
    [Header(" Elements ")]
    private List<GridCell> updatedCells = new List<GridCell>();
    private void Awake()
    {
        StackController.onStackPlaced += StackPlacedCallback;
    }
    private void Onestroy()
    {
        StackController.onStackPlaced -= StackPlacedCallback;
    }
    private void StackPlacedCallback(GridCell gridCell)
    {
        StartCoroutine(StackPlacedCoroutine(gridCell));

       

    }
    IEnumerator StackPlacedCoroutine(GridCell gridCell)
    {
        updatedCells.Add(gridCell);
        while(updatedCells.Count >0)
            yield return CheckForMerge(updatedCells[0]);
    }
    IEnumerator CheckForMerge(GridCell gridCell)
    {
        updatedCells.Remove(gridCell);
        if (!gridCell.IsOccupied)
            yield break;
        List<GridCell> neighborGridCells = GetNeighborGridCells(gridCell);

        if (neighborGridCells.Count <= 0)
        {
            Debug.Log("khong có ô nào xung quanh");
            yield break;
        }
        
        Color gridCellTopHexagonColor = gridCell.Stack.GetTopHexagonColor();


        Debug.Log(gridCellTopHexagonColor);
        List<GridCell> similarNeighborGridCells = GetSimilarNeighborGridCells(gridCellTopHexagonColor, neighborGridCells.ToArray());
        Debug.Log($"có {similarNeighborGridCells.Count} ô lân cận ");
        updatedCells.AddRange(similarNeighborGridCells);
        List<Hexagon> hexagonsToAdd = GetHexagonToAdd(gridCellTopHexagonColor, similarNeighborGridCells.ToArray());
        
        Debug.Log($"có {hexagonsToAdd.Count} hexagons được thêm vào");
        RemoveHexagonsFromStacks(hexagonsToAdd, similarNeighborGridCells.ToArray());
        MoveHexagons(gridCell, hexagonsToAdd);
        yield return StartCoroutine(MoveHexagons(gridCell, hexagonsToAdd));
        

        yield return new WaitForSeconds(0.2f); // chờ một tí sau khi di chuyển
        yield return CheckForCompleteStack(gridCell, gridCellTopHexagonColor);


    }

    private List<GridCell> GetNeighborGridCells(GridCell gridCell)
    {
        LayerMask gridCellMask = 1 << gridCell.gameObject.layer;
        List<GridCell> neighborGridCells = new List<GridCell>();
        // Assuming you have a reference to the current grid cell as 'CurrentGridCell'
        // Replace 'CurrentGridCell' with the actual reference if needed
        Collider[] neighborGridCellColliders = Physics.OverlapSphere(gridCell.transform.position, 2, gridCellMask);
        foreach (Collider gridCellCollider in neighborGridCellColliders)
        {
            GridCell neighborGridCell = gridCellCollider.GetComponent<GridCell>();
            if (!neighborGridCell.IsOccupied)
                continue;
            if (neighborGridCell == gridCell)
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
    float initialY = gridCell.Stack.Hexagons.Count * .2f;
    int completed = 0;

    if (hexagonsToAdd.Count == 0)
        yield break;

    for (int i = 0; i < hexagonsToAdd.Count; i++)
    {
        Hexagon hexagon = hexagonsToAdd[i];
        float targetY = initialY + i * .2f;
        Vector3 targetPosition = Vector3.up * targetY;
        gridCell.Stack.Add(hexagon);

        hexagon.MoveToLocal(targetPosition, () => {
            completed++;
        });
    }

    // Chờ cho đến khi tất cả MoveToLocal hoàn tất
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
        while (similarHexagons.Count > 0)
        {
            similarHexagons[0].SetParent(null);
            similarHexagons[0].Vanish(delay);
            //DestroyImmediate(similarHexagons[0].gameObject);
            delay += .05f;

            gridCell.Stack.Remove(similarHexagons[0]);

            similarHexagons.RemoveAt(0);

        }
        updatedCells.Add(gridCell);
        yield return new WaitForSeconds(.2f + (similarHexagonCount + 1) * .01f);

    }
}
