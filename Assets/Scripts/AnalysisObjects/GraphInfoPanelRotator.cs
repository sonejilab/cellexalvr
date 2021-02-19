using UnityEngine;
using Valve.VR.InteractionSystem;

namespace CellexalVR.AnalysisObjects
{

    /// <summary>
    /// Rotates the panels that are shown beside each graph when the help tool is activated.
    /// </summary>
    public class GraphInfoPanelRotator : MonoBehaviour
    {
        private Transform CameraToLookAt;

        void Start()
        {
            CameraToLookAt = Player.instance.headCollider.transform;
        }

        void Update()
        {
            transform.LookAt(CameraToLookAt);
            transform.Rotate(0f, -90f, 0f);
        }
    }
}
