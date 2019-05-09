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
        private float fade = 0;
        private Transform target;
        private float speed;
        private float targetScale;
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
            }

            if (delete)
            {
                DeleteObject(objectToDelete);
            }

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("HeatBoard") || other.CompareTag("Network") || other.CompareTag("Subgraph"))
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
        void InitiateDelete(GameObject obj)
        {
            switch (obj.tag)
            {
                case "HeatBoard":
                    if (objectToDelete.GetComponent<Heatmap>().removable)
                    {
                        Debug.Log("Script is running");
                        CellexalError.SpawnError("Delete failed", "Can not delete heatmap yet. Wait for script to finish before removing it.");
                        controllerInside = false;
                        return;
                    }
                    delete = true;
                    CellexalEvents.HeatmapBurned.Invoke();
                    break;

                case "Network":
                    NetworkHandler nh = objectToDelete.GetComponent<NetworkHandler>();
                    if (nh)
                    {
                        if (objectToDelete.GetComponent<NetworkHandler>().removable)
                        {
                            Debug.Log("Script is running");
                            CellexalError.SpawnError("Delete failed", "Can not delete network yet. Wait for script to finish before removing it.");
                            controllerInside = false;
                            return;
                        }
                        foreach (NetworkCenter nc in objectToDelete.GetComponent<NetworkHandler>().networks)
                        {
                            nc.BringBackOriginal();
                        }
                        referenceManager.arcsSubMenu.DestroyTab(nh.name.Split('_')[1]); // Get last part of nw name   
                        referenceManager.networkGenerator.networkList.RemoveAll(item => item == null);
                        referenceManager.graphManager.RemoveNetwork(nh);
                    }
                    delete = true;
                    break;

                case "Subgraph":
                    referenceManager.graphManager.Graphs.Remove(objectToDelete.GetComponent<Graph>());
                    delete = true;
                    break;
            }
        }

        /// <summary>
        /// Called every frame if bool variable delete is true.
        /// It creates the animation of the delete process. Switch case is used to make sure we send the right message to the other users.
        /// </summary>
        /// <param name="obj">The object to remove.</param>
        void DeleteObject(GameObject obj)
        {
            if (!obj)
            {
                delete = false;
                GetComponent<MeshRenderer>().material = inactiveMat;
                return;
            }
            float step = speed * Time.deltaTime;
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, transform.position, step);
            obj.transform.localScale -= Vector3.one * Time.deltaTime * shrinkSpeed;
            obj.transform.Rotate(Vector3.one * Time.deltaTime * 100);

            switch (obj.tag)
            {
                case "HeatBoard":
                    referenceManager.gameManager.InformMoveHeatmap(obj.name, obj.transform.position, obj.transform.rotation, obj.transform.localScale);
                    break;

                case "Network":
                    referenceManager.gameManager.InformMoveNetwork(obj.name, obj.transform.position, obj.transform.rotation, obj.transform.localScale);
                    break;

                case "Subgraph":
                    referenceManager.gameManager.InformMoveGraph(obj.name, obj.transform.position, obj.transform.rotation, obj.transform.localScale);
                    break;
            }

            if (obj.transform.localScale.x <= targetScale)
            {
                CellexalLog.Log("Deleted object: " + obj.name);
                delete = false;
                Destroy(obj);
                GetComponent<MeshRenderer>().material = inactiveMat;
                GetComponent<Light>().color = Color.white;
                transform.localScale = Vector3.one * 0.03f;
                GetComponent<Light>().range = 0.04f;
                if (obj.GetComponent<NetworkHandler>())
                {
                    referenceManager.gameManager.InformDeleteNetwork(obj.name);
                }
                else
                {
                    referenceManager.gameManager.InformDeleteObject(obj.name);
                }
            }
        }
    }
}