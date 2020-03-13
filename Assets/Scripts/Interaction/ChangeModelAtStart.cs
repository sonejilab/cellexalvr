using CellexalVR.General;
using System.Collections;
using UnityEngine;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Changes the right controller's model at the start of the program, this is needed because the controllermodelswitcher's gameobject is inactive so it can't run a coroutine
    /// </summary>
    internal class ChangeModelAtStart : MonoBehaviour
    {
        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                var referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
                VRTK.VRTK_SDKManager sdkManager = gameObject.GetComponent<VRTK.VRTK_SDKManager>();
                sdkManager.scriptAliasLeftController = referenceManager.leftControllerScriptAlias;
                sdkManager.scriptAliasRightController = referenceManager.rightControllerScriptAlias;
            }
        }

        public ControllerModelSwitcher modelSwitcher;

        private void Start()
        {
            if (!CrossSceneInformation.Spectator)
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