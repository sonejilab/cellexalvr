using UnityEngine;
using System.Collections;

/// <summary>
/// This class controls the color picker that is used in the settings menu.
/// </summary>
public class ColorPicker : MonoBehaviour
{
    //RenderTexture satValBox = new RenderTexture(190, 190, 0, RenderTextureFormat.ARGB32);
    public GameObject satValBox;
    public GameObject previewColorBox;
    public TMPro.TMP_InputField rgbCodeBox;
    public GameObject satValMarker;
    public GameObject hueMarker;
    public UnityEngine.UI.Image satValMarkerImage;

    private Material satValBoxMaterial;
    private Material previewColorBoxMaterial;
    private float hue = 0f;
    private float sat = 0f;
    private float val = 0f;
    private enum ColorPickerComponent { NONE, SATVALBOX, HUEBOX }
    private ColorPickerComponent activeComponent = ColorPickerComponent.NONE;
    [HideInInspector]
    public ColorPickerButton activeButton;

    void Start()
    {
        satValBoxMaterial = satValBox.GetComponent<UnityEngine.UI.Image>().material;
        previewColorBoxMaterial = previewColorBox.GetComponent<UnityEngine.UI.Image>().material;

        satValBoxMaterial.SetFloat("_Hue", hue);
        Color newColor = Color.HSVToRGB(hue, sat, val);
        previewColorBoxMaterial.color = newColor;
        rgbCodeBox.text = ColorUtility.ToHtmlStringRGB(newColor);

        UpdateComponents();
    }

    void Update()
    {

        if (Input.GetMouseButton(0))
        {
            Vector2 pos = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, Input.mousePosition, null, out pos);
            if (Input.GetMouseButtonDown(0))
            {
                // figure out what component was pressed
                if (pos.x > 10 && pos.x < 200 && pos.y > 50 && pos.y < 240)
                {
                    activeComponent = ColorPickerComponent.SATVALBOX;
                }
                else if (pos.x > 210 && pos.x < 240 && pos.y > 50 && pos.y < 240)
                {
                    activeComponent = ColorPickerComponent.HUEBOX;
                }
            }
            // handle the active component
            if (activeComponent == ColorPickerComponent.SATVALBOX)
            {
                pos = Clamp(pos, 10, 200, 50, 240);
                HandleSatValBox(pos);
                Cursor.visible = false;
            }
            else if (activeComponent == ColorPickerComponent.HUEBOX)
            {
                pos = Clamp(pos, 210, 240, 50, 240);
                HandleHueBox(pos);
                Cursor.visible = false;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            activeComponent = ColorPickerComponent.NONE;
            Cursor.visible = true;
            if (activeButton != null)
            {
                activeButton.ChooseColor();
            }
        }

    }

    /// <summary>
    /// Clamps a <see cref="Vector2"/> between some values.
    /// </summary>
    /// <param name="v">The vector to clamp.</param>
    /// <param name="minx">The minimum value of <see cref="Vector2.x"/>.</param>
    /// <param name="maxx">The maximum value of <see cref="Vector2.x"/>.</param>
    /// <param name="miny">The minimum value of <see cref="Vector2.y"/>.</param>
    /// <param name="maxy">The maximum value of <see cref="Vector2.y"/>.</param>
    /// <returns>The resulting clamped <see cref="Vector2"/></returns>
    private Vector2 Clamp(Vector2 v, float minx, float maxx, float miny, float maxy)
    {
        if (v.x < minx)
            v.x = minx;
        else if (v.x > maxx)
            v.x = maxx;

        if (v.y < miny)
            v.y = miny;
        else if (v.y > maxy)
            v.y = maxy;

        return v;
    }

    /// <summary>
    /// Handles what happens when the user clicks inside the saturation/value box.
    /// </summary>
    /// <param name="mousePos">The current mouse position.</param>
    private void HandleSatValBox(Vector2 mousePos)
    {
        sat = (mousePos.x - 10) / 190f;
        val = (mousePos.y - 50) / 190f;

        UpdateComponents();
    }

    /// <summary>
    /// Handles what happens when the user clicks inside the hue box.
    /// </summary>
    /// <param name="mousePos">The current mouse position.</param>
    private void HandleHueBox(Vector2 mousePos)
    {
        hue = (mousePos.y - 50) / 190f;
        UpdateComponents();
    }

    /// <summary>
    /// Updates all visual components in the color picker.
    /// </summary>
    public void UpdateComponents()
    {
        if (hue >= 1f)
        {
            hue = 0.9999f;
        }

        if (val < .5)
        {
            satValMarkerImage.color = Color.white;
        }
        else
        {
            satValMarkerImage.color = Color.black;
        }

        Color newColor = Color.HSVToRGB(hue, sat, val);
        //Color.RGBToHSV(newColor, out hue, out sat, out val);

        satValBoxMaterial.SetFloat("_Hue", hue);
        float oldZ = satValMarker.transform.localPosition.z;
        satValMarker.transform.localPosition = new Vector3(10 + sat * 190, 50 + val * 190, oldZ);
        hueMarker.transform.localPosition = new Vector3(225, 50 + hue * 190, 0);
        rgbCodeBox.text = ColorUtility.ToHtmlStringRGB(newColor);
        previewColorBoxMaterial.color = Color.HSVToRGB(hue, sat, val);

        if (activeButton != null)
        {
            activeButton.SetColor(newColor);
        }
    }

    /// <summary>
    /// Handles what happens when the user is done editting the RGB-text box
    /// </summary>
    public void HandleRGBBox()
    {
        string text = "#" + rgbCodeBox.text;
        Color newColor = Color.white;
        ColorUtility.TryParseHtmlString(text, out newColor);
        Color.RGBToHSV(newColor, out hue, out sat, out val);
        UpdateComponents();
    }

    /// <summary>
    /// Sets the current color of the color picker. Also updates all visual components.
    /// </summary>
    /// <param name="newColor">The new color.</param>
    public void SetColor(Color newColor)
    {
        Color.RGBToHSV(newColor, out hue, out sat, out val);
        UpdateComponents();
    }

    /// <summary>
    /// Gets the currently selected color.
    /// </summary>
    /// <returns>The currently selected color.</returns>
    public Color GetColor()
    {
        return Color.HSVToRGB(hue, sat, val);
    }

    /// <summary>
    /// Moves the color picker to a position on the screen. The color pickers lower left corner will be at <paramref name="desired"/>. The color picker may be moved to fit inside the screen.
    /// </summary>
    /// <param name="desired">The desired position of the color picker.</param>
    public void MoveToDesiredPosition(Vector3 desired)
    {

        if (desired.y + 250 > Screen.height)
        {
            desired = new Vector3(desired.x, Screen.height - 250, desired.z);
        }

        transform.position = desired;
    }
}
