using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandwicher
{
    [CreateAssetMenu(fileName ="Level_", menuName ="SO/Level")]
    [InlineEditor]
    public class Level : ScriptableObject
    {
        [SerializeField]
        string levelDisplayName;

        [SerializeField, Multiline]
        string description;

        [SerializeField, ReadOnly]
        string sceneName;

        [SerializeField]
        Object scene;

        [Title("Game")]
        [SerializeField]
        int customerCount = 10;

        [SerializeField]
        List<Ingredient> ingredients = new();

        [SerializeField]
        List<Order> orders = new();

        [SerializeField]
        List<CustomerProfile> customerProfiles = new();

        public string LevelDisplayName { get => levelDisplayName; }
        public string Description { get => description; }
        public string SceneName { get => sceneName; }
        public Object Scene { get => scene; }
        public int CustomerCount { get => customerCount; }
        public List<Ingredient> Ingredients { get => ingredients; }
        public List<Order> Orders { get => orders; }
        public List<CustomerProfile> CustomerProfiles { get => customerProfiles; }

        public List<Recipe> GetUniqueRecipes()
        {
            var recipes = new List<Recipe>();
            foreach (var order in orders)
                if (!recipes.Contains(order.Recipe))
                    recipes.Add(order.Recipe);
            return recipes;
        }

        void OnValidate()
        {
            if (scene != null) sceneName = scene.name;
        }
    }
}
