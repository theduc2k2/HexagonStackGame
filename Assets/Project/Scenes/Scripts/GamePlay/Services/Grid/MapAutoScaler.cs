using UnityEngine;

public sealed class MapAutoScaler
{
    private readonly float maxWidth;
    private readonly float maxHeight;

    public MapAutoScaler(float maxWidth, float maxHeight)
    {
        this.maxWidth = maxWidth;
        this.maxHeight = maxHeight;
    }

    public void Apply(Transform mapRoot, Bounds bounds)
    {
        if (mapRoot == null)
            return;

        float mapWidth = bounds.size.x + 2f;
        float mapHeight = bounds.size.z + 2.5f;
        float scaleX = maxWidth / mapWidth;
        float scaleZ = maxHeight / mapHeight;
        float finalScale = Mathf.Min(1f, scaleX, scaleZ);

        mapRoot.localScale = new Vector3(finalScale, finalScale, finalScale);
        mapRoot.localPosition = -bounds.center * finalScale;
    }
}
