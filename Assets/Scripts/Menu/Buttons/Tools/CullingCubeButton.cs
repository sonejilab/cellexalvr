using Assets.Scripts.SceneObjects;
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

        private int counter;
        protected override string Description
        {
            get { return "Spawn Culling Cube"; }
        }

        public override void Click()
        {
            GameObject cube = Instantiate(cullingCubePrefab);
            cube.GetComponent<CullingCube>().boxNr = counter;
            if (referenceManager.screenshotCamera.gameObject.activeSelf)
            {
                spriteRenderer.sprite = standardTexture;
            }
            else
            {
                spriteRenderer.sprite = deactivatedTexture;

            }
            counter++;
            if (counter == 2)
                SetButtonActivated(false);

        }
    }
}
