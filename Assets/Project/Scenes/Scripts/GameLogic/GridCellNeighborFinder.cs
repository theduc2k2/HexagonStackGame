using System.Collections.Generic;
using UnityEngine;

public sealed class GridCellNeighborFinder
{
    private readonly float searchRadius;

    public GridCellNeighborFinder(float searchRadius)
    {
        this.searchRadius = searchRadius;
    }

    public List<GridCell> FindOccupiedNeighbors(GridCell gridCell)
    {
        List<GridCell> neighbors = new List<GridCell>();
        if (gridCell == null)
            return neighbors;

        LayerMask gridCellMask = 1 << gridCell.gameObject.layer;
        float scaledRadius = searchRadius * gridCell.transform.lossyScale.x;
        Collider[] colliders = Physics.OverlapSphere(gridCell.transform.position, scaledRadius, gridCellMask);

        foreach (Collider collider in colliders)
        {
            GridCell neighbor = collider.GetComponent<GridCell>();
            if (neighbor == null || neighbor == gridCell || !neighbor.IsOccupied)
                continue;

            neighbors.Add(neighbor);
        }

        return neighbors;
    }
}
