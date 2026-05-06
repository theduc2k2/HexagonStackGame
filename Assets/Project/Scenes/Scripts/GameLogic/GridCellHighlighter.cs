using UnityEngine;

public sealed class GridCellHighlighter
{
    private readonly Color placeableColor;
    private readonly float emissionStrength;
    private GridCell highlightedCell;

    public GridCellHighlighter(Color placeableColor, float emissionStrength)
    {
        this.placeableColor = placeableColor;
        this.emissionStrength = emissionStrength;
    }

    public void Set(GridCell newCell)
    {
        if (highlightedCell == newCell)
            return;

        Apply(highlightedCell, false);
        highlightedCell = newCell;
        Apply(highlightedCell, true);
    }

    private void Apply(GridCell cell, bool isHighlighted)
    {
        if (cell == null)
            return;

        Renderer[] renderers = cell.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rend = renderers[i];
            MaterialPropertyBlock block = new MaterialPropertyBlock();

            if (isHighlighted)
            {
                rend.GetPropertyBlock(block);
                block.SetColor("_Color", placeableColor);
                block.SetColor("_BaseColor", placeableColor);
                block.SetColor("_EmissionColor", placeableColor * emissionStrength);
            }

            rend.SetPropertyBlock(block);
        }
    }
}
