using System.Collections.Generic;
using UnityEngine;

public sealed class GridCellPool
{
    private readonly GridCell prefab;
    private readonly Queue<GridCell> pool = new Queue<GridCell>();

    public GridCellPool(GridCell prefab)
    {
        this.prefab = prefab;
    }

    public GridCell Get()
    {
        return pool.Count > 0 ? pool.Dequeue() : Object.Instantiate(prefab);
    }

    public void Release(GridCell cell)
    {
        if (cell == null)
            return;

        if (cell.IsOccupied)
            cell.ClearHexStack();

        cell.gameObject.SetActive(false);
        pool.Enqueue(cell);
    }
}
