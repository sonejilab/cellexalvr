using CellexalVR.Menu.Buttons;
using UnityEngine;

namespace Menu.Buttons.General
{
    public class ScreenshotLayerToggleButton : SliderButton
    {
        public bool toggleAllButton;
        public string layerName;
        protected override string Description => $"Toggle {layerName}";
        protected override void ActionsAfterSliding()
        {
            if (toggleAllButton)
            {
                referenceManager.screenshotCamera.ToggleAllLayers(currentState);
            }
            else
            {
                referenceManager.screenshotCamera.ToggleLayerToCapture(layerName, currentState);
            }
        }
        
        
    }
}