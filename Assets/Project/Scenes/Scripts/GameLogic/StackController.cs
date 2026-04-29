using System;
using System.Collections.Generic;
using UnityEngine;

public class StackController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask hexagonLayerMask;
    [SerializeField] private LayerMask gridHexagonLayerMask;
    [SerializeField] private LayerMask groundLayerMask;

    [Header("Drag Settings")]
    [SerializeField] private float dragHeight = 2f;
    [SerializeField] private float dragSmoothTime = 0.06f;
    [SerializeField] private float snapSearchRadius = 1.75f;
    [SerializeField] private float snapReleaseRadius = 2.2f;
    [SerializeField] private float snapReleaseRadiusMultiplier = 1.4f;

    [Header("Placement Highlight")]
    [SerializeField] private Color placeableColor = new Color(0f, 0.9f, 1.0f, 1f);
    [SerializeField] private float emissionStrength = 0.8f;

    [Header("Rotation Settings")]
    [SerializeField] private Transform mapRoot;
    [SerializeField] private float rotationSensitivity = 0.2f;
    [SerializeField] private float rotationSnapSpeed = 5f;

    private float targetRotationY;
    private bool isRotating;
    private Vector3 lastMousePosition;

    private HexStack currentHexStack;
    private Vector3 currentHexStackPos;
    private GridCell targetCell;

    private Vector3 dragVelocity;
    private readonly List<GridCell> cachedGridCells = new List<GridCell>();
    private GridCell highlightedCell;

    public static Action<GridCell> onStackPlaced;

    private void OnDisable()
    {
        SetHighlightedCell(null);
    }

    private void Update()
    {
        SmoothMapSnapRotation();

        if (MergeManager.IsMerging)
        {
            if (isRotating)
            {
                SnapToNearestDirection();
                isRotating = false;
            }
            return;
        }

        ManageController();
    }

    private void ManageController()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(GetClickRay(), out RaycastHit hitDown, 500f, hexagonLayerMask))
            {
                Hexagon hexagon = hitDown.collider.GetComponent<Hexagon>();
                if (hexagon != null && hexagon.HexStack != null)
                {
                    currentHexStack = hexagon.HexStack;
                    currentHexStackPos = currentHexStack.transform.position;
                    dragVelocity = Vector3.zero;
                    RefreshGridCells();
                }
            }
            else if (Physics.Raycast(GetClickRay(), out _, 500f, gridHexagonLayerMask))
            {
                isRotating = true;
                lastMousePosition = Input.mousePosition;
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (currentHexStack != null)
                ManageMouseDrag();
            else if (isRotating)
                RotateMapSmooth();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (currentHexStack != null)
                ManageMouseUp();

            if (isRotating)
            {
                SnapToNearestDirection();
                isRotating = false;
            }
        }
    }

    private void RotateMapSmooth()
    {
        Vector3 delta = Input.mousePosition - lastMousePosition;
        targetRotationY += -delta.x * rotationSensitivity;
        lastMousePosition = Input.mousePosition;
    }

    private void SmoothMapSnapRotation()
    {
        if (mapRoot == null)
            return;

        Quaternion desiredRotation = Quaternion.Euler(0f, targetRotationY, 0f);
        mapRoot.rotation = Quaternion.Lerp(mapRoot.rotation, desiredRotation, Time.deltaTime * rotationSnapSpeed);
    }

    private void SnapToNearestDirection()
    {
        if (mapRoot == null)
            return;

        float[] validAngles = { 0f, 120f, 240f };
        float currentY = mapRoot.rotation.eulerAngles.y;
        float nearestAngle = validAngles[0];
        float minDiff = Mathf.Abs(Mathf.DeltaAngle(currentY, validAngles[0]));

        for (int i = 1; i < validAngles.Length; i++)
        {
            float diff = Mathf.Abs(Mathf.DeltaAngle(currentY, validAngles[i]));
            if (diff < minDiff)
            {
                minDiff = diff;
                nearestAngle = validAngles[i];
            }
        }

        targetRotationY = nearestAngle;
    }

    private void ManageMouseDrag()
    {
        if (!TryGetPointerWorldPosition(out Vector3 pointerWorldPos))
            return;

        Vector3 desiredStackPos = pointerWorldPos;
        desiredStackPos.y = dragHeight;

        currentHexStack.transform.position = Vector3.SmoothDamp(
            currentHexStack.transform.position,
            desiredStackPos,
            ref dragVelocity,
            dragSmoothTime);

        targetCell = FindNearestAvailableCell(pointerWorldPos, snapSearchRadius);
        SetHighlightedCell(targetCell);
    }

        private void ManageMouseUp()
    {
        SetHighlightedCell(null);

        if (targetCell == null)
            targetCell = FindNearestAvailableCell(currentHexStack.transform.position, snapReleaseRadius);

        if (targetCell == null && TryGetPointerWorldPosition(out Vector3 pointerWorldPos))
            targetCell = FindNearestAvailableCell(pointerWorldPos, snapReleaseRadius * snapReleaseRadiusMultiplier);

        if (targetCell == null)
        {
            LeanTween.move(currentHexStack.gameObject, currentHexStackPos, 0.25f).setEase(LeanTweenType.easeOutBack);
            currentHexStack = null;
            dragVelocity = Vector3.zero;
            return;
        }

        Vector3 targetPosition = targetCell.transform.position;
        targetPosition.y = 0.2f;

        // Lưu lại tham chiếu để dùng trong OnComplete của LeanTween
        HexStack stackToPlace = currentHexStack;
        GridCell cellToAssign = targetCell;

        // Logic placement thực hiện ngay để khóa ô (Occupied) tránh lỗi logic
        stackToPlace.Place();
        cellToAssign.AssignHexStack(stackToPlace);
        stackToPlace.transform.SetParent(cellToAssign.transform, true); // Giữ nguyên kích thước to ban đầu để làm animation

        // Hiệu ứng "đặt xuống nhẹ nhàng" và tự động thu nhỏ lại cho vừa với ô lưới
        LeanTween.scale(stackToPlace.gameObject, Vector3.one, 0.15f).setEase(LeanTweenType.easeOutQuad);
        LeanTween.move(stackToPlace.gameObject, targetPosition, 0.15f)
            .setEase(LeanTweenType.easeOutQuad) // Chạm đất êm ái
            .setOnComplete(() =>
            {
                // CHỈ KHI NÀO hạ cánh xong mới kích hoạt hiệu ứng Merge
                onStackPlaced?.Invoke(cellToAssign);
            });

        targetCell = null;
        currentHexStack = null;
        dragVelocity = Vector3.zero;
    }


    private bool TryGetPointerWorldPosition(out Vector3 worldPos)
    {
        Ray ray = GetClickRay();

        if (Physics.Raycast(ray, out RaycastHit hitGrid, 500f, gridHexagonLayerMask))
        {
            worldPos = hitGrid.point;
            return true;
        }

        if (Physics.Raycast(ray, out RaycastHit hitGround, 500f, groundLayerMask))
        {
            worldPos = hitGround.point;
            return true;
        }

        worldPos = Vector3.zero;
        return false;
    }

    private void RefreshGridCells()
    {
        cachedGridCells.Clear();
        GridCell[] cells = FindObjectsOfType<GridCell>();
        StackSpawner spawner = FindObjectOfType<StackSpawner>();

        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i] != null && cells[i].gameObject.activeInHierarchy)
            {
                if (spawner != null && spawner.IsSpawnSlot(cells[i].transform))
                    continue;

                cachedGridCells.Add(cells[i]);
            }
        }
    }

    private GridCell FindNearestAvailableCell(Vector3 worldPos, float maxDistance)
    {
        GridCell nearest = null;
        float bestSqrDistance = maxDistance * maxDistance;

        for (int i = 0; i < cachedGridCells.Count; i++)
        {
            GridCell cell = cachedGridCells[i];
            if (cell == null || !cell.gameObject.activeInHierarchy || cell.IsOccupied)
                continue;

            float sqrDistance = (cell.transform.position - worldPos).sqrMagnitude;
            if (sqrDistance < bestSqrDistance)
            {
                bestSqrDistance = sqrDistance;
                nearest = cell;
            }
        }

        return nearest;
    }

    private void SetHighlightedCell(GridCell newCell)
    {
        if (highlightedCell == newCell)
            return;

        ApplyHighlight(highlightedCell, false);
        highlightedCell = newCell;
        ApplyHighlight(highlightedCell, true);
    }

    private void ApplyHighlight(GridCell cell, bool isHighlighted)
    {
        if (cell == null)
            return;

        Renderer[] renderers = cell.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rend = renderers[i];
            var block = new MaterialPropertyBlock();

            if (isHighlighted)
            {
                rend.GetPropertyBlock(block);
                block.SetColor("_Color", placeableColor);
                block.SetColor("_BaseColor", placeableColor);
                block.SetColor("_EmissionColor", placeableColor * emissionStrength);
                rend.SetPropertyBlock(block);
            }
            else
            {
                rend.SetPropertyBlock(block);
            }
        }
    }

    private Ray GetClickRay() => Camera.main.ScreenPointToRay(Input.mousePosition);
}
