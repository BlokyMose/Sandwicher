using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandwicher
{
    [RequireComponent(typeof(Animator))]
    public class LevelTransition : MonoBehaviour
    {
        Animator animator;
        int boo_show;
        public Action OnLoad;

        void Awake()
        {
            DontDestroyOnLoad(this);
            animator = GetComponent<Animator>();
            boo_show = Animator.StringToHash(nameof(boo_show));
        }

        public void LoadLevel()
        {
            OnLoad?.Invoke();
        }

        public void Hide()
        {
            animator.SetBool(boo_show, false);
        }

        public void DestroySelf() => Destroy(gameObject);

        public static void HideAllTransition()
        {
            foreach (var transition in FindObjectsOfType<LevelTransition>())
                transition.Hide();
        }
    }
}
