#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

public class LevelDataExporter : EditorWindow
{
    [MenuItem("Hexagon Tools/1. Export Selected Levels to Data")]
    public static void ExportLevels()
    {
        // Sử dụng những GameObject đang được chọn trên Hierarchy thay vì tìm trong LevelController
        GameObject[] selectedLevels = Selection.gameObjects.OrderBy(g => g.transform.GetSiblingIndex()).ToArray();
        
        if (selectedLevels == null || selectedLevels.Length == 0)
        {
            Debug.LogError("❌ Vui lòng chọn các GameObject Level cũ trên Hierarchy trước khi bấm Export!");
            return;
        }

        string folderPath = "Assets/Project/Resources/Levels";
        if (!AssetDatabase.IsValidFolder("Assets/Project/Resources"))
        {
            AssetDatabase.CreateFolder("Assets/Project", "Resources");
        }
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Project/Resources", "Levels");
        }

        int count = 0;
        for (int i = 0; i < selectedLevels.Length; i++)
        {
            GameObject levelObj = selectedLevels[i];
            
            LevelData levelData = ScriptableObject.CreateInstance<LevelData>();
            // Đặt ID dựa trên thứ tự đang chọn
            levelData.levelID = i + 1; 
            
            // CHỈ LẤY CÁC Ô ĐANG ACTIVE (bỏ qua những ô bị tàng hình/ẩn đi)
            GridCell[] cells = levelObj.GetComponentsInChildren<GridCell>(false);
            if (cells.Length == 0) continue;

            foreach (GridCell cell in cells)
            {
                CellData cellData = new CellData();
                cellData.localPosition = cell.transform.localPosition;
                cellData.isUnlocked = true;
                levelData.cells.Add(cellData);
            }

            string assetPath = $"{folderPath}/Level_{levelData.levelID}.asset";
            AssetDatabase.CreateAsset(levelData, assetPath);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"✅ Đã XUẤT THÀNH CÔNG {count} levels ra dạng Data! Tool giờ chỉ lấy các ô đang Active.");
    }
}
#endif
