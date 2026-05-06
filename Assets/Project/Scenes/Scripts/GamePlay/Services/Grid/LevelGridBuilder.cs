using System.Collections.Generic;
using UnityEngine;

public sealed class LevelGridBuilder
{
    private readonly GridCellPool pool;
    private readonly Transform parent;
    private readonly MapAutoScaler mapAutoScaler;
    private readonly List<GridCell> activeCells = new List<GridCell>();

    public IReadOnlyList<GridCell> ActiveCells => activeCells;

    public LevelGridBuilder(GridCellPool pool, Transform parent, MapAutoScaler mapAutoScaler)
    {
        this.pool = pool;
        this.parent = parent;
        this.mapAutoScaler = mapAutoScaler;
    }

    public void Build(LevelData levelData)
    {
        Clear();

        if (levelData == null)
            return;

        bool hasBounds = false;
        Bounds bounds = default;

        foreach (CellData cellData in levelData.cells)
        {
            GridCell cell = pool.Get();
            cell.transform.SetParent(parent);
            cell.transform.localPosition = cellData.localPosition;
            cell.gameObject.SetActive(true);
            activeCells.Add(cell);

            if (!hasBounds)
            {
                bounds = new Bounds(cellData.localPosition, Vector3.zero);
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(cellData.localPosition);
            }
        }

        if (hasBounds)
            mapAutoScaler.Apply(parent, bounds);
    }

    public void Clear()
    {
        foreach (GridCell cell in activeCells)
            pool.Release(cell);

        activeCells.Clear();
    }

    public void ClearOccupiedStacks()
    {
        foreach (GridCell cell in activeCells)
        {
            if (cell != null && cell.IsOccupied)
                cell.ClearHexStack();
        }
    }
}
