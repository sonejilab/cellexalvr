using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using VRTK;

namespace CellexalVR.Interaction
{
    public class VRSlider : MonoBehaviour
    {
        public GameObject handle;
        public GameObject fillArea;
        public TextMeshProUGUI sliderValueText;
        public TextMeshProUGUI header;
        public string headerText;
        public float minValue;
        public float maxValue;
        public float startValue;
        public UnityEvent OnHandleRelease;
        public UnityEvent OnHandleGrabbed;
        [HideInInspector] public float value;

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
            value = minValue + xValue * (maxValue - minValue);
        }

        private void OnValidate()
        {
            handleStartPosition = handle.transform.localPosition;
            float relativeVal = startValue / maxValue;
            float xValue = xMaxPos * relativeVal;
            handleStartPosition.x = xValue;
            xValue /= xMaxPos;
            value = minValue + xValue * (maxValue - minValue);
            handle.transform.localPosition = handleStartPosition;
            Vector3 fillAreaScale = fillArea.transform.localScale;
            fillAreaScale.x = 0.001f * xValue;
            fillArea.transform.localScale = fillAreaScale;
            sliderValueText.text = $"{((int) (xValue * 100)).ToString()}%";
        }

        // Update is called once per frame
        private void Update()
        {
            if (handleInteractable.IsGrabbed())
            {
                OnHandleGrabbed.Invoke();
            }

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
        }

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
            value = minValue + xValue * (maxValue - minValue);
            sliderValueText.text = $"{((int) (xValue * 100)).ToString()}%";
            Vector3 fillAreaScale = fillArea.transform.localScale;
            fillAreaScale.x = 0.001f * xValue;
            fillArea.transform.localScale = fillAreaScale;
        }

        private void OnRelease(object sender, VRTK.InteractableObjectEventArgs e)
        {
            OnHandleRelease.Invoke();
        }
    }
}