using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandwicher
{
    [CreateAssetMenu(menuName ="SO/Order/Order", fileName ="Order_")]
    public class Order : ScriptableObject
    {
        [SerializeField]
        string orderName;

        [SerializeField]
        Recipe recipe;

        [SerializeField]
        OrderStatement statement;

        public string OrderName { get => orderName;  }
        public OrderStatement Statement { get => statement; }
        public Recipe Recipe { get => recipe; }

        public string GetStatementText()
        {
            var statementText = "";
            foreach (var phrase in statement.Phrases)
                statementText += phrase.GetFilledText(this, statement) + statement.PhraseSeparator;

            return statementText;
        }

        public bool TryGetIngredientByIndex(OrderIngIndex orderIngIndex, out Ingredient foundIngredient)
        {
            foreach (var orderIng in Recipe.IngredientPositions)
                if (orderIng.Index == orderIngIndex)
                {
                    foundIngredient = orderIng.Ingredient;
                    return true;
                }

            foundIngredient = null;
            return false;
        }

        public bool HasIngredient(Ingredient ingredient)
        {
            foreach (var orderIng in Recipe.IngredientPositions)
                if (orderIng.Ingredient == ingredient) return true;

            return false;
        }
    }
}
