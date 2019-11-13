using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using UnityEngine;
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
        private float speed;
        private float targetScale;
        private float currentTime;
        private float deleteTime = 0.7f;
        private float shrinkSpeed;
        private GameObject objectToDelete;
        private bool runningScript;

        private SteamVR_TrackedObject rightController;
        private SteamVR_Controller.Device device;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        void Start()
        {
            rightController = referenceManager.rightController;
            speed = 1.5f;
            shrinkSpeed = 2f;
            targetScale = 0.1f;
        }

        void Update()
        {
            if (device == null)
            {
                rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
                device = SteamVR_Controller.Input((int)rightController.index);
            }

            if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
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
            GetComponent<MeshRenderer>().material = inactiveMat;
            GetComponent<Light>().color = Color.white;
            transform.localScale = Vector3.one * 0.03f;
            GetComponent<Light>().range = 0.04f;
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
            currentTime = 0f;
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
                        nh.DeleteNetworkMultiUser();
                    }

                    break;

                case "SubGraph":
                    referenceManager.graphManager.DeleteGraph(obj.gameObject.name, obj.tag);
                    break;

                case "FacsGraph":
                    referenceManager.graphManager.DeleteGraph(obj.gameObject.name, obj.tag);
                    break;
            }
        }

        /// <summary>
        /// Called every frame if bool variable delete is true.
        /// It creates the animation of the delete process. Switch case is used to make sure we send the right message to the other users.
        /// </summary>
        /// <param name="obj">The object to remove.</param>
        //private void DeleteObject(GameObject obj)
        //{
        //    print("delete obj - " + obj.gameObject.name);
        //    if (!obj)
        //    {
        //        delete = false;
        //        GetComponent<MeshRenderer>().material = inactiveMat;
        //        return;
        //    }

        //    if (currentTime < deleteTime)
        //    {
        //        float step = speed * Time.deltaTime;
        //        obj.transform.position = Vector3.MoveTowards(obj.transform.position, transform.position, step);
        //        obj.transform.localScale -= Vector3.one * Time.deltaTime * shrinkSpeed;
        //        obj.transform.Rotate(Vector3.one * Time.deltaTime * 100);
        //    }

        //    if (Mathf.Abs(currentTime - deleteTime) <= 0.05f)
        //    {
        //        if (obj.transform.GetComponentInParent<NetworkHandler>() != null)
        //        {
        //            obj = obj.transform.GetComponentInParent<NetworkHandler>().gameObject;
        //            Destroy(obj);
        //        }
        //        else if (obj.GetComponent<NetworkHandler>())
        //        {
        //            Destroy(obj);
        //        }
        //        else
        //        {
        //            Destroy(obj);
        //        }
        //        CellexalLog.Log("Deleted object: " + obj.name);
        //        delete = false;
        //        GetComponent<MeshRenderer>().material = inactiveMat;
        //        GetComponent<Light>().color = Color.white;
        //        transform.localScale = Vector3.one * 0.03f;
        //        GetComponent<Light>().range = 0.04f;
        //    }
        //}
    }
}
