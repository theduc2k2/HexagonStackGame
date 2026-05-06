using System.Collections.Generic;
using UnityEngine;

public sealed class MergeCandidateSelector
{
    public List<GridCell> FindSimilarTopColorNeighbors(Color color, IEnumerable<GridCell> neighbors)
    {
        List<GridCell> similarNeighbors = new List<GridCell>();

        foreach (GridCell neighbor in neighbors)
        {
            if (neighbor == null || !neighbor.IsOccupied)
                continue;

            if (neighbor.Stack.GetTopHexagonColor() == color)
                similarNeighbors.Add(neighbor);
        }

        return similarNeighbors;
    }

    public List<Hexagon> FindTopMatchingHexagons(Color color, IEnumerable<GridCell> cells)
    {
        List<Hexagon> matchingHexagons = new List<Hexagon>();

        foreach (GridCell cell in cells)
        {
            HexStack stack = cell.Stack;
            for (int i = stack.Hexagons.Count - 1; i >= 0; i--)
            {
                Hexagon hexagon = stack.Hexagons[i];
                if (hexagon.color != color)
                    break;

                matchingHexagons.Add(hexagon);
                hexagon.SetParent(null);
            }
        }

        return matchingHexagons;
    }
}
