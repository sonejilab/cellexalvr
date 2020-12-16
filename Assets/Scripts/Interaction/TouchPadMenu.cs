using System;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

namespace CellexalVR.Interaction
{
    public class TouchPadMenu : MonoBehaviour
    {
        public SteamVR_Action_Boolean touchPadPress;
        public SteamVR_Input_Sources inputSource;

        //events
        public UnityEvent onRightClick;
        public UnityEvent onLeftClick;
        public UnityEvent onUpClick;
        public UnityEvent onDownClick;
        
        private Vector2 touchPadPosition;

        // private void Update()
        // {
        //     if (touchPadPress.GetStateDown(inputSource))
        //     {
        //         // touchPadPosition = SteamVR_Input.GetVector2("TouchPadPosition", inputSource);
        //         // HandleInput(touchPadPosition);
        //     }
        // }

        private void HandleInput(Vector2 position)
        {
            if (position.y > 0.5f)
            {
                onUpClick.Invoke();
            }
            
            else if (position.y < -0.5f)
            {
                onDownClick.Invoke();
            }

            else if (position.x > 0.5f)
            {
                onRightClick.Invoke();
            }

            else if (position.x < -0.5f)
            {
                onLeftClick.Invoke();
            }
        }

    }
}