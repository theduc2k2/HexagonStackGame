using UnityEngine;

public class GridCell : MonoBehaviour
{
    public HexStack Stack { get; private set; }

    public bool IsOccupied
    {
        get => Stack != null;
        private set { }
    }

    public void AssignHexStack(HexStack stack)
    {
        Stack = stack;
        Debug.Log($"✅ GridCell tại {transform.position} được gán HexStack, IsOccupied: {IsOccupied}");
    }

    public void ClearHexStack()
    {
        if (IsOccupied && Stack != null)
        {
            Destroy(Stack.gameObject);
            Stack = null;
            Debug.Log($"✅ GridCell tại {transform.position} được xóa HexStack, IsOccupied: {IsOccupied}");
        }
    }
}