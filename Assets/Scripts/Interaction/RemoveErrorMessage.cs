using UnityEngine;
using CellexalVR.General;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Remove the pop up error message in the scene.
    /// </summary>
    public class RemoveErrorMessage : MonoBehaviour
    {
        public GameObject errorMessage;
        private bool controllerInside;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                controllerInside = true;
                GetComponent<Renderer>().material.color = Color.red;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                controllerInside = false;
                GetComponent<Renderer>().material.color = Color.white;
            }
        }

        private void Update()
        {
            if (controllerInside && Player.instance.rightHand.grabPinchAction.GetStateDown(Player.instance.rightHand.handType))
            {
                Destroy(errorMessage);
            }
        }
    }

}