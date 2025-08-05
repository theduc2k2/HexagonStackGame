using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private GameObject hexagon; // Prefab lục giác
    [SerializeField] private Grid grid; // Grid component
    [SerializeField] private int gridSize; // Kích thước lưới
    [SerializeField] private bool generateGrid = false; // Toggle để kích hoạt sinh lưới thủ công

    // Gọi khi có thay đổi trong Inspector
    private void OnValidate()
    {
        // Chỉ sinh lưới nếu toggle generateGrid được bật
        if (generateGrid && grid != null && hexagon != null)
        {
            GenerateGrid();
            generateGrid = false; // Đặt lại toggle sau khi sinh
        }
    }

    // Phương thức để sinh lưới
    public void GenerateGrid()
    {
        // Kiểm tra điều kiện
        if (grid == null || hexagon == null)
        {
            Debug.LogWarning("Grid hoặc Hexagon prefab chưa được gán trong Inspector!");
            return;
        }

        // Xóa tất cả các đối tượng con hiện tại
        DeleteAllChildren();

        // Sinh lưới lục giác
        for (int x = -gridSize; x <= gridSize; x++)
        {
            for (int y = -gridSize; y <= gridSize; y++)
            {
                Vector3 spawnPos = grid.CellToWorld(new Vector3Int(x, y, 0));
                if (spawnPos.magnitude > grid.CellToWorld(new Vector3Int(1, 0, 0)).magnitude * gridSize)
                    continue;

                Instantiate(hexagon, spawnPos, hexagon.transform.rotation, transform);
            }
        }
    }

    // Phương thức xóa tất cả các đối tượng con
    private void DeleteAllChildren()
    {
        // Lặp ngược để tránh lỗi khi xóa
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
#if UNITY_EDITOR
            DestroyImmediate(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }
    }

    // Phương thức để gọi từ UI hoặc script khác
    public void ClearGrid()
    {
        DeleteAllChildren();
    }
}