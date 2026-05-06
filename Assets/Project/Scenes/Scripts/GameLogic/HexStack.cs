using System.Collections.Generic;
using UnityEngine;

public class HexStack : MonoBehaviour
{
    private readonly List<Hexagon> hexagons = new List<Hexagon>();

    public List<Hexagon> Hexagons => hexagons;
    public GridCell CurrentGridCell { get; set; }
    public int Count => hexagons.Count;

    public void Add(Hexagon hexagon)
    {
        if (hexagon == null || hexagons.Contains(hexagon))
            return;

        hexagons.Add(hexagon);
        hexagon.Configure(this);
        hexagon.transform.SetParent(transform);
    }

    public Color GetTopHexagonColor() => hexagons[^1].color;

    public void Place()
    {
        foreach (Hexagon hexagon in hexagons)
            hexagon.DisableCollider();
    }

    public bool Contains(Hexagon hexagon) => hexagons.Contains(hexagon);

    public void Remove(Hexagon hexagon)
    {
        if (hexagon == null || !hexagons.Remove(hexagon))
            return;

        hexagon.Configure(null);

        if (hexagons.Count > 0)
            return;

        if (Application.isPlaying)
            Destroy(gameObject);
        else
            DestroyImmediate(gameObject);
    }
}
