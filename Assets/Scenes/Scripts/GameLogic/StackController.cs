using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask hexagonLayerMask;
    [SerializeField] private LayerMask gridHexagonLayerMask;
    [SerializeField] private LayerMask groundLayerMask;

    [Header("Rotation Settings")]
    [SerializeField] private Transform mapRoot;
    [SerializeField] private float rotationSensitivity = 0.2f;
    [SerializeField] private float rotationSnapSpeed = 5f;

    private float targetRotationY = 0f;
    private bool isRotating = false;
    private Vector3 lastMousePosition;

    private HexStack currentHexStack;
    private Vector3 currentHexStackPos;

    [Header(" Data ")]
    private GridCell targetCell;

    [Header(" Actions")]
    public static Action<GridCell> onStackPlaced;

    void Update()
    {
        ManageController();
        SmoothMapSnapRotation();
    }

    private void ManageController()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(GetClickRay(), out RaycastHit hitDown, 500, hexagonLayerMask))
            {
                Hexagon hexagon = hitDown.collider.GetComponent<Hexagon>();
                if (hexagon != null && hexagon.HexStack != null)
                {
                    currentHexStack = hexagon.HexStack;
                    currentHexStackPos = currentHexStack.transform.position;
                    Debug.Log($"Đã chọn HexStack tại vị trí: {currentHexStackPos}");
                }
            }
            else if (Physics.Raycast(GetClickRay(), out RaycastHit hitGrid, 500, gridHexagonLayerMask))
            {
                isRotating = true;
                lastMousePosition = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (currentHexStack != null)
            {
                ManagerMouseDrag();
            }
            else if (isRotating)
            {
                RotateMapSmooth();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (currentHexStack != null)
            {
                ManagerMouseUp();
            }

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
        Quaternion desiredRotation = Quaternion.Euler(0f, targetRotationY, 0f);
        mapRoot.rotation = Quaternion.Lerp(mapRoot.rotation, desiredRotation, Time.deltaTime * rotationSnapSpeed);
    }

    private void SnapToNearestDirection()
    {
        float[] validAngles = { 0f, 120f, 240f };
        float currentY = mapRoot.rotation.eulerAngles.y;
        float nearestAngle = validAngles[0];
        float minDiff = Mathf.Abs(Mathf.DeltaAngle(currentY, validAngles[0]));

        for (int i = 1; i < validAngles.Length; i++)
        {
            float angle = validAngles[i];
            float diff = Mathf.Abs(Mathf.DeltaAngle(currentY, angle));
            if (diff < minDiff)
            {
                minDiff = diff;
                nearestAngle = angle;
            }
        }

        targetRotationY = nearestAngle;
    }

    private void ManagerMouseDrag()
    {
        RaycastHit hit;
        if (Physics.Raycast(GetClickRay(), out hit, 500, gridHexagonLayerMask))
        {
            DraggingAboveGridCell(hit);
        }
        else if (Physics.Raycast(GetClickRay(), out hit, 500, groundLayerMask))
        {
            DraggingAboveGround();
        }
        else
        {
            Debug.Log("Không phát hiện ô lưới hoặc mặt đất.");
        }
    }

    private void DraggingAboveGround()
    {
        RaycastHit hit;
        Physics.Raycast(GetClickRay(), out hit, 500, groundLayerMask);
        if (hit.collider == null)
        {
            Debug.Log("chưa phát hiện ô ");
            return;
        }
        Vector3 currentStackTargetPos = hit.point;
        currentStackTargetPos.y = 2;
        currentHexStack.transform.position = Vector3.MoveTowards(
            currentHexStack.transform.position,
            currentStackTargetPos,
            Time.deltaTime * 30);

        targetCell = null;
    }

    private void DraggingAboveGridCell(RaycastHit hit)
    {
        GridCell gridCell = hit.collider.GetComponent<GridCell>();
        if (gridCell == null)
        {
            Debug.Log("Không tìm thấy GridCell trên ô lưới.");
            DraggingAboveGround();
            return;
        }

        if (gridCell.IsOccupied)
        {
            Debug.Log("Ô lưới đã bị chiếm, không thể đặt stack.");
            targetCell = null;
        }
        else
        {
            DraggingAboveNonOccupiedGridCell(gridCell);
        }
    }

    private void DraggingAboveNonOccupiedGridCell(GridCell gridCell)
    {
        Vector3 currentStackTargetPos = gridCell.transform.position;
        currentStackTargetPos.y = 2;
        currentHexStack.transform.position = Vector3.MoveTowards(
            currentHexStack.transform.position,
            currentStackTargetPos,
            Time.deltaTime * 30);

        targetCell = gridCell;
    }

    private void ManagerMouseUp()
    {
        if (targetCell == null)
        {
            currentHexStack.transform.position = currentHexStackPos;
            currentHexStack = null;
            return;
        }
        Vector3 targetPosition = targetCell.transform.position;
        targetPosition.y = 0.2f;
        currentHexStack.transform.position = targetPosition;
        currentHexStack.transform.SetParent(targetCell.transform);
        currentHexStack.Place();
        targetCell.AssignHexStack(currentHexStack);

        onStackPlaced?.Invoke(targetCell);

        targetCell = null;
        currentHexStack = null;
    }

    private Ray GetClickRay() => Camera.main.ScreenPointToRay(Input.mousePosition);
}
