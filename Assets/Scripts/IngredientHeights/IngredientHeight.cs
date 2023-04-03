using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandwicher
{
    [CreateAssetMenu(menuName = "SO/Ingredient Height", fileName = "IngHeight_")]
    public class IngredientHeight : ScriptableObject
    {
        [SerializeField]
        int height = 0;

        public int Height => height;
    }
}
