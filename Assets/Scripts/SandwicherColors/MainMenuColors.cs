using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandwicher
{
    [CreateAssetMenu(fileName ="MainMenuColors_", menuName ="SO/Main Menu Colors")]
    public class MainMenuColors : ScriptableObject
    {
        [Serializable]
        public class Set
        {
            [SerializeField] string setName;
            [SerializeField] Color partTimer;
            [SerializeField] Color sandwicher;
            [SerializeField] Color start;
            [SerializeField] Color sky;
            [SerializeField] bool showStarsFarAway; 
            [SerializeField] bool showStarsNear;
            [SerializeField] Color clouds;
            public string SetName { get => setName; }
            public Color PartTimer { get => partTimer; }
            public Color Sandwicher { get => sandwicher; }
            public Color Sky { get => sky; }
            public Color Start { get => start; }
            public bool ShowStarsFarAway { get => showStarsFarAway; }
            public bool ShowStarsNear { get => showStarsNear; }
            public Color Clouds { get => clouds; }
        }

        [SerializeField]
        List<Set> sets = new();

        public List<Set> Sets { get => sets;  }


    }
}
