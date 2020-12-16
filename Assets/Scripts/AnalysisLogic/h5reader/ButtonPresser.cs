using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CellexalVR.General;
using System.IO;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace CellexalVR.AnalysisLogic.H5reader
{
    public class ButtonPresser : MonoBehaviour
    {
        public BoxCollider collider;
        [SerializeField] private GameObject note;
        private bool controllerInside;
        [SerializeField] private Color color;
        private Button button;

        private void Start()
        {
            button = GetComponent<Button>();
            collider.size = new Vector3(70, 30, 1);
            collider.center = new Vector3(0, -15, 0);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag.Equals("Player"))
            {
                controllerInside = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag.Equals("Player"))
            {
                controllerInside = false;
            }
        }

        private void Update()
        {
            if (controllerInside && Player.instance.rightHand.grabPinchAction.GetStateDown(Player.instance.rightHand.handType))
            {
                button.onClick.Invoke();
                
            }
        }
    }
}