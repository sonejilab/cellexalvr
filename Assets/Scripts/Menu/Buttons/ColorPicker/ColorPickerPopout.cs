using Assets.Scripts.Menu.ColorPicker;
using CellexalVR.General;
using CellexalVR.Menu.Buttons;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Assets.Scripts.Menu.Buttons.ColorPicker
{
    public class ColorPickerPopout : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public CellexalButton finalizeChoiceButton;
        public GameObject satvalBox;
        public GameObject satvalMarker;
        public GameObject hueSlider;
        public GameObject hueMarker;


        private float hue;
        private float sat;
        private float val;
        private ColorPickerButton buttonToUpdate;
        private GameObject rightController;
        private bool triggerPressed = false;
        private Material satvalBoxShader;
        private Vector3 satValBoxLowerLeft = new Vector3(-0.46f, 0f, -0.14f);
        private Vector3 satValBoxUpperRight = new Vector3(0.14f, 0f, 0.46f);
        private Vector3 huesliderBoxLowerLeft = new Vector3(0.16f, 0f, -0.14f);
        private Vector3 huesliderBoxUpperRight = new Vector3(0.24f, 0f, 0.46f);



        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            rightController = referenceManager.rightController.gameObject.gameObject;
        }

        private void OnEnable()
        {
            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
            CellexalEvents.RightTriggerUp.AddListener(OnTriggerUp);
        }

        private void OnDisable()
        {
            CellexalEvents.RightTriggerClick.RemoveListener(OnTriggerClick);
            CellexalEvents.RightTriggerUp.RemoveListener(OnTriggerUp);
        }

        private void OnTriggerClick()
        {
            triggerPressed = true;
        }

        private void OnTriggerUp()
        {
            triggerPressed = false;
        }

        private void Update()
        {
            Raycast();
        }

        public void Raycast()
        {
            RaycastHit hit;
            if (Physics.Raycast(rightController.transform.position, rightController.transform.forward, out hit, 25f))
            {
                if (hit.collider.gameObject != gameObject)
                {
                    return;
                }
                referenceManager.controllerModelSwitcher.SwitchToModel(CellexalVR.Interaction.ControllerModelSwitcher.Model.Menu);

                // the raycast hit in local coordinates
                Vector3 hitCoord = transform.InverseTransformPoint(hit.point);
                if (CoordInsideBox(hitCoord, satValBoxLowerLeft, satValBoxUpperRight))
                {
                    // hit sat/val box
                    if (triggerPressed)
                    {
                        satvalMarker.transform.localPosition = new Vector3(hitCoord.x, 1.1f, hitCoord.z);
                        sat = hitCoord.x - satValBoxLowerLeft.x / (satValBoxUpperRight.x - satValBoxLowerLeft.x);
                        val = hitCoord.z - satValBoxLowerLeft.z / (satValBoxUpperRight.z - satValBoxLowerLeft.z);
                        UpdateColor();
                    }
                }
                else if (CoordInsideBox(hitCoord, huesliderBoxLowerLeft, huesliderBoxUpperRight))
                {
                    // hit hue slider
                    if (triggerPressed)
                    {
                        hueMarker.transform.localPosition = new Vector3(0.2f, 0.8f, hitCoord.z);
                        hue = hitCoord.z - huesliderBoxLowerLeft.z / (huesliderBoxUpperRight.z - huesliderBoxLowerLeft.z);
                        satvalBoxShader.SetFloat("Hue", hue);
                        UpdateColor();
                    }
                }
            }
            else
            {
                referenceManager.controllerModelSwitcher.SwitchToDesiredModel();
            }
        }

        private bool CoordInsideBox(Vector3 coord, Vector3 lowerLeft, Vector3 upperRight)
        {
            return coord.x >= lowerLeft.x && coord.z >= lowerLeft.z && coord.x <= upperRight.x && coord.z <= upperRight.z;
        }

        private void UpdateColor()
        {
            Color newColor = Color.HSVToRGB(hue, sat, val);
            buttonToUpdate.SetColor(newColor);
            finalizeChoiceButton.GetComponent<Renderer>().material.color = newColor;
        }

        public void Open(ColorPickerButton openingButton)
        {
            gameObject.SetActive(true);
            buttonToUpdate = openingButton;
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

    }
}
