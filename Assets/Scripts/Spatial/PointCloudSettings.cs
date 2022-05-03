using AnalysisLogic;
using CellexalVR.AnalysisLogic;
using CellexalVR.General;
using DG.Tweening;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class PointCloudSettings : MonoBehaviour
{
    [SerializeField] private GameObject sizeSlider;
    [SerializeField] private GameObject spreadSlider;
    [SerializeField] private GameObject alphaSlider;
    [SerializeField] private UnityEvent<float> onSizeValueChanged;
    [SerializeField] private UnityEvent<float> onSpreadValueChanged;
    [SerializeField] private UnityEvent<float> onAlphaValueChanged;

    [SerializeField] private InputActionAsset actionAsset;
    [SerializeField] private InputActionReference actionReference;


    private bool toggled;
    private List<VisualEffect> pointClouds = new List<VisualEffect>();
    public float size = 0.003f;
    public float Size
    {
        get => size;
        set
        {
            size = value;
            onSizeValueChanged.Invoke(size);
        }
    }

    public float spread = 1f;
    public float Spread
    {
        get => spread;
        set
        {
            spread = value;
            onSpreadValueChanged.Invoke(spread);
        }
    }

    public float alpha = 1f;
    public float Alpha
    {
        get => alpha;
        set
        {
            alpha = value;
            onAlphaValueChanged.Invoke(alpha);
        }
    }

    private void Start()
    {
        CellexalEvents.GraphsLoaded.AddListener(PopulateList);
        //actionReference.action.performed += OnActionClick;
    }

    private void PopulateList()
    {
        foreach (PointCloud pc in PointCloudGenerator.instance.pointClouds)
        {
            pointClouds.Add(pc.GetComponent<VisualEffect>());
        }
    }

    private void OnActionClick(InputAction.CallbackContext context)
    {
        ToggleSettings();
    }

    public void ToggleSettings()
    {
        if (!toggled)
        {
            Transform cameraTransform = ReferenceManager.instance.headset.transform;
            transform.position = cameraTransform.position + cameraTransform.forward * 0.7f;
            transform.LookAt(2 * transform.position - cameraTransform.position);
        }
        Vector3 targetScale = toggled ? Vector3.zero : Vector3.one;
        sizeSlider.transform.DOScale(targetScale, 0.8f).SetEase(Ease.OutBounce);
        spreadSlider.transform.DOScale(targetScale, 0.8f).SetEase(Ease.OutBounce);
        alphaSlider.transform.DOScale(targetScale, 0.8f).SetEase(Ease.OutBounce);
        toggled = !toggled;
    }

    public void ChangePointSize(float value)
    {
        foreach (VisualEffect vfx in pointClouds)
        {
            vfx.SetFloat("Size", value);
            vfx.enabled = false;
            vfx.enabled = true;
        }
    }

    public void ChangePointSpread(float value)
    {
        foreach (VisualEffect vfx in pointClouds)
        {
            vfx.SetFloat("Spread", value);
            vfx.enabled = false;
            vfx.enabled = true;
        }
    }

    public void ChangePointAlpha(float value)
    {
        foreach (VisualEffect vfx in pointClouds)
        {
            vfx.SetFloat("Transparency", value);
            vfx.enabled = false;
            vfx.enabled = true;
        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(PointCloudSettings))]
public class PointCloudSettingsEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var settings = (PointCloudSettings)target;

        float newSizeValue = EditorGUILayout.FloatField("size", settings.size);
        if (newSizeValue != settings.Size)
        {
            settings.Size = newSizeValue;
        }

        float newSpreadValue = EditorGUILayout.FloatField("spread", settings.spread);
        if (newSpreadValue != settings.Spread)
        {
            settings.Spread = newSpreadValue;
        }

        float newAlphaValue = EditorGUILayout.FloatField("alpha", settings.alpha);
        if (newAlphaValue != settings.Alpha)
        {
            settings.Alpha = newAlphaValue;
        }
    }
}
#endif