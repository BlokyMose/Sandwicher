using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace Sandwicher
{
    [CreateAssetMenu(fileName = "Cus_", menuName = "SO/Customer Profile")]
    public class CustomerProfile : ScriptableObject
    {
        [SerializeField]
        string customerName;

        [SerializeField]
        string aliasName;

        [SerializeField, PreviewField]
        Sprite icon;

        [SerializeField]
        SpriteLibraryAsset sLib;

        [SerializeField]
        Tolerance waitTolerance;

        [SerializeField]
        Tolerance priceTolerance;


        public string DisplayName { get => customerName + " \"" +aliasName+"\""; }
        public Sprite Icon { get => icon; }
        public SpriteLibraryAsset SLib { get => sLib; }
        public Tolerance WaitTolerance { get => waitTolerance; }
        public Tolerance PriceTolerance { get => priceTolerance; }
    }
}
