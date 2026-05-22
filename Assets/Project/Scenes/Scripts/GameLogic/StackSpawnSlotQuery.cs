using UnityEngine;

public sealed class StackSpawnSlotQuery
{
    private readonly Transform stackPositionParent;
    private readonly float occupiedRadius;

    public StackSpawnSlotQuery(Transform stackPositionParent, float occupiedRadius)
    {
        this.stackPositionParent = stackPositionParent;
        this.occupiedRadius = occupiedRadius;
    }

    public bool IsSlotAvailable(Transform slot)
    {
        return slot != null
            && !HasActiveStackInSlot(slot)
            && !IsOccupiedByWorldPosition(slot);
    }

    public bool IsSpawnSlot(Transform target)
    {
        return stackPositionParent != null
            && target != null
            && target.IsChildOf(stackPositionParent);
    }

    public void ValidateSpawnPoints()
    {
        if (stackPositionParent == null)
            return;

        const float minSqrDistance = 0.01f;
        for (int i = 0; i < stackPositionParent.childCount; i++)
        {
            Vector3 a = stackPositionParent.GetChild(i).position;
            for (int j = i + 1; j < stackPositionParent.childCount; j++)
            {
                Vector3 b = stackPositionParent.GetChild(j).position;
                if ((a - b).sqrMagnitude <= minSqrDistance)
                {
                    Debug.LogWarning(
                        $"Spawn points {i} and {j} are too close. This can cause visual overlap.",
                        stackPositionParent);
                }
            }
        }
    }

    private static bool HasActiveStackInSlot(Transform slot)
    {
        for (int i = 0; i < slot.childCount; i++)
        {
            Transform child = slot.GetChild(i);
            if (child == null || !child.gameObject.activeInHierarchy)
                continue;

            if (child.GetComponent<HexStack>() != null)
                return true;
        }

        return false;
    }

    private bool IsOccupiedByWorldPosition(Transform slot)
    {
        HexStack[] allStacks = Object.FindObjectsByType<HexStack>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        float sqrRadius = occupiedRadius * occupiedRadius;
        Vector3 slotPos = slot.position;

        for (int i = 0; i < allStacks.Length; i++)
        {
            HexStack stack = allStacks[i];
            if (stack == null || !stack.gameObject.activeInHierarchy)
                continue;

            if ((stack.transform.position - slotPos).sqrMagnitude <= sqrRadius)
                return true;
        }

        return false;
    }
}
