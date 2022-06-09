using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Interaction;
using UnityEngine;
using UnityEngine.XR;

namespace CellexalVR.Tools
{
    /// <summary>
    /// To remove objects in the scene. Graphs can not be deleted. Delete tool is activated by the delete tool button.
    /// </summary>
    public class RemovalController : MonoBehaviour
    {
        public Material inactiveMat;
        public Material activeMat;
        public ReferenceManager referenceManager;

        private bool controllerInside;
        private bool delete;
        private Transform target;
        private GameObject objectToDelete;
        private bool runningScript;

        // Open XR 
        //private SteamVR_Controller.Device device;
        private UnityEngine.XR.Interaction.Toolkit.ActionBasedController rightController;
        // Open XR 
        //private SteamVR_Controller.Device device;
        private UnityEngine.XR.InputDevice device;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            rightController = referenceManager.rightController;
            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
        }


        private void OnTriggerClick()
        {
            if (controllerInside)
            {
                if (objectToDelete == null) return;
                InitiateDelete(objectToDelete);
                referenceManager.multiuserMessageSender.SendMessageDeleteObject(objectToDelete.gameObject.name, objectToDelete.gameObject.tag);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("HeatBoard") || other.CompareTag("Network") || other.CompareTag("SubGraph")
                || other.CompareTag("FacsGraph") || other.CompareTag("FilterBlock"))
            {
                controllerInside = true;
                objectToDelete = other.gameObject;
                GetComponent<Light>().color = Color.red;
                GetComponent<Light>().range = 0.05f;
                GetComponent<MeshRenderer>().material = activeMat;
                transform.localScale = Vector3.one * 0.04f;
            }

        }

        private void OnTriggerExit(Collider other)
        {
            ResetHighlight();
            controllerInside = false;
        }


        /// <summary>
        /// Some things work differently depending on what type of object is being removed. 
        /// Also network and heatmap scripts need to be completely finished before the objects can be removed.
        /// </summary>
        /// <param name="obj">The object to remove.</param>
        private void InitiateDelete(GameObject obj)
        {
            objectToDelete = obj;
            switch (obj.tag)
            {
                case "HeatBoard":
                    if (obj.GetComponent<Heatmap>().removable)
                    {
                        Debug.Log("Script is running");
                        CellexalError.SpawnError("Delete failed", "Can not delete heatmap yet. Wait for script to finish before removing it.");
                        controllerInside = false;
                        return;
                    }
                    referenceManager.heatmapGenerator.DeleteHeatmap(obj.gameObject.name);
                    break;

                case "Network":
                    NetworkHandler nh = obj.GetComponent<NetworkHandler>();
                    if (nh)
                    {
                        nh.DeleteNetwork();
                    }

                    break;

                case "SubGraph":
                case "FacsGraph":
                    referenceManager.graphManager.DeleteGraph(obj.gameObject.name, obj.tag);
                    break;
            }
        }

        public void ResetHighlight()
        {
            GetComponent<MeshRenderer>().material = inactiveMat;
            GetComponent<Light>().color = Color.white;
            transform.localScale = Vector3.one * 0.03f;
            GetComponent<Light>().range = 0.04f;
        }
    }
}
