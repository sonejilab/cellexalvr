using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using VRTK;
/// <summary>
/// This class represents the helper tool. Its job is to provide the user with descriptions of whatever it touches.
/// </summary>
public class HelperTool : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public TextMeshPro textMesh;
    public VRTK_StraightPointerRenderer pointerRenderer;
    public VRTK_Pointer pointer;
    public LayerMask layersToIgnore;
    public Transform customOrigin;
    public GameObject transparentQuad;
    public GameObject opaqueQuad;
    public GameObject controllerHelpTextsRight;
    public GameObject controllerHelpTextsLeft;
    public GameObject otherControllerHelpTool;

    private SteamVR_TrackedObject rightController;
    private string standardText = "Point the laser towards something to find out more";
    private string descriptionFilePath;
    private Dictionary<string, string> descriptions = new Dictionary<string, string>();
    private SteamVR_Controller.Device device;
    private GameObject helpMenu;
    private Ray ray;
    private RaycastHit hit;
    private Transform raycastingSource;
    private LayerMask savedLayersToIgnore;
    private Transform savedCustomOrigin;
    private float timeLastHit;
    private float initialY;
    private bool activated;

    private void Start()
    {
        descriptionFilePath = Application.streamingAssetsPath + "\\descriptions.txt";
        ReadDescriptionFile(descriptionFilePath);
        helpMenu = referenceManager.helpMenu;
        helpMenu.SetActive(false);
        opaqueQuad.SetActive(false);
        transparentQuad.SetActive(true);
        rightController = referenceManager.rightController;
        initialY = transform.localPosition.y;
        SetToolActivated(false);
    }

    private void Update()
    {
        if (activated)
        {
            raycastingSource = customOrigin.transform;
            device = SteamVR_Controller.Input((int)rightController.index);
            ray = new Ray(raycastingSource.position, raycastingSource.forward);
            if (Physics.Raycast(ray, out hit, 100f, ~layersToIgnore))
            {
                GameObject hitGameObject = hit.transform.gameObject;
                if (descriptions.ContainsKey(hitGameObject.tag))
                {
                    // if the gameobject's tag is in the dictionary
                    textMesh.text = descriptions[hitGameObject.tag];
                    timeLastHit = Time.time;
                    SetQuadOpaque(true);
                }
                else if (descriptions.ContainsKey(hitGameObject.name))
                {
                    // if the gameobject's name is in the dictionary
                    textMesh.text = descriptions[hitGameObject.name];
                    timeLastHit = Time.time;
                    SetQuadOpaque(true);
                }
                else if (descriptions.ContainsKey(hitGameObject.transform.parent.gameObject.name))
                {
                    // if the gameobject's parent's name is in the dictionary
                    // this happens when the raycast hits the keyboard
                    textMesh.text = descriptions[hitGameObject.transform.parent.gameObject.name];
                    timeLastHit = Time.time;
                    SetQuadOpaque(true);
                }
                else
                {
                    // Often the laser can be pointed at something and then accidentaly quickly pointed at nothing
                    // and then pointed at the original thing again. So we wait 4 seconds before removing the current 
                    // description just in case that happens.
                    if (Time.time - timeLastHit > 4)
                    {
                        // if we hit something but don't have a description for it
                        textMesh.text = standardText;
                        SetQuadOpaque(false);
                    }
                }
                Vector3 oldScale = opaqueQuad.transform.localScale;
                float newScaleY = textMesh.renderedHeight / 30f;
                Vector3 oldPosition = opaqueQuad.transform.localPosition;
                float newPositionY = -(newScaleY / 2f) + 2.1f;
                opaqueQuad.transform.localScale = new Vector3(oldScale.x, newScaleY, oldScale.z);
                opaqueQuad.transform.localPosition = new Vector3(oldPosition.x, newPositionY, oldPosition.z);
            }
            else
            {
                if (Time.time - timeLastHit > 4)
                {
                    // if we hit nothing
                    textMesh.text = standardText;
                    SetQuadOpaque(false);
                }
            }
        }
    }

    /// <summary>
    /// Sets one of the quads that act as background to active or inactive.
    /// </summary>
    /// <param name="opaque"> True for showing the opaque quad, false for showing the transparent. </param>
    private void SetQuadOpaque(bool opaque)
    {
        opaqueQuad.SetActive(opaque);
        transparentQuad.SetActive(!opaque);
    }

    /// <summary>
    /// Saves the layers that the laserpointer should ignore. Should be called <b>before</b> SetToolActivated.
    /// </summary>
    public void SaveLayersToIgnore()
    {
        savedLayersToIgnore = pointerRenderer.layersToIgnore;
    }

    /// <summary>
    /// Activates or deactivates this tool.
    /// </summary>
    /// <param name="activate"> True for activating the tool false for deactivating. </param>
    public void SetToolActivated(bool activate)
    {
        if (activate)
        {
            // change some fields to make the laser pointer work as intended
            pointerRenderer.layersToIgnore = layersToIgnore;
            savedCustomOrigin = pointer.customOrigin;
            pointer.customOrigin = customOrigin;
        }
        else
        {
            // change the fields back again
            pointerRenderer.layersToIgnore = savedLayersToIgnore;
            pointer.customOrigin = savedCustomOrigin;
        }
        pointerRenderer.enabled = activate;
        activated = activate;
        gameObject.SetActive(activate);
        helpMenu.SetActive(activate);
        otherControllerHelpTool.SetActive(activate);
        controllerHelpTextsRight.SetActive(activate);
        controllerHelpTextsLeft.SetActive(activate);
    }

    /// <summary>
    /// Reads the descriptions.txt which should be in the Assets folder.
    /// </summary>
    /// <param name="filepath"> The path to the file. </param>
    private void ReadDescriptionFile(string filepath)
    {
        // The file format should be
        // [KEY]:[VALUE]
        // [KEY]:[VALUE]
        // ...
        // Where [KEY] is either TAG_ followed by the name of a tag or just the name of a gameobject as it is displayed in the editor.
        // [VALUE] is the description that should be displayed when the tool touches the object

        string[] lines = File.ReadAllLines(filepath);
        if (lines.Length == 0)
        {
            Debug.LogWarning("No description file found at " + descriptionFilePath);
            return;
        }

        foreach (string line in lines)
        {
            // ignore empty lines
            if (line.Length == 0)
                continue;

            // comments in the file start with #
            if (line[0] == '#')
                continue;
            // line breaks in decsriptions are written with \n in plaintext.
            string formattedLine = line.Replace(@"\n", "\n");
            // tag names in the file start with "TAG_"
            if (formattedLine.Substring(0, 4).Equals("TAG_", StringComparison.Ordinal))
            {
                var colonIndex = formattedLine.IndexOf(":");
                string tagName = formattedLine.Substring(4, colonIndex - 4);

                descriptions[tagName] = formattedLine.Substring(colonIndex + 1);
            }
            else
            {
                // everything else is assumed to be names of gameobjects
                string[] splitString = formattedLine.Split(new char[] { ':' }, 2);
                descriptions[splitString[0]] = splitString[1];
            }

        }
    }

}
