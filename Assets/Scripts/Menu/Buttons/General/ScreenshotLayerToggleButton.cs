namespace CellexalVR.Menu.Buttons.General
{
    public class ScreenshotLayerToggleButton : SliderButton
    {
        public bool toggleAllButton;
        public string[] layerNames;
        protected override string Description => $"Toggle {string.Join(", ", layerNames)}";
        protected override void ActionsAfterSliding()
        {
            if (toggleAllButton)
            {
                referenceManager.screenshotCamera.ToggleAllLayers(currentState);
            }
            else
            {
                foreach (string layerName in layerNames)
                {
                    if (layerName == "Background")
                    {
                        referenceManager.screenshotCamera.ToggleBackground(currentState);
                    }
                    else
                    {
                        referenceManager.screenshotCamera.ToggleLayerToCapture(layerName, currentState);
                    }
                }
            }
        }
    }
}
