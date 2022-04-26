using UnityEngine;
using System.Collections;
using CellexalVR.General;
using CellexalVR.Interaction;

namespace CellexalVR.Filters
{
    /// <summary>
    /// Represents a zone on a <see cref="FilterCreatorBlock"/> that can be highlighted. Highlighting is done by offsetting the texture to a part that has a zone highlighted is shown.
    /// </summary>
    public class FilterCreatorBlockHighlighter : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public FilterCreatorBlock parent;
        public int section;
        public enum FieldType { NAME, OPERATOR, VALUE, ATTRIBUTE_INCLUDE }
        public FieldType type;
        public TMPro.TextMeshPro textmeshpro;

        private FilterManager filterManager;
        // Open XR 
        //private SteamVR_Controller.Device device;
        private UnityEngine.XR.InputDevice device;
        private UnityEngine.XR.Interaction.Toolkit.ActionBasedController rightController;
        private bool controllerInside;

        private void Start()
        {
            rightController = referenceManager.rightController;
            filterManager = referenceManager.filterManager;
            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);

        }

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
                parent = transform.parent?.GetComponent<FilterCreatorBlock>();
                textmeshpro = gameObject.GetComponent<TMPro.TextMeshPro>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Smaller Controller Collider"))
            {
                if (parent.HighlightedSection != null)
                {
                    parent.HighlightedSection.controllerInside = false;
                }

                parent.HighlightedSection = this;
                controllerInside = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Smaller Controller Collider"))
            {
                controllerInside = false;
                if (parent.HighlightedSection == this)
                {
                    parent.HighlightedSection = null;
                }
            }
        }


        private void OnTriggerClick()
        {
            // Open XR
            //device = SteamVR_Controller.Input((int)rightController.index);
            if (controllerInside)
            {
                KeyboardHandler keyboard = null;
                if (type == FieldType.NAME)
                {
                    keyboard = referenceManager.filterNameKeyboard;
                    keyboard.additionalOutputs.Add(textmeshpro);
                }
                else if (type == FieldType.OPERATOR)
                {
                    keyboard = referenceManager.filterOperatorKeyboard;
                    keyboard.output = textmeshpro;

                }
                else if (type == FieldType.VALUE)
                {
                    keyboard = referenceManager.filterValueKeyboard;
                    keyboard.additionalOutputs.Add(textmeshpro);

                }
                else if (type == FieldType.ATTRIBUTE_INCLUDE)
                {
                    textmeshpro.text = textmeshpro.text == "included" ? "not included" : "included";
                    referenceManager.filterManager.UpdateFilterFromFilterCreator();
                }

                if (keyboard != null)
                {
                    keyboard.gameObject.SetActive(true);
                    keyboard.transform.position = transform.position;
                    keyboard.transform.Translate(new Vector3(-0.05f, -0.1f, 0f), Space.Self);
                }
            }

        }
    }
}

