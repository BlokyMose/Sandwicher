using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandwicher
{
    [CreateAssetMenu(fileName = "Rec_", menuName ="SO/Recipe")]
    public class Recipe : ScriptableObject
    {
        [Serializable]
        public class IngredientPosition
        {
            [HorizontalGroup(150)]
            [SerializeField, LabelWidth(.1f)]
            Ingredient ingredient;

            [HorizontalGroup]
            [SerializeField, LabelWidth(.1f)]
            OrderIngIndex index;

            public Ingredient Ingredient { get => ingredient; }
            public OrderIngIndex Index { get => index; }

            public IngredientPosition(Ingredient ingredient, OrderIngIndex index)
            {
                this.ingredient = ingredient;
                this.index = index;
            }
        }

        [SerializeField]
        string recipeName;

        [SerializeField]
        string desc;

        [SerializeField]
        FoodCategory category;

        [SerializeField]
        List<IngredientPosition> ingredientPositions = new();

        public string RecipeName { get => recipeName; }
        public string Desc { get => desc; }
        public FoodCategory Category { get => category; }
        public List<IngredientPosition> IngredientPositions { get => ingredientPositions; }

        public List<Ingredient> Ingredients
        {
            get
            {
                var ingredients = new List<Ingredient>();
                foreach (var ing in ingredientPositions)
                    ingredients.Add(ing.Ingredient);
                return ingredients;
            }
        }
    }
}
