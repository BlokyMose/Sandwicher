using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Sandwicher.MainMenuColors;
using UnityUtility;

namespace Sandwicher
{
    public class MainMenu : MonoBehaviour
    {
        [Title("Game")]
        [SerializeField]
        GameCache gameCache;

        [SerializeField]
        Image tapPanel;

        [Title("World")]
        [SerializeField]
        Image partTimer;
        [SerializeField]
        Image sandwicher;
        [SerializeField]
        TextMeshProUGUI tapToStart;
        [SerializeField]
        Image sky;
        [SerializeField]
        GameObject starsFarAway;
        [SerializeField]
        GameObject starsNear;
        [SerializeField]
        List<Image> clouds = new();


        [Title("Resources")]
        [SerializeField]
        MainMenuColors mainMenuColors;

        [Title("BGM")]
        [SerializeField]
        AudioSource audio;
        [SerializeField]
        AudioClip dawnBGM;
        [SerializeField]
        AudioClip dayBGM;
        [SerializeField]
        AudioClip duskBGM;
        [SerializeField]
        AudioClip nightBGM;

        void Awake()
        {
            LevelTransition.HideAllTransition();

            var now = DateTime.Now;
            var dawn = new DateTime(now.Year, now.Month, now.Day, 5, 30, 0);
            var day = new DateTime(now.Year, now.Month, now.Day, 9, 0, 0);
            var dusk = new DateTime(now.Year, now.Month, now.Day, 16, 30, 0);
            var night = new DateTime(now.Year, now.Month, now.Day, 19, 0, 0);
            if (now > dawn)
            {
                if (now > day)
                {
                    if (now > dusk)
                    {
                        if (now > night)
                        {
                            SetColors("Night");
                            audio.clip = nightBGM;
                        }
                        else
                        {
                            SetColors("Dusk");
                            audio.clip = duskBGM;
                        }
                    }
                    else
                    {
                        SetColors("Day");
                        audio.clip = dayBGM;
                    }
                }
                else
                {
                    SetColors("Dawn");
                    audio.clip = dawnBGM;
                }
            }
            else
            {
                SetColors("Night");
            }
            audio.Play();
            tapPanel.AddEventTrigger(LoadLevelMenu);

        }

        [Button]
        public void SetColors(string setName)
        {
            var set = mainMenuColors.Sets.Find(s => s.SetName == setName);
            if (set != null)
            {
                partTimer.color = set.PartTimer;
                sandwicher.color = set.Sandwicher;
                sky.color = set.Sky;
                tapToStart.color = set.Start;
                starsFarAway.SetActive(set.ShowStarsFarAway);
                starsNear.SetActive(set.ShowStarsNear);
                foreach (var cloud in clouds)
                    cloud.color = set.Clouds;
            }
        }

        public void LoadLevelMenu()
        {
            var transition = Instantiate(gameCache.LevelTransition);
            transition.OnLoad += () => SceneManager.LoadScene(gameCache.LevelMenu.SceneName);
        }
    }
}
