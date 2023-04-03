using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandwicher
{
    [CreateAssetMenu(menuName = "SO/Ingredient/Ingredient", fileName = "Ing_")]
    public class Ingredient : ScriptableObject
    {
        [SerializeField]
        string ingName;

        [SerializeField, PreviewField]
        Sprite icon;

        [SerializeField, PreviewField]
        Sprite sprite;

        [SerializeField]
        IngredientShape shape = IngredientShape.Flat;

        [SerializeField, HorizontalGroup("rounded")]
        bool canBeRounded = false;

        [SerializeField, PreviewField, ShowIf(nameof(CanBeRounded)), HorizontalGroup("rounded"), LabelWidth(.1f)]
        Sprite spriteRounded;

        [SerializeField, ShowIf(nameof(CanBeRounded))]
        IngredientHeight nextSpaceRoundedToFlat;

        [SerializeField]
        int price;

        [SerializeField]
        IngredientHeight height;

        [SerializeField]
        IngredientHeight nextSpace;

        public string IngName => ingName; 
        public Sprite Icon => icon; 
        public Sprite Sprite => sprite; 
        public int Price  => price;
        public IngredientHeight Height { get => height; }
        public IngredientHeight NextSpace { get => nextSpace; }
        public int TotalSpace => height.Height + nextSpace.Height;
           
        public Sprite SpriteRounded { get => spriteRounded; }
        public IngredientShape Shape { get => shape; }
        public bool CanBeRounded { get => canBeRounded;  }
        public IngredientHeight ExtraRoundedHeight { get => nextSpaceRoundedToFlat; }
    }
}
