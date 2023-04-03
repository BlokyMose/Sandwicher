using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandwicher
{
    [CreateAssetMenu(fileName ="Food_", menuName = "SO/Food Category")]
    public class FoodCategory : ScriptableObject
    {
        [SerializeField]
        string foodName;

        public string FoodName => foodName;
    }
}
