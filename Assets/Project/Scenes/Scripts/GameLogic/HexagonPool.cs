using System.Collections.Generic;
using UnityEngine;

public class HexagonPool : MonoBehaviour
{
    public static HexagonPool Instance;

    [Header("Prefab")]
    [SerializeField] private Hexagon hexagonPrefab;

    [Header("Settings")]
    [SerializeField] private int initialHexagonCount = 60;

    private Queue<Hexagon> hexagonPool = new Queue<Hexagon>();
    private Transform poolContainer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Tạo container ẩn để tránh bị hiện trên Grid
        GameObject containerGO = new GameObject("Hex_Hidden_Pool");
        containerGO.transform.SetParent(this.transform);
        containerGO.SetActive(false); 
        poolContainer = containerGO.transform;
        
        for (int i = 0; i < initialHexagonCount; i++) AddHexagonToPool();
    }

    private void AddHexagonToPool()
    {
        Hexagon hex = Instantiate(hexagonPrefab, poolContainer);
        hex.gameObject.SetActive(false);
        hexagonPool.Enqueue(hex);
    }

    public Hexagon GetHexagon(Vector3 position, Quaternion rotation, Transform parent)
    {
        if (hexagonPool.Count == 0) AddHexagonToPool();

        Hexagon hex = hexagonPool.Dequeue();
        hex.transform.SetParent(parent);
        hex.transform.position = position;
        hex.transform.rotation = rotation;
        hex.transform.localScale = Vector3.one;
        hex.PrepareForReuse();
        hex.gameObject.SetActive(true);
        return hex;
    }

    public void ReturnHexagon(Hexagon hex)
    {
        if (hex == null) return;
        
        LeanTween.cancel(hex.gameObject);
        hex.gameObject.SetActive(false);
        hex.transform.SetParent(poolContainer);
        hexagonPool.Enqueue(hex);
    }
}
