using Assets.Scripts.SceneObjects;
using CellexalVR.General;
using CellexalVR.Tools;
using System.Collections.Generic;
using UnityEngine;
namespace CellexalVR.Menu.Buttons.Tools
{
    /// <summary>
    /// Represents the button that spawns a culling cube. A maximum of two can be spawned.
    /// </summary>
    public class CullingCubeButton : CellexalButton
    {
        public GameObject cullingCubePrefab;
        public bool remove;
        public CullingCubeButton otherButton;

        private int counter;
        protected override string Description
        {
            get { return remove ?  "Remove Culling Cube" : "Spawn Culling Cube"; }
        }

        private void Start()
        {
            if (remove)
                SetButtonActivated(false);
            CellexalEvents.CullingCubeSpawned.AddListener(() => counter++);
            CellexalEvents.CullingCubeRemoved.AddListener(() => counter--);
        }

        public override void Click()
        {
            if (remove)
            {
                GameObject cubeToDestroy = GameObject.Find("CullingCube" + counter);
                Destroy(cubeToDestroy);
                otherButton.SetButtonActivated(true);
                CellexalEvents.CullingCubeRemoved.Invoke();
                if (counter == 0)
                    SetButtonActivated(false);
            }
            else
            {
                GameObject cube = Instantiate(cullingCubePrefab);

                CellexalEvents.CullingCubeSpawned.Invoke();
                cube.GetComponent<CullingCube>().boxNr = counter;
                cube.gameObject.name = "CullingCube" + counter;
                otherButton.SetButtonActivated(true);
                if (counter == 2)
                    SetButtonActivated(false);
            }

        }
    }
}
