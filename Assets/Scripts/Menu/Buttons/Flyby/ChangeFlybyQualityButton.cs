
using CellexalVR.Menu.SubMenus;
using TMPro;

namespace CellexalVR.Menu.Buttons.Flyby
{
    /// <summary>
    /// Button in the flyby menu that changes the flyby quality when clicked.
    /// </summary>
    public class ChangeFlybyQualityButton : CellexalButton
    {
        public FlybyMenu flybyMenu;
        public TextMeshPro qualityText;

        protected override string Description => "Change render quality";

        public override void Click()
        {
            switch (flybyMenu.RenderQuality)
            {
                case FlybyMenu.FlybyRenderQuality.q1080p:
                    flybyMenu.RenderQuality = FlybyMenu.FlybyRenderQuality.q720p;
                    qualityText.text = "720p";
                    break;
                case FlybyMenu.FlybyRenderQuality.q720p:
                    flybyMenu.RenderQuality = FlybyMenu.FlybyRenderQuality.q480p;
                    qualityText.text = "480p";
                    break;
                case FlybyMenu.FlybyRenderQuality.q480p:
                    flybyMenu.RenderQuality = FlybyMenu.FlybyRenderQuality.q1080p;
                    qualityText.text = "1080p";
                    break;
            }
        }
    }
}
