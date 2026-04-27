using UnityEngine;

public class GridTester : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private Grid grid;

    [Header("Settings")]
    [SerializeField] private Vector3Int gridPos;

    private void UpdateGridPos()
    {
        if (grid == null)
            return;

        transform.position = grid.CellToWorld(gridPos);
    }

    private void OnValidate()
    {
        UpdateGridPos();
    }
}
