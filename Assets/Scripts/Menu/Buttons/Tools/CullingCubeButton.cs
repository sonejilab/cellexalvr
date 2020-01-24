using CellexalVR.Tools;
using UnityEngine;
namespace CellexalVR.Menu.Buttons.Tools
{
    /// <summary>
    /// Represents the button that toggles the screenshot tool.
    /// </summary>
    public class CullingCubeButton : CellexalButton
    {
        public GameObject cullingCubePrefab;

        protected override string Description
        {
            get { return "Spawn Culling Cube"; }
        }

        public override void Click()
        {
            GameObject cube = Instantiate(cullingCubePrefab);
            if (referenceManager.screenshotCamera.gameObject.activeSelf)
            {
                spriteRenderer.sprite = standardTexture;
            }
            else
            {
                spriteRenderer.sprite = deactivatedTexture;

            }

        }
    }
}
