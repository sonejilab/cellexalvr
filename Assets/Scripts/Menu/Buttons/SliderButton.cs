using System;
using System.Collections;
using UnityEngine;

namespace CellexalVR.Menu.Buttons
{
    /// <summary>
    /// A button that animates a sliding effect between two states. Once sliding is complete some actions are performed.
    /// Typically a toggle button.
    /// </summary>
    public abstract class SliderButton : CellexalButton
    {
        public Transform rightSide;
        public Transform leftSide;
        public GameObject slider;
        public float slideTime;
        public GameObject background;
        public bool startState;

        protected bool currentState;

        public bool CurrentState
        {
            get => currentState;
            set
            {
                if (value == currentState) return;
                StartCoroutine(SlideToNewState());
            }
        }

        //private void OnValidate()
        //{
        //    if (gameObject.scene.IsValid())
        //    {
        //        slider.transform.localPosition = startState ? rightSide.localPosition : leftSide.localPosition;
        //        slider.transform.localPosition += new Vector3(0, 0, 0f);
        //        currentState = startState;
        //    }
        //}

        private void Start()
        {
            CurrentState = startState;
            //StartCoroutine(SlideToNewState());
            UpdateColors();
        }

        /// <summary>
        /// Actions are in this case not directly on click but first sliding animation. 
        /// </summary>
        public override void Click()
        {
            StartCoroutine(SlideToNewState());
        }

        private IEnumerator SlideToNewState()
        {
            currentState = !currentState;
            float currentTime = 0;
            Vector3 startPosition = slider.transform.localPosition;
            Vector3 targetPosition = currentState ? rightSide.localPosition : leftSide.localPosition;
            targetPosition.z = 0f;
            while (currentTime <= slideTime)
            {
                currentTime += Time.deltaTime;
                float step = currentTime / slideTime;
                slider.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, step);
                yield return null;
            }
            UpdateColors();
            ActionsAfterSliding();
        }

        /// <summary>
        /// Actions to be performed once animation is complete.
        /// </summary>
        protected abstract void ActionsAfterSliding();

        public override void SetHighlighted(bool highlight)
        {
            base.SetHighlighted(highlight);
            slider.GetComponent<Renderer>().material.color = highlight ? (new Color(0.8f, 0.8f, 0.8f)) : Color.gray;
        }

        private void UpdateColors(bool forceState = false)
        {
            foreach (Renderer rend in background.GetComponentsInChildren<Renderer>())
            {
                rend.material.color = currentState || forceState ? new Color(0.37f, 1f, 0.53f) : new Color(0.24f, 0.24f, 0.24f);
            }
        }
    }
}