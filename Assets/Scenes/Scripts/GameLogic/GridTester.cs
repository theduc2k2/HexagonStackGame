using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class GridTester : MonoBehaviour
{
    [Header(" Elements ")]
    [SerializeField] private Grid grid;

    [Header(" Settings ")]
    [SerializeField] private Vector3Int gridPos;

    private void UpdateGridPos()
    {
        transform.position = grid.CellToWorld(gridPos);
    }
    private void OnValidate()
   {
    UpdateGridPos();
   }

}
