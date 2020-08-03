using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using CellexalVR.General;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using VRTK;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// General slider script that can be used to change a value that has a max and min value.
    /// One or more function(s) being called when the slider handle is grabbed and one or more when the handle is released;
    /// </summary>
    public class VRSlider : MonoBehaviour
    {
        public GameObject handle;
        public GameObject attachPoint;
        public GameObject fillArea;
        public TextMeshProUGUI sliderValueText;
        public TextMeshProUGUI header;
        public string headerText;
        public float minValue;
        public float maxValue;
        public float startValue;
        public ReferenceManager referenceManager;

        public enum SliderType
        {
            VelocityParticleSize,
            PDFCurvature,
            PDFRadius,
            PDFWidth,
            PDFHeight
        };

        public SliderType sliderType;
        
        public float Value
        {
            get => value;
            set => this.value = value;
        }

        public UnityEvent OnHandleRelease;
        
        private float value;

        [Serializable]
        public class OnHandleGrabbedEvent : UnityEvent<float>
        {
        };

        public OnHandleGrabbedEvent OnHandleGrabbed = new OnHandleGrabbedEvent();

        private Vector3 handleStartPosition;
        private float xMaxPos = 100;
        private float xMinPos = 0;
        private VRTK_InteractableObject handleInteractable;

        // Start is called before the first frame update
        private void Start()
        {
            handleStartPosition = handle.transform.localPosition;
            float relativeVal = startValue / maxValue;
            float xValue = xMaxPos * relativeVal;
            handleStartPosition.x = xValue;

            xValue /= xMaxPos;

            handle.transform.localPosition = handleStartPosition;
            handleInteractable = handle.GetComponent<VRTK_InteractableObject>();
            handleInteractable.InteractableObjectUngrabbed += OnRelease;
            header.text = headerText;
            sliderValueText.text = $"{((int) (xValue * 100)).ToString()}%";
            Value = minValue + xValue * (maxValue - minValue);
        }

        private void OnValidate()
        {
            handleStartPosition = handle.transform.localPosition;
            float relativeVal = startValue / maxValue;
            float xValue = xMaxPos * relativeVal;
            handleStartPosition.x = xValue;
            xValue /= xMaxPos;
            Value = minValue + xValue * (maxValue - minValue);
            handle.transform.localPosition = handleStartPosition;
            Vector3 fillAreaScale = fillArea.transform.localScale;
            fillAreaScale.x = 0.001f * xValue;
            fillArea.transform.localScale = fillAreaScale;
            sliderValueText.text = $"{((int) (xValue * 100)).ToString()}%";
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }

        private void Update()
        {
            float xValue = handle.transform.localPosition.x;

            if (xValue >= 100)
            {
                xValue = 100;
            }

            if (xValue <= 0)
            {
                xValue = 0;
            }

            if (handleInteractable.IsGrabbed())
            {
                OnHandleGrabbed.Invoke(Value);
            }

            handle.transform.localPosition = new Vector3(xValue, 5, 0);
        }

        /// <summary>
        /// The position of the handle goes from 0 - 100.
        /// This is used to determine the value which depends on the min and max set for the slider.
        /// </summary>
        public void UpdateSliderValue()
        {
            float xValue = handle.transform.localPosition.x;
            if (xValue >= 100)
            {
                xValue = 100;
            }

            if (xValue <= 0)
            {
                xValue = 0;
            }

            handle.transform.localPosition = new Vector3(xValue, 5, 0);

            xValue /= xMaxPos;
            Value = minValue + xValue * (maxValue - minValue);
            sliderValueText.text = $"{((int) (xValue * 100)).ToString()}%";
            Vector3 fillAreaScale = fillArea.transform.localScale;
            fillAreaScale.x = 0.001f * xValue;
            fillArea.transform.localScale = fillAreaScale;
        }

        /// <summary>
        /// If slider value is to be updated from outside (e.g. multi user) then update handler first then value.
        /// </summary>
        /// <param name="value"></param>
        public void UpdateSliderValue(float value)
        {
            handle.transform.localPosition = new Vector3(value, 5, 0);
            UpdateSliderValue();
        }

        public void UpdateSliderValueMultiUser()
        {
            referenceManager.multiuserMessageSender.SendMessageUpdateSliderValue(sliderType, handle.transform.localPosition.x);
        }

        // private void OnTriggerEnter(Collider other)
        // {
        //     if (attachPoint == null) return;
        //     attachPoint.SetActive(true);
        //
        // }
        //
        // private void OnTriggerExit(Collider other)
        // {
        //     if (attachPoint == null) return;
        //     attachPoint.SetActive(false);
        // }

        private void OnRelease(object sender, VRTK.InteractableObjectEventArgs e)
        {
            OnHandleRelease.Invoke();
        }
    }
}