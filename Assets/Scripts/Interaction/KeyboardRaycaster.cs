﻿using CellexalVR.General;
using CellexalVR.Interaction;
using UnityEngine;
namespace CurvedVRKeyboard
{

    public class KeyboardRaycaster : KeyboardComponent
    {

        //------Raycasting----
        [SerializeField, HideInInspector]
        private Transform raycastingSource;

        [SerializeField, HideInInspector]
        private GameObject target;
        public ReferenceManager referenceManager;
        private float rayLength;
        private Ray ray;
        private RaycastHit hit;
        private LayerMask layer;
        private float minRaylengthMultipler = 1.5f;
        //---interactedKeys---
        private KeyboardStatus keyboardStatus;
        private KeyboardItem keyItemCurrent;
        private SteamVR_TrackedObject rightController;
        private SteamVR_Controller.Device device;
        private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
        private CurvedVRKeyboard.KeyboardItem lastHitKey = null;

        [SerializeField, HideInInspector]
        private string clickInputName;

        void Start()
        {
            rightController = referenceManager.rightController;
            keyboardStatus = gameObject.GetComponent<KeyboardStatus>();
            keyboardStatus.SetVars(referenceManager);
            int layerNumber = gameObject.layer;
            layer = 1 << layerNumber;
        }

        void Update()
        {
            device = SteamVR_Controller.Input((int)rightController.index);
            // * sum of all scales so keys are never to far
            rayLength = Vector3.Distance(raycastingSource.position, target.transform.position) * (minRaylengthMultipler +
                                                                                                  (Mathf.Abs(target.transform.lossyScale.x) + Mathf.Abs(target.transform.lossyScale.y) + Mathf.Abs(target.transform.lossyScale.z)));
            if ((referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.Keyboard)
                || referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.TwoLasers
                || this.gameObject.name == "WebKeyboard" 
                || this.gameObject.name == "FolderKeyboard")
            {
                RayCastKeyboard();
            }
        }

        /// <summary>
        /// Check if camera is pointing at any key.
        /// If it does changes state of key
        /// </summary>
        private void RayCastKeyboard()
        {
            raycastingSource = referenceManager.rightLaser.transform;
            ray = new Ray(raycastingSource.position, raycastingSource.forward);
            if (Physics.Raycast(ray, out hit, rayLength, layer))
            {         // If any key was hit
                //print(hit.collider.transform.gameObject.name);
                KeyboardItem focusedKeyItem = hit.collider.transform.gameObject.GetComponent<KeyboardItem>();
                if (focusedKeyItem != null)
                {         // Hit may occur on item without script
                    ChangeCurrentKeyItem(focusedKeyItem);
                    keyItemCurrent.Hovering();
                    lastHitKey = keyItemCurrent;
#if !UNITY_HAS_GOOGLEVR
                    if (device.GetPressDown(triggerButton))
                    {        // If key clicked
#else
			if(GvrController.TouchDown) {
#endif
                        keyItemCurrent.Click();
                        keyboardStatus.HandleClick(keyItemCurrent);
                        referenceManager.gameManager.InformKeyClicked(keyItemCurrent);
                    }
                }
                if (focusedKeyItem == null && lastHitKey != null)
                {
                    lastHitKey.StopHovering();
                }
            }
            else if (keyItemCurrent != null)
            {        // If no target hit and lost focus on item
                ChangeCurrentKeyItem(null);
            }
        }

        private void ChangeCurrentKeyItem(KeyboardItem key)
        {
            if (keyItemCurrent != null)
            {
                keyItemCurrent.StopHovering();
            }
            keyItemCurrent = key;
        }

        //---Setters---
        public void SetRayLength(float rayLength)
        {
            this.rayLength = rayLength;
        }

        public void SetRaycastingTransform(Transform raycastingSource)
        {
            this.raycastingSource = raycastingSource;
        }

        public void SetClickButton(string clickInputName)
        {
            this.clickInputName = clickInputName;
        }

        public void SetTarget(GameObject target)
        {
            this.target = target;
        }
    }
}