using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class HexStack : MonoBehaviour
{
    public List<Hexagon> Hexagons { get; private set; }
    public GridCell CurrentGridCell { get; set; }

    public void Add(Hexagon hexagon)
    {
        if (Hexagons == null)
            Hexagons = new List<Hexagon>();
        Hexagons.Add(hexagon);
        hexagon.transform.parent = transform;
    }

    // Start is called before the first frame update
    public Color GetTopHexagonColor() => Hexagons[^1].color;

    // return Hexagons[Hexagons.Count - 1].color;
    // return Hexagons.Last().color;



    public void Place()
    {
        foreach (Hexagon hexagon in Hexagons)
            hexagon.DisableCollider();
    }
    public bool Contains(Hexagon hexagon) => Hexagons.Contains(hexagon);
    public void Remove(Hexagon hexagon)
    {
        Hexagons.Remove(hexagon);
        if (Hexagons.Count <= 0)
            DestroyImmediate(gameObject);
    }
}
