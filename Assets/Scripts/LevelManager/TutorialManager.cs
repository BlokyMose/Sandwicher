using Encore.Utility;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityUtility;

namespace Sandwicher
{
    [RequireComponent(typeof(LevelManager))]
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField]
        Image homeBut;

        [SerializeField]
        Image nextBut;

        [SerializeField]
        Image signBut;

        [SerializeField]
        List<GameObject> tutorialTexts = new();

        [Title("Game")]
        [SerializeField]
        GameObject profitRatingPanel;

        [SerializeField]
        GameObject gameCanvas;

        [SerializeField]
        GameObject startCanvas;

        [SerializeField]
        GameObject recipeCanvas;

        [SerializeField]
        GameObject customerCanvas;

        [SerializeField]
        GameObject scoreCanvas;

        [SerializeField]
        GameObject menuCanvas;

        LevelManager levelManager;
        int pageIndex;

        private void Awake()
        {
            levelManager = GetComponent<LevelManager>();
            homeBut.AddEventTrigger(levelManager.LoadMainMenu);
            nextBut.AddEventTrigger(NextPage);
            signBut.AddEventTrigger(levelManager.LoadNextLevel);
            ShowPage(pageIndex);
        }

        public void NextPage()
        {
            ShowPage(pageIndex+1);
        }

        public void ShowPage(int pageIndex)
        {
            if (!tutorialTexts.HasIndex(pageIndex)) return;
            this.pageIndex = pageIndex;

            foreach (var text in tutorialTexts) text.SetActive(false);
            tutorialTexts[pageIndex].SetActive(true);

            if (pageIndex == 0)
            {
                levelManager.gameObject.SetActive(true);
                profitRatingPanel.SetActive(false);
                gameCanvas.SetActive(false);
                startCanvas.SetActive(false);
                recipeCanvas.SetActive(false);
                customerCanvas.SetActive(false);
                scoreCanvas.SetActive(false);
                menuCanvas.SetActive(false);

                nextBut.gameObject.SetActive(true);
                homeBut.gameObject.SetActive(true);
                signBut.gameObject.SetActive(false);
            }

            else if (pageIndex == 1)
            {
                levelManager.SetupMenus(true, true, false);
                menuCanvas.SetActive(true);
            }

            else if (pageIndex == 2)
            {
                levelManager.StartGame();
                gameCanvas.GetComponent<CanvasGroup>().alpha = 0;
            }

            else if (pageIndex == 3)
            {
                gameCanvas.SetActive(true);
                gameCanvas.GetComponent<CanvasGroup>().alpha = 1;
                nextBut.gameObject.SetActive(false);
                homeBut.gameObject.SetActive(false);
            }

            else if (pageIndex == 4)
            {
                nextBut.gameObject.SetActive(false);
                homeBut.gameObject.SetActive(false);
            }

            else if (pageIndex == 6)
            {
                customerCanvas.SetActive(false);
                gameCanvas.SetActive(false);
                profitRatingPanel.SetActive(true);
                levelManager.StopAllCoroutines();

                var allGOs = new List<GameObject>(FindObjectsOfType<GameObject>());
                foreach (var go in allGOs)
                {
                    if (go.name.Contains("Cus_"))
                        go.SetActive(false);
                }

                nextBut.gameObject.SetActive(true);
                homeBut.gameObject.SetActive(true);
            }
            else if (pageIndex == 7)
            {
                profitRatingPanel.SetActive(false);
                nextBut.gameObject.SetActive(false);
                signBut.gameObject.SetActive(true);
            }


        }

    }
}
