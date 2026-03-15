using UnityEngine;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
public class SnapObjectToGrid : MonoBehaviour
{
    public enum AxisLock { None, X, Y, Z } // Define the AxisLock enum

    public bool snapInPlayMode = false;
    public bool snapInEditMode = true;
    public AxisLock keepAxis = AxisLock.None; // Select axis to keep in the inspector
    public Vector3 snapOffset = Vector3.zero; // Offset applied during snapping

    private GridMovementsController gridController;
    private Vector3 lastPosition;

    private void OnEnable()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            if (snapInPlayMode && transform.position != lastPosition)
            {
                SnapToGrid();
                lastPosition = transform.position;
            }
        }
        else
        {
            if (snapInEditMode && transform.position != lastPosition)
            {
                SnapToGrid();
                lastPosition = transform.position;
            }
        }
    }

    private void SnapToGrid()
    {
        gridController = transform.parent.GetComponentInChildren<GridMovementsController>();

        if (gridController != null)
        {
            Vector3 snappedPosition = gridController.GetClosestGridPoint(transform.position);

            // Apply the snap offset
            snappedPosition += snapOffset;

            // Keep the original position on the selected axis
            switch (keepAxis)
            {
                case AxisLock.X:
                    snappedPosition.x = transform.position.x;
                    break;
                case AxisLock.Y:
                    snappedPosition.y = transform.position.y;
                    break;
                case AxisLock.Z:
                    snappedPosition.z = transform.position.z;
                    break;
            }

            transform.position = snappedPosition;
        }
    }
}
}