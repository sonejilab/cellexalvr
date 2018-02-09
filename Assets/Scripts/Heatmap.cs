using System.Collections.Generic;
using UnityEngine;
using System.IO;
//using System.Drawing;
using System;
using VRTK;
using System.Drawing;
using System.Collections;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

/// <summary>
/// This class represents a heatmap.
/// </summary>
public class Heatmap : MonoBehaviour
{
    // TODO CELLEXAL: make a single gene box plot (out of an existing heatmap?)
    public Texture texture;
    public TextMesh infoText;
    public GameObject highlightQuad;

    private Dictionary<Cell, int> containedCells;
    private SteamVR_Controller.Device device;
    private bool controllerInside = false;
    private GameObject fire;
    private SteamVR_TrackedObject rightController;
    private Transform raycastingSource;
    private string imageFilepath;
    public string HeatmapName;
    private ReferenceManager referenceManager;
    private GameManager gameManager;
    private TextMesh highlightInfoText;


    private List<Tuple<string, int>> cells;
    private List<Tuple<int, float>> groupWidths;
    private string[] genes;
    private int bitmapWidth = 4096;
    private int bitmapHeight = 4096;
    private int heatmapX = 250;
    private int heatmapY = 250;
    private int heatmapWidth = 3596;
    private int heatmapHeight = 3596;
    private int geneListX = 3846;
    private int geneListWidth = 250;
    private int groupBarY = 150;
    private int groupBarHeight = 50;

    // Use this for initialization
    void Start()
    {
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        rightController = referenceManager.rightController;
        raycastingSource = rightController.transform;
        gameManager = referenceManager.gameManager;
        fire = referenceManager.fire;
        highlightInfoText = highlightQuad.GetComponentInChildren<TextMesh>();
    }

    public void BuildTexture()
    {
        StartCoroutine(BuildTextureCoroutine());
    }

    private IEnumerator BuildTextureCoroutine()
    {
        List<GraphPoint> selection = referenceManager.selectionToolHandler.GetLastSelection();
        // item1 is the cell name, item2 is the group
        cells = new List<Tuple<string, int>>();
        groupWidths = new List<Tuple<int, float>>();

        int lastGroup = -1;
        float width = 0;
        // read the cells and their groups
        foreach (GraphPoint graphpoint in selection)
        {
            int group = graphpoint.CurrentGroup;
            cells.Add(new Tuple<string, int>(graphpoint.Label, group));
            if (lastGroup == -1)
            {
                lastGroup = group;
            }

            // used for saving the widths of the groups later
            if (group != lastGroup)
            {
                groupWidths.Add(new Tuple<int, float>(lastGroup, width));
                width = 0;
                lastGroup = group;
            }
            width++;
        }
        // add the last group as well
        groupWidths.Add(new Tuple<int, float>(lastGroup, width));

        string[] tempGenes = { "gata1", "klf1", "car1", "kel", "mpl", "hlf", "ltb", "ly6a", "ifitm1", "elane", "atp8b4", "ccl9" };
        genes = tempGenes;

        SQLiter.SQLite database = referenceManager.database;

        Bitmap bitmap = new Bitmap(bitmapWidth, bitmapHeight);
        System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

        System.Drawing.Font geneFont = new System.Drawing.Font(FontFamily.GenericMonospace, 40f, System.Drawing.FontStyle.Bold);

        // turn the expression colors into brushes that are used for drawing later
        int numberOfExpressionColors = CellExAlConfig.NumberOfExpressionColors;
        SolidBrush[] brushes = new SolidBrush[numberOfExpressionColors];
        GraphManager graphManager = referenceManager.graphManager;
        for (int i = 0; i < numberOfExpressionColors; ++i)
        {
            UnityEngine.Color uc = graphManager.GeneExpressionMaterials[i].color;
            SolidBrush p = new SolidBrush(System.Drawing.Color.FromArgb((int)(uc.r * 255), (int)(uc.g * 255), (int)(uc.b * 255)));
            brushes[i] = p;
        }
        g.Clear(System.Drawing.Color.FromArgb(255, 255, 255));

        // extract the cell names into a separate array
        string[] cellsArray = new string[cells.Count];
        for (int i = 0; i < cells.Count; ++i)
        {
            cellsArray[i] = cells[i].Item1;
        }

        float xcoord = heatmapX;
        float ycoord = heatmapY;
        float xcoordInc = (float)heatmapWidth / cellsArray.Length;
        float ycoordInc = (float)heatmapHeight / genes.Length;

        // update the group widths now that we know the width of one cell in the heatmap
        for (int i = 0; i < groupWidths.Count; ++i)
        {
            Tuple<int, float> oldTuple = groupWidths[i];
            groupWidths[i] = new Tuple<int, float>(oldTuple.Item1, oldTuple.Item2 * xcoordInc);
        }

        // get the grouping colors
        SolidBrush[] groupBrushes = new SolidBrush[numberOfExpressionColors];
        for (int i = 0; i < graphManager.SelectedMaterials.Length; ++i)
        {
            UnityEngine.Color unitycolor = graphManager.SelectedMaterials[i].color;
            groupBrushes[i] = new SolidBrush(System.Drawing.Color.FromArgb((int)(unitycolor.r * 255), (int)(unitycolor.g * 255), (int)(unitycolor.b * 255)));
        }

        // draw the colored bar on the top
        for (int i = 0; i < cells.Count; ++i)
        {
            // the group bar should be aligned with the heatmap in X-axis
            g.FillRectangle(groupBrushes[cells[i].Item2], xcoord, groupBarY, xcoordInc, groupBarHeight);
            xcoord += xcoordInc;
        }
        xcoord = heatmapX;

        // draw the actual heatmap
        for (int i = 0; i < genes.Length; ++i)
        {
            while (database.QueryRunning)
            {
                yield return null;
            }
            database.QueryGenesInCells(genes[i], cellsArray);
            while (database.QueryRunning)
            {
                yield return null;
            }
            ArrayList result = database._result;

            // put everything in a hashmap so lookups are fast later.
            Dictionary<string, float> expressions = new Dictionary<string, float>((int)(result.Count * 1.25f));
            float highestExpression = 0;
            foreach (Tuple<string, float> t in result)
            {
                expressions[t.Item1] = t.Item2;
                if (t.Item2 > highestExpression)
                {
                    highestExpression = t.Item2;
                }
            }

            for (int j = 0; j < cellsArray.Length; ++j)
            {
                string cell = cellsArray[j];
                float expression = 0f;
                if (expressions.ContainsKey(cell))
                {
                    expression = expressions[cell];
                }

                if (expression == highestExpression)
                {
                    g.FillRectangle(brushes[brushes.Length - 1], xcoord, ycoord, xcoordInc, ycoordInc);
                }
                else
                {
                    g.FillRectangle(brushes[(int)(expression / highestExpression * numberOfExpressionColors)], xcoord, ycoord, xcoordInc, ycoordInc);
                }
                xcoord += xcoordInc;
            }

            g.DrawString(genes[i], geneFont, SystemBrushes.MenuText, geneListX, ycoord);
            xcoord = heatmapX;
            ycoord += ycoordInc;

        }

        g.Dispose();
        // copy the bitmap data over to a unity texture
        MemoryStream stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        Texture2D tex = new Texture2D(bitmapWidth, bitmapHeight);
        tex.LoadImage(stream.ToArray());
        stream.Close();
        GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
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

        raycastingSource = rightController.transform;
        // this method is probably responsible for too much. oh well.
        Ray ray = new Ray(raycastingSource.position, raycastingSource.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            int hitx = (int)(hit.textureCoord.x * bitmapWidth);
            int hity = (int)(hit.textureCoord.y * bitmapHeight);
            if (CoordinatesInsideRect(hitx, hity, geneListX, heatmapY, geneListWidth, heatmapHeight))
            {
                // if we hit the list of genes
                int geneHit = (int)((float)((bitmapHeight - hity) - heatmapY) / heatmapHeight * genes.Length);

                float highlightMarkerWidth = (float)geneListWidth / bitmapWidth;
                float highlightMarkerHeight = ((float)heatmapHeight / bitmapHeight) / genes.Length;
                float highlightMarkerX = (float)geneListX / bitmapWidth + highlightMarkerWidth / 2 - 0.5f;
                float highlightMarkerY = -(float)heatmapY / bitmapHeight - geneHit * (highlightMarkerHeight) - highlightMarkerHeight / 2 + 0.5f;

                highlightQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, -0.001f);
                highlightQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
                highlightQuad.SetActive(true);
                highlightInfoText.text = "";

                if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                {
                    referenceManager.cellManager.ColorGraphsByGene(genes[geneHit]);
                }
            }
            else if (CoordinatesInsideRect(hitx, bitmapHeight - hity, heatmapX, groupBarY, heatmapWidth, groupBarHeight))
            {
                // if we hit the grouping bar

                // get this groups width and xcoordinate
                float groupX = heatmapX;
                float groupWidth = 0;
                for (int i = 0; i < groupWidths.Count; ++i)
                {
                    if (groupX + groupWidths[i].Item2 > hitx)
                    {
                        groupWidth = groupWidths[i].Item2;
                        break;
                    }
                    groupX += groupWidths[i].Item2;
                }

                float highlightMarkerWidth = groupWidth / bitmapWidth;
                float highlightMarkerHeight = ((float)groupBarHeight / bitmapHeight);
                float highlightMarkerX = groupX / bitmapWidth + highlightMarkerWidth / 2 - 0.5f;
                float highlightMarkerY = -(float)groupBarY / bitmapHeight - highlightMarkerHeight / 2 + 0.5f;

                highlightQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, -0.001f);
                highlightQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
                highlightQuad.SetActive(true);
                highlightInfoText.text = "";
            }
            else if (CoordinatesInsideRect(hitx, bitmapHeight - hity, heatmapX, heatmapY, heatmapWidth, heatmapHeight))
            {
                // if we hit the actual heatmap
                // get this groups width and xcoordinate
                float groupX = heatmapX;
                float groupWidth = 0;
                int group = 0;
                for (int i = 0; i < groupWidths.Count; ++i)
                {
                    if (groupX + groupWidths[i].Item2 > hitx)
                    {
                        group = groupWidths[i].Item1;
                        groupWidth = groupWidths[i].Item2;
                        break;
                    }
                    groupX += groupWidths[i].Item2;
                }
                int geneHit = (int)((float)((bitmapHeight - hity) - heatmapY) / heatmapHeight * genes.Length);
                float highlightMarkerWidth = groupWidth / bitmapWidth;
                float highlightMarkerHeight = ((float)heatmapHeight / bitmapHeight) / genes.Length;
                float highlightMarkerX = groupX / bitmapWidth + highlightMarkerWidth / 2 - 0.5f;
                float highlightMarkerY = -(float)heatmapY / bitmapHeight - geneHit * (highlightMarkerHeight) - highlightMarkerHeight / 2 + 0.5f;

                highlightQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, -0.001f);
                highlightQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
                highlightQuad.SetActive(true);
                highlightInfoText.text = "Group: " + group + "\nGene: " + genes[geneHit];
                highlightInfoText.transform.localScale = new Vector3(0.003f / highlightMarkerWidth, 0.003f / highlightMarkerHeight, 0.003f);

            }
            else
            {
                highlightQuad.SetActive(false);
                highlightInfoText.text = "";
            }
        }
        else
        {
            highlightQuad.SetActive(false);
            highlightInfoText.text = "";
        }
    }

    private bool CoordinatesInsideRect(int x, int y, int rectX, int rectY, int rectWidth, int rectHeight)
    {
        return x >= rectX && y >= rectY && x <= rectX + rectWidth && y <= rectY + rectHeight;
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
        foreach (KeyValuePair<Cell, int> pair in containedCells)
        {
            pair.Key.SetGroup(pair.Value);
        }
    }

    /// <summary>
    /// Sets some variables. Should be called after a heatmap is instantiated.
    /// </summary>
    public void SetVars(Dictionary<Cell, int> colors)
    {
        // containedCells = new Dictionary<Cell, Color>();
        containedCells = colors;
        infoText.text = "Total number of cells: " + colors.Count;
        // infoText.text += "\nNumber of colours: " + numberOfColours;
    }
}
