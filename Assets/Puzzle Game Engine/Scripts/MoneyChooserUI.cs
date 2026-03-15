using System;
using UnityEngine;
using UnityEngine.UI;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class MoneyChooserUI : MonoBehaviour
    {
        [Range(1, 3)]
        public int displayMoneyIndex;

        public GameObject[] moneysUI;
        public GameObject[] moneys1Toggles;
        public GameObject[] moneys2Toggles;
        public GameObject[] moneys3Toggles;

        private void OnValidate()
        {
            UpdateMoneyUI();
        }

        private void UpdateMoneyUI()
        {
            switch (displayMoneyIndex)
            {
                case 1:
                    moneysUI[0].SetActive(true);
                    moneysUI[1].SetActive(false);
                    moneysUI[2].SetActive(false);

                    for (int i = 0; i < moneys1Toggles.Length; i++)
                        moneys1Toggles[i].SetActive(true);

                    for (int i = 0; i < moneys2Toggles.Length; i++)
                        moneys2Toggles[i].SetActive(false);

                    for (int i = 0; i < moneys3Toggles.Length; i++)
                        moneys3Toggles[i].SetActive(false);
                    break;

                case 2:
                    moneysUI[0].SetActive(false);
                    moneysUI[1].SetActive(true);
                    moneysUI[2].SetActive(false);

                    for (int i = 0; i < moneys1Toggles.Length; i++)
                        moneys1Toggles[i].SetActive(false);

                    for (int i = 0; i < moneys2Toggles.Length; i++)
                        moneys2Toggles[i].SetActive(true);

                    for (int i = 0; i < moneys3Toggles.Length; i++)
                        moneys3Toggles[i].SetActive(false);
                    break;

                case 3:
                    moneysUI[0].SetActive(false);
                    moneysUI[1].SetActive(false);
                    moneysUI[2].SetActive(true);

                    for (int i = 0; i < moneys1Toggles.Length; i++)
                        moneys1Toggles[i].SetActive(false);

                    for (int i = 0; i < moneys2Toggles.Length; i++)
                        moneys2Toggles[i].SetActive(false);

                    for (int i = 0; i < moneys3Toggles.Length; i++)
                        moneys3Toggles[i].SetActive(true);
                    break;
            }
        }

        public void OverWriteValue_MoneyIndex(Slider slider)
        {
            displayMoneyIndex = Mathf.RoundToInt(slider.value);

            UpdateMoneyUI();
        }
    }
}
