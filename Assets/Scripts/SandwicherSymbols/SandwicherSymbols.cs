using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandwicher
{
    [CreateAssetMenu(menuName = "SO/Symbols",fileName = "Symbols_")]
    public class SandwicherSymbols : ScriptableObject
    {
        [SerializeField]
        List<Sprite> nums = new();

        [SerializeField, PreviewField]
        Sprite cash;

        [SerializeField, PreviewField]
        Sprite plus;

        [SerializeField, PreviewField]
        Sprite minus;

        [Header("Rating")]
        [SerializeField, PreviewField]
        Sprite ratingSad;

        [SerializeField, PreviewField]
        Sprite ratingOk;

        [SerializeField, PreviewField]
        Sprite ratingHappy;

        [Header("Sound")]
        [SerializeField, PreviewField]
        Sprite soundMute;        
        [SerializeField, PreviewField]
        Sprite soundMuteBottom;

        [SerializeField, PreviewField]
        Sprite soundQuiet;        
        [SerializeField, PreviewField]
        Sprite soundQuietBottom;

        [SerializeField, PreviewField]
        Sprite soundMedium;
        [SerializeField, PreviewField]
        Sprite soundMediumBottom;
        
        [SerializeField, PreviewField]
        Sprite soundFull;
        [SerializeField, PreviewField]
        Sprite soundFullBottom;

        public List<Sprite> Nums { get => nums; }
        public Sprite Cash { get => cash; }
        public Sprite RatingSad { get => ratingSad; }
        public Sprite RatingOk { get => ratingOk; }
        public Sprite RatingHappy { get => ratingHappy; }
        public Sprite Plus { get => plus; }
        public Sprite Minus { get => minus; }
        public Sprite SoundMute { get => soundMute; }
        public Sprite SoundMuteBottom { get => soundMuteBottom; }
        public Sprite SoundQuiet { get => soundQuiet; }
        public Sprite SoundQuietBottom { get => soundQuietBottom; }
        public Sprite SoundMedium { get => soundMedium; }
        public Sprite SoundMediumBottom { get => soundMediumBottom; }
        public Sprite SoundFull { get => soundFull; }
        public Sprite SoundFullBottom { get => soundFullBottom; }
    }
}
