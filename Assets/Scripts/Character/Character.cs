using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandwicher
{
    [RequireComponent(typeof(Animator))]
    public class Character : MonoBehaviour
    {
        [SerializeField]
        float speed = 1;

        Animator animator;
        int int_direction; // 0: down, 1: up, 2: right, 3: left
        int int_action;  // 0: idle; 1: walk
        Vector2 direction;

        void Awake()
        {
            animator = GetComponent<Animator>();
            int_direction = Animator.StringToHash(nameof(int_direction));
            int_action = Animator.StringToHash(nameof(int_action));

            if (TryGetComponent<Brain>(out var brain))
            {
                brain.OnMoveInput += Move;
            }
        }

        void OnDestroy()
        {
            if (TryGetComponent<Brain>(out var brain))
            {
                brain.OnMoveInput -= Move;
            }
        }

        void Update()
        {
            transform.Translate(speed/5f * Time.deltaTime * direction);
        }

        public void Move(Vector2 dir)
        {
            Move(dir.ToMoveDir());
        }

        public void Move(MoveDir dir)
        {
            if (dir != MoveDir.Stop)
                animator.SetInteger(int_direction, (int)dir);
            animator.SetInteger(int_action, dir == MoveDir.Stop ? (int)CharAction.Idle: (int)CharAction.Walk);
            direction = dir.ToVector();
        }
    }
}
