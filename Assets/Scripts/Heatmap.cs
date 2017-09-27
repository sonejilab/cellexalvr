using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using VRTK;

/// <summary>
/// This class represents a heatmap.
/// </summary>
public class Heatmap : MonoBehaviour
{

    public Texture texture;
    public TextMesh infoText;
    private Dictionary<Cell, Color> containedCells;
    private SteamVR_Controller.Device device;
    private GraphManager graphManager;
    private bool controllerInside = false;
    private GameObject fire;
    private SteamVR_TrackedObject rightController;
    private string imageFilepath;
    public string HeatmapName;
    private ReferenceManager referenceManager;
    private GameManager gameManager;

    // Use this for initialization
    void Start()
    {
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        rightController = referenceManager.rightController;
        gameManager = referenceManager.gameManager;
        fire = referenceManager.fire;
        graphManager = referenceManager.graphManager;
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            controllerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            controllerInside = false;
        }
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) && fire.activeSelf)
        {
            gameObject.GetComponent<HeatmapBurner>().BurnHeatmap();
        }
        if (GetComponent<VRTK_InteractableObject>().enabled)
        {
            gameManager.InformMoveHeatmap(HeatmapName, transform.position, transform.rotation, transform.localScale);
        }
    }

    /// <summary>
    /// Updates this heatmap's image.
    /// </summary>
    public void UpdateImage(string filepath)
    {
        imageFilepath = filepath;
        byte[] fileData = File.ReadAllBytes(filepath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
    }

    /// <summary>
    /// Saves the image used to create this heatmap to another directory.
    /// The saved image will have the a name based on when the image is saved.
    /// </summary>
    public void SaveImage()
    {
        string saveDir = Directory.GetCurrentDirectory() + @"\Saved_Images";
        if (!Directory.Exists(saveDir))
        {
            CellExAlLog.Log("Creating directory " + CellExAlLog.FixFilePath(saveDir));
            Directory.CreateDirectory(saveDir);
        }

        saveDir += "\\" + CellExAlUser.Username;
        if (!Directory.Exists(saveDir))
        {
            CellExAlLog.Log("Creating directory " + CellExAlLog.FixFilePath(saveDir));
            Directory.CreateDirectory(saveDir);
        }

        // this is the only acceptable date time format, order-wise
        var time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        string saveFileName = saveDir + @"\heatmap_" + time + ".png";
        // if the button is pressed twice the same second, the filenames will collide.
        while (File.Exists(saveFileName))
        {
            // append "_d" until the filenames no longer collide.
            // microsoft is removing the 260 character filename limit so this shouldn't run into too many problems
            saveFileName += "_d";
        }
        File.Move(imageFilepath, saveFileName);
    }

    /// <summary>
    /// Recolours all graphs with the colors that the cells had when this heatmap was created.
    /// Graph points that are not part of this heatmap are not recoloured and will keep their colour.
    /// </summary>
    public void ColorCells()
    {
        // print("color cells");
        foreach (KeyValuePair<Cell, Color> pair in containedCells)
        {
            pair.Key.SetColor(pair.Value);
        }
    }

    /// <summary>
    /// Sets some variables. Should be called after a heatmap is instantiated.
    /// </summary>
    public void SetVars(Dictionary<Cell, Color> colors)
    {
        // containedCells = new Dictionary<Cell, Color>();
        containedCells = colors;
        infoText.text = "Total number of cells: " + colors.Count;
        // infoText.text += "\nNumber of colours: " + numberOfColours;
    }
}
