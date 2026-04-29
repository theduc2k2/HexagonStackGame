using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CellData
{
    // Lưu lại vị trí (tọa độ local) của ô trên lưới để lúc load lấy ra đặt đúng chỗ
    public Vector3 localPosition; 
    
    // Đánh dấu ô này có khả dụng không (nếu tương lai bạn có hệ thống ổ khóa)
    public bool isUnlocked = true; 
}

[CreateAssetMenu(fileName = "Level_1", menuName = "Hexagon/Level Data")]
public class LevelData : ScriptableObject
{
    public int levelID;
    
    // Điểm mục tiêu của màn chơi (nếu có, nếu không thì dùng ScoreManager hiện tại)
    public int targetScore; 
    
    // Danh sách toàn bộ các ô grid có trong map này
    public List<CellData> cells = new List<CellData>();
}
