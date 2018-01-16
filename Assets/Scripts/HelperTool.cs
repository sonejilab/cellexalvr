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
    public HelperToolActivator helpToolActivator;

    private GraphManager graphManager;
    private SteamVR_TrackedObject rightController;
    private string standardText = "Point the laser towards something to find out more";
    private string descriptionFilePath;
    private Dictionary<string, string> descriptions = new Dictionary<string, string>();
    private GameObject helpMenu;
    private Ray ray;
    private RaycastHit hit;
    private Transform raycastingSource;
    private LayerMask savedLayersToIgnore;
    private Transform savedCustomOrigin = null;
    private float timeLastHit;
    private bool activated;

    private List<GameObject> graphInfoPanels = new List<GameObject>();
    public List<GameObject> GraphInfoPanels { get { return graphInfoPanels; } }

    private void Start()
    {
        descriptionFilePath = Application.streamingAssetsPath + "\\descriptions.txt";
        ReadDescriptionFile(descriptionFilePath);
        helpMenu = referenceManager.helpMenu;
        helpMenu.SetActive(false);
        opaqueQuad.SetActive(false);
        transparentQuad.SetActive(true);
        rightController = referenceManager.rightController;
        graphManager = referenceManager.graphManager;
        SetToolActivated(false);

    }

    private void Update()
    {
        if (activated)
        {
            raycastingSource = customOrigin.transform;
            ray = new Ray(raycastingSource.position, raycastingSource.forward);
            if (Physics.Raycast(ray, out hit, 100f, ~layersToIgnore))
            {
                GameObject hitGameObject = hit.transform.gameObject;
                if (descriptions.ContainsKey(hitGameObject.name))
                {
                    // if the gameobject's name is in the dictionary
                    textMesh.text = descriptions[hitGameObject.name];
                    timeLastHit = Time.time;
                    SetQuadOpaque(true);
                }
                else if (descriptions.ContainsKey(hitGameObject.tag))
                {
                    // if the gameobject's tag is in the dictionary
                    textMesh.text = descriptions[hitGameObject.tag];
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
                    // and then pointed at the original thing again, especially when pointing it at graphs. So we wait 2 seconds before removing the current 
                    // description just in case that happens.
                    if (Time.time - timeLastHit > 2)
                    {
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
                if (Time.time - timeLastHit > 2)
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
            if (savedCustomOrigin == null)
                savedCustomOrigin = pointer.customOrigin;
            pointer.customOrigin = customOrigin;
        }
        else
        {
            // change the fields back again
            pointerRenderer.layersToIgnore = savedLayersToIgnore;
            pointer.customOrigin = savedCustomOrigin;
            savedCustomOrigin = null;
        }
        pointerRenderer.enabled = activate;
        activated = activate;
        gameObject.SetActive(activate);
        helpMenu.SetActive(activate);
        otherControllerHelpTool.SetActive(activate);
        controllerHelpTextsRight.SetActive(activate);
        controllerHelpTextsLeft.SetActive(activate);
        helpToolActivator.SwitchText(activate);
        foreach (GameObject panel in GraphInfoPanels)
        {
            panel.SetActive(activate);
        }
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
        CellExAlLog.Log("Started reading description file.");
        if (!File.Exists(filepath))
        {
            Debug.LogWarning("No description file found at " + filepath);
            return;
        }

        FileStream fileStream = new FileStream(filepath, FileMode.Open);
        StreamReader streamReader = new StreamReader(fileStream);
        int lineNbr = 0;
        while (!streamReader.EndOfStream)
        {
            lineNbr++;
            string line = streamReader.ReadLine();
            // ignore empty lines
            if (line.Length == 0)
                continue;

            // comments in the file start with #
            if (line[0] == '#')
                continue;

            // line breaks in decsriptions are written with \n in plaintext.
            string formattedLine = line.Replace(@"\n", "\n");
            string[] splitString = formattedLine.Split(new char[] { ':' }, 2);
            if (splitString.Length != 2)
            {
                CellExAlLog.Log("WARNING: No colon (:) found in description file at line " + lineNbr);
                continue;
            }
            if (splitString[0].Length == 0)
            {
                CellExAlLog.Log("WARNING: No key found in description file at line " + lineNbr);
                continue;
            }
            if (splitString[1].Length == 0)
            {
                CellExAlLog.Log("WARNING: No description found in description file at line " + lineNbr);
                continue;
            }
            // tag names in the file start with "TAG_"
            if (formattedLine.Substring(0, 4).Equals("TAG_", StringComparison.Ordinal))
            {
                // Remove the "TAG_" part
                string tagName = splitString[0].Substring(4);
                descriptions[tagName] = splitString[1];
            }
            else
            {
                // everything else is assumed to be names of gameobjects
                descriptions[splitString[0]] = splitString[1];
            }
        }
        streamReader.Close();
        fileStream.Close();
        CellExAlLog.Log("Finished reading description file.");
    }
}
