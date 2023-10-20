using CellexalVR.AnalysisLogic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.VFX;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class ToggleMoveSelectedPointsButton : CellexalButton
    {
        protected override string Description => "Toggle Move Selected Points";

        private bool toggle;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void Click()
        {
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<RaycastSystem>().move = !toggle;
            var pc = PointCloudGenerator.instance.pointClouds[0];
            if (!toggle)
            {
                Color[] cols = new Color[pc.positionTextureMap.width * pc.positionTextureMap.height];
                pc.positionTextureMap.SetPixels(cols);
                //pc.GetComponent<VisualEffect>().SetTexture("PositionTextureMap", blackTex);
            }
            else
            {
                Color[] cols = pc.orgPositionTextureMap.GetPixels();
                pc.positionTextureMap.SetPixels(cols);
            }
            pc.positionTextureMap.Apply(false);
            pc.GetComponent<VisualEffect>().SetVector3("CullingCubeSize", Vector3.one * (toggle ? 1.02f : 50f));
            toggle = !toggle;
        }
    }
}