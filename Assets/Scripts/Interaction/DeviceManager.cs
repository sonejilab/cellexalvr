using UnityEngine;
using System.Collections;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using CellexalVR.General;

namespace CellexalVR.Interaction
{
    public class DeviceManager : MonoBehaviour
    {
        public static DeviceManager instance;
        public bool RightTriggerPressed { get; set; }
        public bool LeftTriggerPressed { get; set; }


        private InputDevice rightHand;
        private InputDevice leftHand;


        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            rightHand = GetDevice(XRNode.RightHand);
            leftHand = GetDevice(XRNode.LeftHand);
        }

        private void Update()
        {
            if (rightHand.characteristics == InputDeviceCharacteristics.None)
            {
                rightHand = GetDevice(XRNode.RightHand);
            }
            if (leftHand.characteristics == InputDeviceCharacteristics.None)
            {
                leftHand = GetDevice(XRNode.LeftHand);
            }
            GetTriggerState(rightHand);
            GetTriggerState(leftHand);
        }

        private InputDevice GetDevice(XRNode node)
        {
            var device = new InputDevice();
            var devices = new List<InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesAtXRNode(node,
                devices);
            if (devices.Count == 1)
            {
                device = devices[0];
                if (node == XRNode.RightHand) rightHand = device;
                if (node == XRNode.LeftHand) leftHand = device;
                //Debug.Log($"Device name '{device.name}' with role '{device.role.ToString()}'");
            }
            else if (devices.Count > 1)
            {
                Debug.Log($"Found more than one '{device.characteristics}'!");
                device = devices[0];
            }

            return device;
        }


        // return 0: Trigger is not pressed. 1: Trigger pressed first. 2 : Trigger is being helled down. 3: Trigger was pulled up.
        private void GetTriggerState(InputDevice device)
        {
            bool triggerPressed;
            if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed) && triggerPressed)
            {
                if (device == rightHand)
                {
                    if (!RightTriggerPressed)
                    {
                        RightTriggerPressed = true;
                        CellexalEvents.RightTriggerClick.Invoke();
                    }
                    CellexalEvents.RightTriggerPressed.Invoke();
                }
                else if (device == leftHand)
                {
                    if (!LeftTriggerPressed)
                    {
                        LeftTriggerPressed = true;
                        CellexalEvents.LeftTriggerClick.Invoke();
                    }
                    CellexalEvents.LeftTriggerPressed.Invoke();
                }
            }

            else if (device == rightHand)
            {
                if (RightTriggerPressed)
                {
                    RightTriggerPressed = false;
                    CellexalEvents.RightTriggerUp.Invoke();
                }
            }
            else if (device == leftHand)
            {
                if (LeftTriggerPressed)
                {
                    LeftTriggerPressed = false;
                    CellexalEvents.LeftTriggerUp.Invoke();
                }
            }


        }

    }
}