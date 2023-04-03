using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityUtility;
using static UnityUtility.EventTriggerUtility;
using static Sandwicher.SandwicherUtility;

namespace Sandwicher
{
    public class LevelMenu : MonoBehaviour
    {
        [Title("Game")]
        [SerializeField]
        GameCache gameCache;

        [Title("UI")]
        [SerializeField]
        Transform levelPanelsParent;

        [Title("Resources")]
        [SerializeField]
        GameObject levelPanelPrefab;

        [SerializeField]
        SandwicherSymbols symbols;


        void Awake()
        {
            LevelTransition.HideAllTransition();

            SetupLevelPanels(levelPanelsParent, levelPanelPrefab, gameCache);
        }

        void SetupLevelPanels(Transform parent, GameObject levelPanelPrefab, GameCache gameCache)
        {
            parent.DestroyChildren();

            foreach (var level in gameCache.Levels)
            {
                var levelPanel = Instantiate(levelPanelPrefab, parent);
                if (levelPanel.TryGetComponent<GOLister>(out var lister))
                {
                    if (lister.TryGet("title", out var title) &&
                        lister.TryGet("desc", out var desc))
                    {
                        title.GetComponent<TextMeshProUGUI>().text = level.LevelDisplayName;
                        desc.GetComponent<TextMeshProUGUI>().text = level.Description;
                    }

                    DisplayNum(lister, 0, Color.black, symbols, new() { "rating_one", "rating_ten" }, symbols.Nums[0]);
                    DisplayNum(lister, 0, Color.black, symbols, new() { "profit_one", "profit_ten", "profit_hundred" });
                }

                if (levelPanel.TryGetComponent<Image>(out var image))
                {
                    image.AddEventTrigger(() => { LoadGameLevel(level); });
                }
            }
        }

        public void LoadGameLevel(Level level)
        {
            gameCache.GameLevel = level; 
            var transition = Instantiate(gameCache.LevelTransition);
            transition.OnLoad += () => SceneManager.LoadScene(level.SceneName);
        }
    }
}
