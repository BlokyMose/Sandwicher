using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sandwicher
{
    [RequireComponent(typeof(Image))]
    public class ImageFlipBook : MonoBehaviour
    {
        [Serializable]
        public class Frame
        {
            public Sprite sprite;
            public float duration = 1f;
        }

        [SerializeField]
        bool isLooping = true;

        [SerializeField]
        List<Frame> frames = new();

        Image image;
        float time = 0f;
        int currentFrameIndex = 0;

        void Awake()
        {
            image = GetComponent<Image>();
            image.sprite = frames[currentFrameIndex].sprite;
        }

        void Update()
        {
            if (time > frames[currentFrameIndex].duration)
            {
                currentFrameIndex = (currentFrameIndex + 1) % frames.Count;
                image.sprite = frames[currentFrameIndex].sprite;
                time = 0f;
            }
            time += Time.deltaTime * Time.timeScale;
        }
    }
}
