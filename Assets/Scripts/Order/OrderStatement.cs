using Encore.Utility;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtility;

namespace Sandwicher
{
    [CreateAssetMenu(menuName ="SO/Order Statement/Base Statement")]
    public class OrderStatement : ScriptableObject
    {
        [Serializable]
        public class Phrase
        {
            [HorizontalGroup]
            [SerializeField, LabelWidth(.1f)]
            OrderPhrase type;

            [HorizontalGroup]
            [SerializeField, LabelWidth(.1f), ShowIf("@"+nameof(type)+"==OrderPhrase.Text")]
            string text;

            public OrderPhrase Type { get => type; }
            
            public string Text { get => text; }

            public string GetFilledText(Order order, OrderStatement orderStatement)
            {
                var filledText = "";
                switch (type)
                {
                    case OrderPhrase.Text:
                        filledText = text;
                        break;
                    case OrderPhrase.Ingredients:
                        var toMentionIngredients = new List<Ingredient>();
                        foreach (var ing in order.Recipe.IngredientPositions)
                            //if (ing.IsMentioned)
                                toMentionIngredients.Add(ing.Ingredient);

                        for (int i = 0; i < toMentionIngredients.Count-1; i++)
                            filledText += toMentionIngredients[i].IngName.ToLower() + orderStatement.IngSeparator;

                        filledText += orderStatement.IngLastItemPrefix + orderStatement.PhraseSeparator;
                        filledText += toMentionIngredients.GetLast().IngName.ToLower() + orderStatement.IngLastItemSuffix;
                        break;
                    case OrderPhrase.GeneralName:
                        filledText = order.Recipe.Category.FoodName;
                        break;                    
                    
                    case OrderPhrase.RecipeName:
                        filledText = $"<color=#{orderStatement.RecipeNameColor.ToHex()}>{order.Recipe.RecipeName}</color>";
                        break;
                }

                return filledText;
            }
        }

        [SerializeField]
        Color recipeNameColor = Color.black;

        [SerializeField]
        string phraseSeparator = " ";

        [SerializeField]
        string ingSeparator = ", ";

        [SerializeField]
        string ingLastItemPrefix = "and";

        [SerializeField]
        string ingLastItemSuffix = ", ";

        [SerializeField]
        List<Phrase> phrases = new();

        public List<Phrase> Phrases { get => phrases; }
        public string IngSeparator { get => ingSeparator; }
        public string IngLastItemPrefix { get => ingLastItemPrefix; }
        public string IngLastItemSuffix { get => ingLastItemSuffix; }
        public string PhraseSeparator { get => phraseSeparator;}
        public Color RecipeNameColor { get => recipeNameColor; }
    }
}
