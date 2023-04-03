using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static SandwicherInputActions;

namespace Sandwicher
{
    public class PlayerBrain : Brain, IWorldActions
    {
        private void Awake()
        {
            SandwicherInputActions inputActions = new();
            inputActions.World.SetCallbacks(this);
            inputActions.Enable();
        }

        void OnDestroy()
        {
            
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            OnMoveInput?.Invoke(context.ReadValue<Vector2>());
        }
    }
}
