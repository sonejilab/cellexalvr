using System;
using UnityEngine;
using Valve.VR.Extras;
using Valve.VR.InteractionSystem;

namespace CellexalVR.Interaction
{
    [RequireComponent(typeof(SteamVR_LaserPointer))]
    public class PointerGrab : MonoBehaviour
    {
        private SteamVR_LaserPointer laserPointer;
        private Hand hand;
        private Transform previousParent;

        private void Start()
        {
            laserPointer = GetComponent<SteamVR_LaserPointer>();
            hand = GetComponent<Hand>();
            laserPointer.PointerIn += OnPointerIn;
            laserPointer.PointerOut += OnPointerOut;
            laserPointer.PointerGrab += OnPointerGrab;
            laserPointer.PointerUnGrab += OnPointerUnGrab;
        }

        private void AttachObjectToPointer(Transform transform)
        {
            previousParent = transform.parent;
            transform.parent = hand.transform;
        }

        private void DetachObjectFromPointer(Transform transform)
        {
            transform.parent = previousParent;
        }

        private void OnPointerGrab(object sender, PointerEventArgs e)
        {
            if (!laserPointer.enabled) return;
            if (e.target.GetComponent<Interactable>())
            {
                AttachObjectToPointer(e.target);
                // hand.AttachObject(e.target.gameObject, GrabTypes.Grip);
            }
        }

        private void OnPointerUnGrab(object sender, PointerEventArgs e)
        {
            if (!laserPointer.enabled) return;
            DetachObjectFromPointer(e.target);
            // hand.DetachObject(e.target.gameObject);
        }

        private void OnPointerOut(object sender, PointerEventArgs e)
        {
        }

        private void OnPointerIn(object sender, PointerEventArgs e)
        {
        }
    }
}