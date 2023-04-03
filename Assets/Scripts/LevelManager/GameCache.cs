using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandwicher
{
    [InlineEditor]
    [CreateAssetMenu(fileName ="Game_",menuName ="SO/Game Cache")]
    public class GameCache : ScriptableObject
    {
        [Title("Static Levels")]
        [SerializeField]
        Level mainMenu;

        [SerializeField]
        Level levelMenu;

        [SerializeField]
        List<Level> levels = new();

        [SerializeField]
        LevelTransition levelTransition;

        [Title("Cache")]
        public Level GameLevel;

        public Level MainMenu { get => mainMenu; }
        public Level LevelMenu { get => levelMenu; }
        public List<Level> Levels { get => levels; }
        public LevelTransition LevelTransition { get => levelTransition; }
    }
}
