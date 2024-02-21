using CellexalVR.AnalysisObjects;
using CellexalVR.General;

namespace CellexalVR.Menu.Buttons.Networks
{
    /// <summary>
    /// Saves the correlation network to a txt file in the output folder.
    /// </summary>
    public class SaveNetworkAsTextFileButton : CellexalButton
    {
        public NetworkCenter parent;

        protected override string Description
        {
            get { return "Save this network as a text file"; }
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
            parent.SaveNetworkAsTextFile();
            ReferenceManager.instance.rightController.SendHapticImpulse(0.8f, 0.3f);
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