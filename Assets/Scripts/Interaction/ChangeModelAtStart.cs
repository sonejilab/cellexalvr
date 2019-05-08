using CellexalVR.General;
using System.Collections;
using UnityEngine;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Changes the right controller's model at the start of the program, this is needed because the controllermodelswitcher's gameobject is inactive so it can't run a coroutine
    /// </summary>
    class ChangeModelAtStart : MonoBehaviour
    {

        public ControllerModelSwitcher modelSwitcher;

        private void Start()
        {
            StartCoroutine(ChangeModel());
        }

        IEnumerator ChangeModel()
        {
            while (!modelSwitcher.Ready())
            {
                yield return new WaitForEndOfFrame();
            }
            modelSwitcher.SetMeshes();
            CellexalEvents.ControllersInitiated.Invoke();
        }
    }

}