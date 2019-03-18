using CellexalVR.AnalysisObjects;
using System;
using UnityEngine;
namespace CellexalVR.Menu.Buttons.Networks
{
    /// <summary>
    /// Recalculates the positioning of the nodes and lines.
    /// </summary>
    public class RecalculateNetworkLayoutButton : CellexalButton
    {
        protected override string Description
        {
            get { return "Recalculate layout - " + layout.ToString(); }
        }
        public NetworkCenter center;
        public NetworkCenter.Layout layout;

        protected override void Awake()
        {
            base.Awake();
            //CellexalEvents.NetworkEnlarged.AddListener(TurnOn);
            //CellexalEvents.NetworkUnEnlarged.AddListener(TurnOff);
            TurnOff();
        }

        public override void Click()
        {
            center.CalculateLayout(layout);
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