#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class HexagonLevelEditor : EditorWindow
{
    private int currentLevelID = 1;
    // Kích thước chuẩn của Hexagon có bán kính 1
    private float gridCellSizeX = 1.732f; 
    private float gridCellSizeY = 2f;
    
    private HashSet<Vector2Int> activeCells = new HashSet<Vector2Int>();
    
    private const string LEVELS_PATH = "Assets/Project/Resources/Levels";
    private float hexRenderSize = 15f; // Kích thước hiển thị 2D trên Tool

    [MenuItem("Hexagon Tools/2. Map 2D Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<HexagonLevelEditor>("Level Editor 2D");
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        
        // --- Bảng điều khiển bên trái ---
        GUILayout.BeginVertical(GUILayout.Width(250));
        GUILayout.Label("CÔNG CỤ VẼ MAP 2D", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        currentLevelID = EditorGUILayout.IntField("Level ID:", currentLevelID);
        gridCellSizeX = EditorGUILayout.FloatField("Cell Size X (3D):", gridCellSizeX);
        gridCellSizeY = EditorGUILayout.FloatField("Cell Size Y (3D):", gridCellSizeY);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("1. LOAD LEVEL", GUILayout.Height(35)))
        {
            LoadLevelData();
        }
        
        if (GUILayout.Button("2. CẬP NHẬT (SAVE LẠI)", GUILayout.Height(35)))
        {
            SaveLevelData();
        }

        if (GUILayout.Button("XÓA TRẮNG BẢNG", GUILayout.Height(25)))
        {
            activeCells.Clear();
            Repaint();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "HƯỚNG DẪN VẼ:\n\n" +
            "👉 Rê chuột vào vùng tối bên phải.\n" +
            "👉 Bấm Chuột Trái để thêm/xóa ô.\n" +
            "👉 Giữ Shift + Chuột Phải: Thêm ô.\n" +
            "👉 Giữ Ctrl + Chuột Phải: Xóa ô.\n\n" +
            "Bạn có thể giữ chuột và kéo rê (Drag) để tô nhanh nhiều ô cùng lúc.", 
            MessageType.Info);
            
        GUILayout.EndVertical();

        // --- Bảng vẽ Map bên phải ---
        Rect rect = GUILayoutUtility.GetRect(600, 600, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUI.BeginGroup(rect);
        
        // Vẽ nền tối
        EditorGUI.DrawRect(new Rect(0, 0, rect.width, rect.height), new Color(0.15f, 0.15f, 0.15f));

        DrawAndHandleHexagonGrid(rect);

        GUI.EndGroup();
        GUILayout.EndHorizontal();
    }

    private void DrawAndHandleHexagonGrid(Rect rect)
    {
        Event e = Event.current;
        Vector2 mousePos = e.mousePosition;

        float hexWidth = Mathf.Sqrt(3f) * hexRenderSize;
        float hexHeight = 2f * hexRenderSize;
        float vertSpacing = hexHeight * 0.75f;
        
        // Tâm của bản vẽ nằm giữa màn hình
        Vector2 gridOrigin = new Vector2(rect.width / 2f, rect.height / 2f);

        int mapRadius = 25; // Số ô mỗi chiều

        for (int y = -mapRadius; y <= mapRadius; y++)
        {
            for (int x = -mapRadius; x <= mapRadius; x++)
            {
                // Thuật toán Pointy-Topped, Odd-R layout
                float xOffset = (Mathf.Abs(y) % 2 == 1) ? hexWidth * 0.5f : 0f;
                float px = x * hexWidth + xOffset;
                float py = y * vertSpacing;
                
                Vector2 center = gridOrigin + new Vector2(px, -py); // -py vì Y màn hình đi xuống, Y 3D đi lên
                
                // Bỏ qua nếu ô đó nằm ngoài vùng hiển thị để tối ưu
                if (center.x < -50 || center.x > rect.width + 50 || center.y < -50 || center.y > rect.height + 50)
                    continue;

                Vector2Int cellPos = new Vector2Int(x, y);
                bool isActive = activeCells.Contains(cellPos);
                
                float dist = Vector2.Distance(mousePos, center);
                bool isHovered = dist < hexRenderSize * 0.85f;

                // Xử lý sự kiện chuột
                if (isHovered && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
                {
                    if (e.button == 0) // Chuột trái -> Toggle (Chỉ Toggle khi click xuống, Drag thì tự đoán ý đồ)
                    {
                        if (e.type == EventType.MouseDown)
                        {
                            if (isActive) activeCells.Remove(cellPos);
                            else activeCells.Add(cellPos);
                        }
                        else // MouseDrag chuột trái: nếu đang đè chuột vào ô đã kích hoạt thì coi như là cọ vẽ thêm
                        {
                            activeCells.Add(cellPos);
                        }
                        e.Use();
                    }
                    else if (e.button == 1) // Chuột phải
                    {
                        if (e.shift)
                        {
                            activeCells.Add(cellPos);
                            e.Use();
                        }
                        else if (e.control)
                        {
                            activeCells.Remove(cellPos);
                            e.Use();
                        }
                    }
                }

                // Vẽ Polygon (Hình lục giác)
                Vector3[] verts = GetHexVertices(center, hexRenderSize);
                
                if (isActive)
                {
                    Handles.color = new Color(0f, 0.8f, 1f, 0.8f);
                    Handles.DrawAAConvexPolygon(verts);
                }
                else if (isHovered)
                {
                    Handles.color = new Color(1f, 1f, 1f, 0.2f);
                    Handles.DrawAAConvexPolygon(verts);
                }
                
                Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.4f); // Viền mờ
                Handles.DrawPolyLine(verts);
            }
        }

        // Ép vẽ lại mượt mà khi di chuột
        if (e.type == EventType.MouseDrag || e.type == EventType.MouseMove)
        {
            Repaint();
        }
    }

    private Vector3[] GetHexVertices(Vector2 center, float size)
    {
        Vector3[] corners = new Vector3[7];
        for (int i = 0; i < 6; i++)
        {
            // Pointy topped: -30 độ offset
            float angle_deg = 60f * i - 30f; 
            float angle_rad = Mathf.PI / 180f * angle_deg;
            corners[i] = new Vector3(center.x + size * Mathf.Cos(angle_rad), center.y + size * Mathf.Sin(angle_rad), 0);
        }
        corners[6] = corners[0];
        return corners;
    }

    private void LoadLevelData()
    {
        string assetPath = $"{LEVELS_PATH}/Level_{currentLevelID}.asset";
        LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);

        activeCells.Clear();

        if (data != null)
        {
            Grid grid = CreateTempGrid();
            
            foreach (CellData cell in data.cells)
            {
                // LocalToCell trả về Vector3Int trong đó x, y là trục của bản đồ Lục giác (Grid 2D), z là độ cao
                Vector3Int gridCoord = grid.LocalToCell(cell.localPosition);
                activeCells.Add(new Vector2Int(gridCoord.x, gridCoord.y)); 
            }
            
            DestroyImmediate(grid.gameObject);
            Debug.Log($"✅ Đã load Level {currentLevelID} với {activeCells.Count} ô!");
        }
        else
        {
            Debug.LogWarning($"⚠️ Level {currentLevelID} chưa tồn tại. Sẵn sàng tạo mới!");
        }
    }

    private void SaveLevelData()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Project/Resources"))
            AssetDatabase.CreateFolder("Assets/Project", "Resources");
        if (!AssetDatabase.IsValidFolder(LEVELS_PATH))
            AssetDatabase.CreateFolder("Assets/Project/Resources", "Levels");

        string assetPath = $"{LEVELS_PATH}/Level_{currentLevelID}.asset";
        LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);

        bool isNew = false;
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<LevelData>();
            data.levelID = currentLevelID;
            isNew = true;
        }

        data.cells.Clear();
        Grid grid = CreateTempGrid();

        foreach (Vector2Int cellCoord in activeCells)
        {
            CellData cellData = new CellData();
            // Truyền x, y vào Grid. Nó sẽ tự động dùng Swizzle XZY để áp y vào trục Z trong môi trường 3D.
            cellData.localPosition = grid.CellToLocal(new Vector3Int(cellCoord.x, cellCoord.y, 0));
            cellData.isUnlocked = true;
            data.cells.Add(cellData);
        }

        DestroyImmediate(grid.gameObject);

        if (isNew)
        {
            AssetDatabase.CreateAsset(data, assetPath);
        }
        else
        {
            EditorUtility.SetDirty(data);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"✅ Đã CẬP NHẬT Level {currentLevelID} thành công ({activeCells.Count} ô)!");
    }

    private Grid CreateTempGrid()
    {
        GameObject go = new GameObject("TempGrid");
        Grid grid = go.AddComponent<Grid>();
        grid.cellLayout = GridLayout.CellLayout.Hexagon;
        grid.cellSwizzle = GridLayout.CellSwizzle.XZY; // Map 2D vào trục X-Z của 3D
        grid.cellSize = new Vector3(gridCellSizeX, gridCellSizeY, 0);
        return grid;
    }
}
#endif
