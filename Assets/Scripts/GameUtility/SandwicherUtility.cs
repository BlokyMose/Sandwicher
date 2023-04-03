using Encore.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityUtility;

namespace Sandwicher
{
    public static class SandwicherUtility
    {

        public static void DisplayNum(GOLister lister, int price, Color textColor, SandwicherSymbols symbols, List<string> numNames = null, Sprite defaultSprite = null)
        {
            numNames ??= new() { "num_one", "num_ten", "num_hundred" };

            if (lister.TryGet(numNames.GetAt(0, ""), out var num_one))
            {
                if (defaultSprite == null)
                    num_one.SetActive(false);
                else
                    Set(num_one, textColor, defaultSprite);

            }
            if (lister.TryGet(numNames.GetAt(1,""), out var num_ten))
            {
                if (defaultSprite == null)
                    num_ten.SetActive(false);
                else
                    Set(num_ten, textColor, defaultSprite);
            }
            if (lister.TryGet(numNames.GetAt(2,""), out var num_hundred))
            {
                if (defaultSprite == null)
                    num_hundred.SetActive(false);
                else
                    Set(num_hundred, textColor, defaultSprite);
            }

            var priceString = price.ToString();
            for (int i = priceString.Length - 1; i >= 0; i--)
            {
                if (i == 0 && num_one != null)
                    Set(num_one, textColor, symbols.Nums.GetAt(int.Parse(priceString.Substring(priceString.Length - (i + 1), 1)), 0));

                else if (i == 1 && num_ten != null)
                    Set(num_ten, textColor, symbols.Nums.GetAt(int.Parse(priceString.Substring(priceString.Length - (i + 1), 1)), 0));

                else if (i == 2 && num_hundred != null)
                    Set(num_hundred, textColor, symbols.Nums.GetAt(int.Parse(priceString.Substring(priceString.Length - (i + 1), 1)), 0));
            }

            void Set(GameObject go, Color color, Sprite sprite)
            {
                if (go.TryGetComponent<Image>(out var image))
                {
                    image.color = color;
                    image.sprite = sprite;
                    go.SetActive(true);
                }
            }
        }

        public static void DisplayNumSR(GOLister lister, int price, Color textColor, SandwicherSymbols symbols, List<string> numNames = null, Sprite defaultSprite = null)
        {
            numNames ??= new() { "num_one", "num_ten", "num_hundred" };

            if (lister.TryGet(numNames.GetAt(0, ""), out var num_one))
            {
                if (defaultSprite == null)
                    num_one.SetActive(false);
                else
                    Set(num_one, textColor, defaultSprite);

            }
            if (lister.TryGet(numNames.GetAt(1, ""), out var num_ten))
            {
                if (defaultSprite == null)
                    num_ten.SetActive(false);
                else
                    Set(num_ten, textColor, defaultSprite);
            }
            if (lister.TryGet(numNames.GetAt(2, ""), out var num_hundred))
            {
                if (defaultSprite == null)
                    num_hundred.SetActive(false);
                else
                    Set(num_hundred, textColor, defaultSprite);
            }

            var priceString = price.ToString();
            for (int i = priceString.Length - 1; i >= 0; i--)
            {
                if (i == 0 && num_one != null)
                    Set(num_one, textColor, symbols.Nums.GetAt(int.Parse(priceString.Substring(priceString.Length - (i + 1), 1)), 0));

                else if (i == 1 && num_ten != null)
                    Set(num_ten, textColor, symbols.Nums.GetAt(int.Parse(priceString.Substring(priceString.Length - (i + 1), 1)), 0));

                else if (i == 2 && num_hundred != null)
                    Set(num_hundred, textColor, symbols.Nums.GetAt(int.Parse(priceString.Substring(priceString.Length - (i + 1), 1)), 0));
            }

            void Set(GameObject go, Color color, Sprite sprite)
            {
                if (go.TryGetComponent<SpriteRenderer>(out var SR))
                {
                    SR.color = color;
                    SR.sprite = sprite;
                    go.SetActive(true);
                }
            }
        }

    }
}
