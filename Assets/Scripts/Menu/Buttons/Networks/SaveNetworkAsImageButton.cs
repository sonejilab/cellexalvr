﻿using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.Networks
{
    /// <summary>
    /// For reporting function. Network is saved as image to the user specific folder. If user wants to create a report
    /// the image is included in it.
    /// </summary>
    public class SaveNetworkAsImageButton : CellexalButton
    {
        public NetworkCenter parent;
        public Sprite doneTex;

        protected override string Description
        {
            get { return "Save this network as an image"; }
        }

        protected override void Awake()
        {
            base.Awake();
            //CellexalEvents.NetworkEnlarged.AddListener(TurnOn);
            //CellexalEvents.NetworkUnEnlarged.AddListener(TurnOff);
            TurnOff();
        }

        public override void Click()
        {
            parent.SaveNetworkAsImage();
            ReferenceManager.instance.rightController.SendHapticImpulse(0.8f, 0.3f);
        }

        public void FinishedButton()
        {
            spriteRenderer.sprite = doneTex;
        }

        private void TurnOn()
        {
            SetButtonActivated(true);
        }

        private void TurnOff()
        {
            SetButtonActivated(false);
        }
    }

}