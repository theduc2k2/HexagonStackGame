using UnityEngine;
using UnityEngine.Events;
using HyperPuzzleEngine;
using System.Collections.Generic;
using System;

namespace HyperPuzzleEngine
{
    [RequireComponent(typeof(LevelManager))]
    public class CollectedStacksCounter : MonoBehaviour
    {
        public enum CollectionType
        {
            Stack,
            Piece
        }

        public CollectionType collectionType = CollectionType.Stack;

        [Header("Each Level Needs To Collext X Amount Of Stacks or Pieces To Complete Level")]
        public int[] needToCollect;

        [Space]
        public UnityEvent OnCollectedAny;
        public UnityEvent OnCollectedAll;

        int[] tempCollectedCount;
        private LevelManager levelManager;

        private List<int> collectedInstancesByID = new List<int>();

        private void Start()
        {
            tempCollectedCount = new int[needToCollect.Length];
            levelManager = GetComponent<LevelManager>();
        }

        public void IncreaseCollectedStacks(int count = 1)
        {
            if (collectionType == CollectionType.Piece) return;

            if (!GetComponentInParent<ShowcaseParent>().IsInGameMode()) return;

            tempCollectedCount[levelManager.tempLevelIndex] += count;

            OnCollectedAny.Invoke();

            if (tempCollectedCount[levelManager.tempLevelIndex] >= needToCollect[levelManager.tempLevelIndex])
                OnCollectedAll.Invoke();

            GetComponentInChildren<CollectedPiecesCounter>().IncreaseCollectedCounter(1);
        }

        public void IncreaseCollectedPieces(int count = 1, int instanceID = -1)
        {
            if (collectionType == CollectionType.Stack) return;

            if (!GetComponentInParent<ShowcaseParent>().IsInGameMode()) return;

            if (instanceID != -1)
            {
                if (collectedInstancesByID.Contains(instanceID)) return;
                else collectedInstancesByID.Add(instanceID);
            }

            if (needToCollect.Length < (levelManager.tempLevelIndex + 1))
            {
                needToCollect = new int[levelManager.tempLevelIndex + 1];
                tempCollectedCount = new int[levelManager.tempLevelIndex + 1];
            }

            tempCollectedCount[levelManager.tempLevelIndex] += count;

            OnCollectedAny.Invoke();

            if (tempCollectedCount[levelManager.tempLevelIndex] >= needToCollect[levelManager.tempLevelIndex])
                OnCollectedAll.Invoke();

            GetComponentInChildren<CollectedPiecesCounter>().IncreaseCollectedCounter(1);
        }

        public int GetCurrentlyNeededToCollect()
        {
            return needToCollect[levelManager.tempLevelIndex];
        }

        public int GetCurrentlyCollected()
        {
            return tempCollectedCount[levelManager.tempLevelIndex];
        }

        public void ResetInstanceIDs()
        {
            collectedInstancesByID.Clear();
        }

        public void SetUpCollectable(int indexOfLevel, int countToCollect)
        {
            if (needToCollect.Length < (indexOfLevel + 1))
            {
                needToCollect = new int[indexOfLevel + 1];
                tempCollectedCount = new int[indexOfLevel + 1];
            }
            needToCollect[indexOfLevel] = countToCollect;
        }
    }
}
