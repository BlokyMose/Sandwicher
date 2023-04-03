using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandwicher
{
    [CreateAssetMenu(menuName = "SO/Ingredient/Ingredient Composite", fileName = "IngComp_")]
    public class IngredientComposite : Ingredient
    {

        [SerializeField]
        List<Ingredient> ingredients = new();

        public List<Ingredient> Ingredients => ingredients;
    }
}
