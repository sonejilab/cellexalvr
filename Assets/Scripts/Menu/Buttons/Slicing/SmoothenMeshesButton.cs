using CellexalVR.AnalysisLogic;
using CellexalVR.Spatial;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class SmoothenMeshesButton : CellexalButton
    {
        protected override string Description => "Remove Outliers & Smoothen Meshes";



        protected override void Awake()
        {
            base.Awake();
        }

        public override void Click()
        {
            MeshGenerator.instance.SmoothenMeshes();
        }
    }
}