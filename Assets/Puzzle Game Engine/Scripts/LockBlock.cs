using UnityEngine;
using TMPro;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class LockBlock : MonoBehaviour
    {
        [Header("Lock Settings")]
        public bool isLocked = true; // Toggle lock status in the editor
        public GameObject lockedBlock; // Reference to the locked state GameObject

        private TextMeshPro[] lockTexts; // Reference to TextMeshPro component

        [Range(1, 50)]
        public int boxesToClear = 5; // Number of boxes to clear to unlock

        [Header("Locked Block Global Rotation")]
        public Quaternion lockedBlockGlobalRotation = Quaternion.identity; // Desired global rotation for the lockedBlock

        private void OnValidate()
        {
            lockTexts = lockedBlock.GetComponentsInChildren<TextMeshPro>();
            UpdateLockedState();
        }

        private void Start()
        {
            lockTexts = lockedBlock.GetComponentsInChildren<TextMeshPro>();
            UpdateLockedState();
        }

        private void UpdateLockedState()
        {
            // Enable or disable the locked block based on the isLocked property
            if (lockedBlock != null)
            {
                lockedBlock.SetActive(isLocked);

                // Set the lockedBlock's rotation to the specified global rotation
                lockedBlock.transform.rotation = lockedBlockGlobalRotation;

                // If locked, update the text to show the remaining boxes to clear
                if (isLocked)
                {
                    for (int i = 0; i < lockTexts.Length; i++)
                        lockTexts[i].text = boxesToClear.ToString();
                }
            }
        }

        public void ReduceLockedCounter()
        {
            if (isLocked)
            {
                boxesToClear = Mathf.Max(0, boxesToClear - 1); // Ensure it doesn't go below 0

                // Update the text
                if (lockTexts.Length > 0)
                {
                    for (int i = 0; i < lockTexts.Length; i++)
                        lockTexts[i].text = boxesToClear.ToString();
                }

                // If boxesToClear reaches 0, unlock the block
                if (boxesToClear == 0)
                {
                    isLocked = false;
                    UpdateLockedState(); // Refresh the locked state
                }
            }
        }
    }
}