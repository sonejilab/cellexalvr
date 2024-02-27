using CellexalVR.General;
using CellexalVR.Interaction;
using CellexalVR.Menu.SubMenus;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.ColorPicker
{
    public class ColorPickerPopout : CellexalRaycastable
    {
        public ReferenceManager referenceManager;
        public CellexalButton finalizeChoiceButton;
        public RemoveColorButton removeColorButton;
        public ColorPickerSubMenu colorPickerSubMenu;
        public GameObject satvalBox;
        public GameObject satvalMarker;
        public Sprite satvalMarkerUpperHalfSprite;
        public Sprite satvalMarkerLowerHalfSprite;
        public Sprite satvalMarkerUpperHalfHighlightSprite;
        public Sprite satvalMarkerLowerHalfHighlightSprite;
        public GameObject hueSlider;
        public GameObject hueMarker;
        public Sprite hueMarkerSprite;
        public Sprite hueMarkerHighlightSprite;

        private float hue;
        private float sat;
        private float val;
        private ColorPickerButton buttonToUpdate;
        private bool triggerPressed = false;
        private bool stickToSatValBox = false;
        private bool stickToHueBox = false;
        private Material satvalBoxShader;
        private SpriteRenderer satvalMarkerRenderer;
        private SpriteRenderer hueMarkerRenderer;
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
            satvalMarkerRenderer = satvalMarker.GetComponent<SpriteRenderer>();
            satvalBoxShader = satvalBox.GetComponent<Renderer>().material;
            hueMarkerRenderer = hueMarker.GetComponent<SpriteRenderer>();
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

        public override void OnRaycastHit(RaycastHit hitInfo, CellexalRaycast raycaster)
        {
            // the raycast hit in local coordinates
            Vector3 hitCoord = transform.InverseTransformPoint(hitInfo.point);
            if (!stickToHueBox && (stickToSatValBox || CoordInsideBox(hitCoord, satValBoxLowerLeft, satValBoxUpperRight)))
            {
                // hit sat/val box
                if (triggerPressed)
                {
                    hitCoord = Clamp(hitCoord, satValBoxLowerLeft, satValBoxUpperRight);
                    stickToSatValBox = true;
                    satvalMarker.transform.localPosition = new Vector3(hitCoord.x, 1.1f, hitCoord.z);
                    sat = (hitCoord.x - satValBoxLowerLeft.x) / (satValBoxUpperRight.x - satValBoxLowerLeft.x);
                    val = (hitCoord.z - satValBoxLowerLeft.z) / (satValBoxUpperRight.z - satValBoxLowerLeft.z);
                    UpdateColor();
                }
                else
                {
                    stickToSatValBox = false;
                }
                SetSatValBoxMarkerSprite(triggerPressed);
            }
            else if (stickToHueBox || CoordInsideBox(hitCoord, huesliderBoxLowerLeft, huesliderBoxUpperRight))
            {
                // hit hue slider
                if (triggerPressed)
                {
                    hitCoord = Clamp(hitCoord, huesliderBoxLowerLeft, huesliderBoxUpperRight);
                    stickToHueBox = true;
                    hueMarker.transform.localPosition = new Vector3(0.2f, 0.8f, hitCoord.z);
                    hue = 1 - (hitCoord.z - huesliderBoxLowerLeft.z) / (huesliderBoxUpperRight.z - huesliderBoxLowerLeft.z);
                    satvalBoxShader.SetFloat("_Hue", hue);
                    UpdateColor();
                }
                else
                {
                    stickToHueBox = false;
                }
                SetHueMarkerSprite(triggerPressed);
            }
        }


        private void SetSatValBoxMarkerSprite(bool triggerPressed)
        {
            if (triggerPressed)
            {
                if (val > 0.5f)
                {
                    satvalMarkerRenderer.sprite = satvalMarkerUpperHalfHighlightSprite;
                }
                else
                {
                    satvalMarkerRenderer.sprite = satvalMarkerLowerHalfHighlightSprite;
                }
            }
            else
            {
                if (val > 0.5f)
                {
                    satvalMarkerRenderer.sprite = satvalMarkerUpperHalfSprite;
                }
                else
                {
                    satvalMarkerRenderer.sprite = satvalMarkerLowerHalfSprite;
                }
            }
        }

        private void SetHueMarkerSprite(bool triggerPressed)
        {
            if (triggerPressed)
            {
                hueMarkerRenderer.sprite = hueMarkerHighlightSprite;
            }
            else
            {
                hueMarkerRenderer.sprite = hueMarkerSprite;
            }
        }

        private bool CoordInsideBox(Vector3 coord, Vector3 lowerLeft, Vector3 upperRight)
        {
            return coord.x >= lowerLeft.x && coord.z >= lowerLeft.z && coord.x <= upperRight.x && coord.z <= upperRight.z;
        }

        /// <summary>
        /// Element-wise clamping of a vector.
        /// </summary>
        /// <param name="v">The vector to clamp.</param>
        /// <param name="min">The minimum x, y and z values to clamp between.</param>
        /// <param name="max">The maximum x, y and z values to clamp between.</param>
        /// <returns>A new <see cref="Vector3"/> clamped between <paramref name="min"/> and <paramref name="max"/></returns>
        private Vector3 Clamp(Vector3 v, Vector3 min, Vector3 max)
        {
            return new Vector3(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y), Mathf.Clamp(v.z, min.z, max.z));
        }

        private void UpdateColor()
        {
            Color newColor = Color.HSVToRGB(hue, sat, val);
            buttonToUpdate.SetColor(newColor);
            finalizeChoiceButton.meshStandardColor = newColor;
        }

        public void Open(ColorPickerButton openingButton)
        {
            if (buttonToUpdate)
            {
                // clear already bound button
                CellexalConfig.Config.SelectionToolColors[buttonToUpdate.colorPickerButtonBase.selectionToolColorIndex] = Color.HSVToRGB(hue, sat, val);
                buttonToUpdate.highlightGameObject.SetActive(false);
            }
            gameObject.SetActive(true);
            buttonToUpdate = openingButton;
            Color.RGBToHSV(openingButton.meshStandardColor, out hue, out sat, out val);
            float satvalMarkerX = sat * (satValBoxUpperRight.x - satValBoxLowerLeft.x) + satValBoxLowerLeft.x;
            float satvalMarkerZ = val * (satValBoxUpperRight.z - satValBoxLowerLeft.z) + satValBoxLowerLeft.z;
            float hueMarkerZ = ((1 - hue) * (huesliderBoxUpperRight.z - huesliderBoxLowerLeft.z)) + huesliderBoxLowerLeft.z;
            satvalMarker.transform.localPosition = new Vector3(satvalMarkerX, 1.1f, satvalMarkerZ);
            hueMarker.transform.localPosition = new Vector3(0.2f, 0.8f, hueMarkerZ);
            if (!satvalBoxShader)
            {
                satvalMarkerRenderer = satvalMarker.GetComponent<SpriteRenderer>();
                satvalBoxShader = satvalBox.GetComponent<Renderer>().material;
            }
            SetSatValBoxMarkerSprite(false);
            satvalBoxShader.SetFloat("_Hue", hue);
        }


        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void FinalizeChoice()
        {
            buttonToUpdate.highlightGameObject.SetActive(false);
            buttonToUpdate.colorPickerButtonBase.FinalizeChoice();
            referenceManager.settingsMenu.SetValues();
            //CellexalConfig.Config.SelectionToolColors[buttonToUpdate.colorPickerButtonBase.selectionToolColorIndex] = Color.HSVToRGB(hue, sat, val);
            //referenceManager.configManager.SaveConfigFile(referenceManager.configManager.currentProfileFullPath);
            //CellexalEvents.ConfigLoaded.Invoke();
            finalizeChoiceButton.controllerInside = false;
            Close();
        }

        public void RemoveColor()
        {
            colorPickerSubMenu.RemoveSelectionColorButton(buttonToUpdate);
            buttonToUpdate = null;
            removeColorButton.controllerInside = false;
            Close();
        }
    }
}
