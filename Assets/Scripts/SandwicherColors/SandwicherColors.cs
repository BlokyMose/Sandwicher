using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandwicher
{
    [CreateAssetMenu(menuName = "SO/Colors",fileName = "Colors_")]
    public class SandwicherColors : ScriptableObject
    {
        [SerializeField]
        Color green;

        [SerializeField]
        Color brightGreen;

        [SerializeField]
        Color darkGreen;

        [SerializeField]
        Color darkestGreen;

        [SerializeField]
        Color black;

        [SerializeField]
        Color bg;

        public Color Green { get => green; }
        public Color BrightGreen { get => brightGreen; }
        public Color DarkGreen { get => darkGreen; }
        public Color DarkestGreen { get => darkestGreen; }
        public Color Black { get => black; }
        public Color BG { get => bg; }
    }
}
