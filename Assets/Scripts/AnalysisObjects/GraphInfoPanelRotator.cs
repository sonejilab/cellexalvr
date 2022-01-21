using CellexalVR.General;
using CellexalVR.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace CellexalVR.AnalysisObjects
{
    /// <summary>
    /// Rotates the panels that are shown beside each graph when the help tool is activated.
    /// </summary>
    public class GraphInfoPanelRotator : MonoBehaviour
    {
        [SerializeField] private UnityEvent<string> onTriggerEvent;
        private bool controllerInside;
        private Transform CameraToLookAt;

        private void Start()
        {
            CameraToLookAt = ReferenceManager.instance.headset.transform;
        }

        private void Update()
        {
            transform.LookAt(2 * transform.position - CameraToLookAt.position);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("GameController"))
                return;
            controllerInside = true;
            onTriggerEvent?.Invoke("on");
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("GameController"))
                return;
            controllerInside = false;
            onTriggerEvent?.Invoke("off");
        }
    }
}
