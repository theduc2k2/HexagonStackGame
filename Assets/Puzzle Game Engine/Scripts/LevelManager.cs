using System.Collections;
using UnityEngine;
using HyperPuzzleEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace HyperPuzzleEngine
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Level Reload")]
        public bool reloadAfterLevelFailed = false;
        public float reloadDelay = 2f;

        [HideInInspector] public int tempLevelIndex = 0;

        [Space]
        public GameObject checkForLevelsInChildren;

        List<GameObject> levels = new List<GameObject>();
        List<GameObject> levelClearedPanels = new List<GameObject>();
        List<GameObject> levelFailedPanels = new List<GameObject>();

        int levelIndexAtStart;

        [Space]
        public UnityEvent OnLoadedNextLevel;

        bool alreadyClearedPanel = false;
        bool alreadyLoadedNextLevel = false;

        void Start()
        {
            SetUpLevelDetails();
            LoadCurrentLevel();
        }

        private void SetUpLevelDetails()
        {
            levels.Clear();
            levelClearedPanels.Clear();
            levelFailedPanels.Clear();

            int i = 0;

            if (checkForLevelsInChildren == null) return;

            foreach (SetUpLevelDetailsUI level in checkForLevelsInChildren.GetComponentsInChildren<SetUpLevelDetailsUI>(true))
            {
                if (level.GetComponentInParent<LevelCreator>(true) == null)
                {
                    levels.Add(level.levelObject);
                    levelClearedPanels.Add(level.levelClearedPanel);
                    levelFailedPanels.Add(level.levelFailedPanel);
                    level.levelText.text = "Level " + (i + 1).ToString();

                    i++;
                }
            }
        }

        private void LoadCurrentLevel()
        {
            tempLevelIndex = PlayerPrefs.GetInt(gameObject.name + "_Level", 0);

            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i] != null)
                {
                    levels[i].gameObject.SetActive(false);

                    if (i == tempLevelIndex)
                        levels[tempLevelIndex].gameObject.SetActive(true);
                }
            }


            levelIndexAtStart = tempLevelIndex;
        }

        public void LoadNextLevel(float delay)
        {
            if (alreadyLoadedNextLevel) return;
            alreadyLoadedNextLevel = true;
            Invoke(nameof(CanLoadNextLevelAgain), 3f);

            StartCoroutine(LoadingNextLevel(delay));
        }

        IEnumerator LoadingNextLevel(float delay)
        {
            //Loads next level only if player has not done it already in this game session

            yield return new WaitForSeconds(delay);

            int potentialNextLevelIndex = tempLevelIndex + 1;

            if (potentialNextLevelIndex >= levels.Count)
                potentialNextLevelIndex = 0;

            if (potentialNextLevelIndex != levelIndexAtStart)
            {
                foreach (GameObject panel in levelFailedPanels)
                    panel.SetActive(false);
                foreach (GameObject panel in levelClearedPanels)
                    panel.SetActive(false);

                tempLevelIndex++;

                if (tempLevelIndex >= levels.Count)
                    tempLevelIndex = 0;

                for (int i = 0; i < levels.Count; i++)
                    levels[i].gameObject.SetActive(false);

                levels[tempLevelIndex].gameObject.SetActive(true);

                PlayerPrefs.SetInt(gameObject.name + "_Level", tempLevelIndex);
            }
            else
            {
                PlayerPrefs.SetInt(gameObject.name + "_Level", potentialNextLevelIndex);
            }

            OnLoadedNextLevel.Invoke();

            Transform blockClick = transform.Find("BlockClick");
            if (blockClick != null)
                blockClick.gameObject.SetActive(false);
        }

        #region Cleared and Failed Level Panels

        public void ActivateLevelClearedPanel()
        {
            if (alreadyClearedPanel) return;
            alreadyClearedPanel = true;
            Invoke(nameof(CanLoadNextLevelAgain), 3f);

            Debug.Log("Level Cleared For Game: " + gameObject.name);
            if (levelClearedPanels.Count > tempLevelIndex)
                levelClearedPanels[tempLevelIndex].SetActive(true);
            if (levelFailedPanels.Count > tempLevelIndex)
                levelFailedPanels[tempLevelIndex].SetActive(false);

            if (GetComponent<SoundsManagerForTemplate>() != null)
                GetComponent<SoundsManagerForTemplate>().PlaySound_Level_Won();

            Transform blockClick = transform.Find("BlockClick");
            if (blockClick != null)
                blockClick.gameObject.SetActive(true);
        }

        public void ActivateLevelFailedPanel()
        {
            if (alreadyClearedPanel) return;
            alreadyClearedPanel = true;
            Invoke(nameof(CanLoadNextLevelAgain), 3f);

            if (levelClearedPanels.Count > tempLevelIndex)
                levelClearedPanels[tempLevelIndex].SetActive(false);
            if (levelFailedPanels.Count > tempLevelIndex)
                levelFailedPanels[tempLevelIndex].SetActive(true);

            if (GetComponent<SoundsManagerForTemplate>() != null)
                GetComponent<SoundsManagerForTemplate>().PlaySound_Level_Failed();

            if (reloadAfterLevelFailed)
                Invoke(nameof(ReloadGame), reloadDelay);

            Transform blockClick = transform.Find("BlockClick");
            if (blockClick != null)
                blockClick.gameObject.SetActive(true);
        }

        private void ReloadGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void CanClearLevelAgain()
        {
            alreadyClearedPanel = false;
        }

        private void CanLoadNextLevelAgain()
        {
            alreadyLoadedNextLevel = alreadyClearedPanel = false;
        }

        #endregion
    }
}