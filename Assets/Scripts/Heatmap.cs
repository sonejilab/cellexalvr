using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using VRTK;
using System.Drawing;
using System.Collections;
using System.Drawing.Imaging;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

/// <summary>
/// This class represents a heatmap.
/// </summary>
public class Heatmap : MonoBehaviour
{
    // TODO CELLEXAL: make a single gene box plot (out of an existing heatmap?)
    public Texture texture;
    public TextMesh infoText;
    public GameObject highlightQuad;
    public GameObject confirmQuad;
    public GameObject movingQuadX;
    public GameObject movingQuadY;

    private GraphManager graphManager;
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

    private Bitmap bitmap;
    /// <summary>
    /// Item1: cell name, Item2: group
    /// </summary>
    private List<Tuple<string, int>> cells;
    /// <summary>
    /// Item1: group number, Item2: group width in coordinates, Item3: number of cells in the group
    /// </summary>
    private List<Tuple<int, float, int>> groupWidths;
    private string[] genes;
    private int bitmapWidth = 4096;
    private int bitmapHeight = 4096;
    private int heatmapX = 250;
    private int heatmapY = 250;
    private int heatmapWidth = 3596;
    private int heatmapHeight = 3596;
    private int geneListX = 3846;
    private int geneListWidth = 250;
    private int groupBarY = 100;
    private int groupBarHeight = 100;
    private System.Drawing.Font geneFont;
    private int numberOfExpressionColors;
    private SolidBrush[] heatmapBrushes;
    private Texture2D tex;

    private int selectionStartX;
    private int selectionStartY;
    private bool selecting = false;
    private bool movingSelection = false;
    // these are numbers ranging [0, groupWidths.Length)
    private int selectedGroupLeft;
    private int selectedGroupRight;
    // these are numbers ranging [0, genes.Length)
    private int selectedGeneTop;
    private int selectedGeneBottom;
    // these are the actual coordinates and size of the box
    private float selectedBoxX;
    private float selectedBoxY;
    private float selectedBoxWidth;
    private float selectedBoxHeight;

    void Start()
    {
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        rightController = referenceManager.rightController;
        raycastingSource = rightController.transform;
        gameManager = referenceManager.gameManager;
        fire = referenceManager.fire;
        highlightQuad.SetActive(false);
        confirmQuad.SetActive(false);
        movingQuadX.SetActive(false);
        movingQuadY.SetActive(false);
        highlightInfoText = highlightQuad.GetComponentInChildren<TextMesh>();

        geneFont = new System.Drawing.Font(FontFamily.GenericMonospace, 12f, System.Drawing.FontStyle.Bold);

        // turn the expression colors into brushes that are used for drawing later
        numberOfExpressionColors = CellExAlConfig.NumberOfExpressionColors;
        heatmapBrushes = new SolidBrush[numberOfExpressionColors];
        graphManager = referenceManager.graphManager;
        for (int i = 0; i < numberOfExpressionColors; ++i)
        {
            UnityEngine.Color uc = graphManager.GeneExpressionMaterials[i].color;
            SolidBrush p = new SolidBrush(System.Drawing.Color.FromArgb((int)(uc.r * 255), (int)(uc.g * 255), (int)(uc.b * 255)));
            heatmapBrushes[i] = p;
        }


        gameObject.SetActive(false);
    }

    public void BuildTexture()
    {
        gameObject.SetActive(true);
        GetComponent<Collider>().enabled = false;

        List<GraphPoint> selection = referenceManager.selectionToolHandler.GetLastSelection();
        // item1 is the cell name, item2 is the group
        cells = new List<Tuple<string, int>>();
        groupWidths = new List<Tuple<int, float, int>>();
        float cellWidth = (float)heatmapWidth / selection.Count;
        int lastGroup = -1;
        int width = 0;
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
                groupWidths.Add(new Tuple<int, float, int>(lastGroup, width * cellWidth, (int)width));
                width = 0;
                lastGroup = group;
            }
            width++;
        }
        // add the last group as well
        groupWidths.Add(new Tuple<int, float, int>(lastGroup, width * cellWidth, width));
        //string[] tempGenes = { "gata1", "klf1", "car1", "kel", "mpl", "hlf", "ltb", "ly6a", "ifitm1", "elane", "atp8b4", "ccl9" };
        StreamReader streamReader = new StreamReader(Directory.GetCurrentDirectory() + "\\test_genes.txt");
        string[] tempGenes = new string[250];
        int i = 0;
        while (!streamReader.EndOfStream)
        {
            tempGenes[i++] = streamReader.ReadLine();
        }
        streamReader.Close();
        StartCoroutine(BuildTextureCoroutine(groupWidths, tempGenes));
    }

    private void BuildTexture(List<Tuple<int, float, int>> groupWidths, string[] genes)
    {
        GetComponent<Collider>().enabled = false;
        // merge groups
        for (int i = 0; i < groupWidths.Count - 1; ++i)
        {
            // if two groups with the same color are now beside eachother, merge them
            if (groupWidths[i].Item1 == groupWidths[i + 1].Item1)
            {
                Tuple<int, float, int> oldTuple1 = groupWidths[i];
                Tuple<int, float, int> oldTuple2 = groupWidths[i + 1];
                float newWidthCoords = oldTuple1.Item2 + oldTuple2.Item2;
                int newWidthCells = oldTuple1.Item3 + oldTuple2.Item3;
                groupWidths[i] = new Tuple<int, float, int>(oldTuple1.Item1, newWidthCoords, newWidthCells);
                groupWidths.RemoveAt(i + 1);
            }
        }
        StartCoroutine(BuildTextureCoroutine(groupWidths, genes));
    }

    private IEnumerator BuildTextureCoroutine(List<Tuple<int, float, int>> groupWidths, string[] genes)
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        CellExAlLog.Log("Started building a heatmap texture");

        this.genes = genes;
        SQLiter.SQLite database = referenceManager.database;
        bitmap = new Bitmap(bitmapWidth, bitmapHeight);
        System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);


        GraphManager graphManager = referenceManager.graphManager;
        // get the grouping colors
        SolidBrush[] groupBrushes = new SolidBrush[graphManager.SelectedMaterials.Length];
        for (int i = 0; i < graphManager.SelectedMaterials.Length; ++i)
        {
            UnityEngine.Color unitycolor = graphManager.SelectedMaterials[i].color;
            groupBrushes[i] = new SolidBrush(System.Drawing.Color.FromArgb((int)(unitycolor.r * 255), (int)(unitycolor.g * 255), (int)(unitycolor.b * 255)));
        }
        g.Clear(System.Drawing.Color.FromArgb(255, 255, 255));

        float xcoord = heatmapX;
        float ycoord = heatmapY;
        float xcoordInc = (float)heatmapWidth / cells.Count;
        float ycoordInc = (float)heatmapHeight / genes.Length;

        for (int i = 0; i < groupWidths.Count; ++i)
        {
            int groupNbr = groupWidths[i].Item1;
            float groupWidth = groupWidths[i].Item2;
            g.FillRectangle(groupBrushes[groupNbr], xcoord, groupBarY, groupWidth, groupBarHeight);
            xcoord += groupWidth;
        }
        xcoord = heatmapX;

        database.QueryGenesIds(genes);
        while (database.QueryRunning)
        {
            yield return null;
        }

        ArrayList result = database._result;
        Dictionary<string, string> geneIds = new Dictionary<string, string>(result.Count);
        foreach (Tuple<string, string> t in result)
        {
            // ids are keys, names are values
            geneIds[t.Item2] = t.Item1;
        }

        Dictionary<string, int> genePositions = new Dictionary<string, int>(genes.Length);
        for (int i = 0; i < genes.Length; ++i)
        {
            // ids are keys, names are values
            genePositions[genes[i]] = i;
        }

        Dictionary<string, int> cellsPosition = new Dictionary<string, int>(cells.Count);


        string[] cellsArray = new string[cells.Count];
        for (int i = 0; i < cells.Count; ++i)
        {
            cellsPosition[cells[i].Item1] = i;
            cellsArray[i] = cells[i].Item1;
        }

        database.QueryGenesInCells(genes, cellsArray);
        while (database.QueryRunning)
        {
            yield return null;
        }
        result = database._result;
        Thread thread = new Thread(() => ReadQueryResults(g, xcoordInc, ycoordInc, result, geneIds, genePositions, cellsPosition));
        thread.Start();
        while (thread.IsAlive)
        {
            yield return null;
        }
        // the thread is now done and the heatmap has been painted
        g.Dispose();
        //// copy the bitmap data over to a unity texture
        //// using a memorystream here seemed like a better alternative but made the standalone crash
        string heatmapFilePath = Directory.GetCurrentDirectory() + @"\Images\heatmap_temp.png";
        bitmap.Save(heatmapFilePath, ImageFormat.Png);

        // these yields makes the loading a little bit smoother, but still cuts a few frames.
        Texture2D tex = new Texture2D(4096, 4096);
        yield return null;
        tex.LoadImage(File.ReadAllBytes(heatmapFilePath));
        yield return null;
        GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
        yield return null;
        GetComponent<Collider>().enabled = true;

        stopwatch.Stop();
        CellExAlLog.Log("Finished building a heatmap texture in " + stopwatch.Elapsed.ToString());
    }

    /// <summary>
    /// Run as a thread in <see cref="BuildTexture"/>. Reads the result as a thread because it takes more than a single frame and figuring out where to put yields is hard.
    /// </summary>
    /// <param name="g">The graphics object bound to the heatmap.</param>
    /// <param name="xcoordInc">The width of a cell in the heatmap.</param>
    /// <param name="ycoordInc">The height of a gene in the heatmap.</param>
    /// <param name="result">The list from the database with the results.</param>
    /// <param name="geneIds">Dictionary mapping gene ids to gene names.</param>
    /// <param name="genePositions">Dictionary mapping gene names to their position on the heatmap.</param>
    /// <param name="cellsPosition">Dictionary mapping cell names to their position on the heatmap.</param>
    private void ReadQueryResults(System.Drawing.Graphics g, float xcoordInc, float ycoordInc, ArrayList result, Dictionary<string, string> geneIds, Dictionary<string, int> genePositions, Dictionary<string, int> cellsPosition)
    {
        string geneName = "";
        float highestExpression = 0;
        g.FillRectangle(heatmapBrushes[0], heatmapX, heatmapY, heatmapWidth, heatmapHeight);
        float xcoord = 0;
        float ycoord = 0;
        for (int i = 0; i < result.Count; ++i)
        {
            // the arraylist should contain the gene id and that gene's highest expression before all the expressions
            Tuple<string, float> tuple = (Tuple<string, float>)result[i];
            if (geneIds.ContainsKey(tuple.Item1))
            {
                // new gene
                geneName = geneIds[tuple.Item1];
                highestExpression = tuple.Item2;
                ycoord = heatmapY + genePositions[geneName] * ycoordInc;
                g.DrawString(geneName, geneFont, SystemBrushes.MenuText, geneListX, ycoord);
            }
            else
            {
                string cellName = tuple.Item1;
                float expression = tuple.Item2;
                xcoord = heatmapX + cellsPosition[cellName] * xcoordInc;

                if (expression == highestExpression)
                {
                    g.FillRectangle(heatmapBrushes[heatmapBrushes.Length - 1], xcoord, ycoord, xcoordInc, ycoordInc);
                }
                else
                {
                    g.FillRectangle(heatmapBrushes[(int)(expression / highestExpression * numberOfExpressionColors)], xcoord, ycoord, xcoordInc, ycoordInc);
                }
            }
        }

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
        Ray ray = new Ray(raycastingSource.position, raycastingSource.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            int hitx = (int)(hit.textureCoord.x * bitmapWidth);
            int hity = (int)(hit.textureCoord.y * bitmapHeight);
            if (CoordinatesInsideRect(hitx, hity, geneListX, heatmapY, geneListWidth, heatmapHeight))
            {
                // if we hit the list of genes
                int geneHit = HandleHitGeneList(hity);

                if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                {
                    referenceManager.cellManager.ColorGraphsByGene(genes[geneHit]);
                }
            }
            else if (CoordinatesInsideRect(hitx, bitmapHeight - hity, heatmapX, groupBarY, heatmapWidth, groupBarHeight))
            {
                // if we hit the grouping bar
                HandleHitGroupingBar(hitx);
            }
            else if (CoordinatesInsideRect(hitx, bitmapHeight - hity, heatmapX, heatmapY, heatmapWidth, heatmapHeight))
            {
                // if we hit the actual heatmap
                if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                {
                    if (CoordinatesInsideRect(hitx, bitmapHeight - hity, (int)selectedBoxX, (int)selectedBoxY, (int)selectedBoxWidth, (int)selectedBoxHeight))
                    {
                        // if we hit a confirmed selection
                        movingSelection = true;
                    }
                    else
                    {
                        // if we hit something else
                        selecting = true;
                        selectionStartX = hitx;
                        selectionStartY = hity;
                    }
                }

                if (device.GetPress(SteamVR_Controller.ButtonMask.Trigger) && selecting)
                {
                    // called when choosing a box selection
                    HandleBoxSelection(hitx, hity, selectionStartX, selectionStartY);
                }
                else if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger) && selecting)
                {
                    // called when letting go of the trigger to finalize a box selection
                    selecting = false;
                    ConfirmSelection(hitx, hity, selectionStartX, selectionStartY);
                }
                else if (device.GetPress(SteamVR_Controller.ButtonMask.Trigger) && movingSelection)
                {
                    // called when moving a selection
                    HandleMovingSelection(hitx, hity);
                }
                else if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger) && movingSelection)
                {
                    // called when letting go of the trigger to move the selection
                    MoveSelection(hitx, hity, selectedGroupLeft, selectedGroupRight, selectedGeneTop, selectedGeneBottom);
                }
                else
                {
                    // handle when the raycast just hits the heatmap
                    HandleHitHeatmap(hitx, hity);
                }
            }
            else
            {
                // if we hit the heatmap but not any area of interest, like the borders or any space in between
                highlightQuad.SetActive(false);
                highlightInfoText.text = "";
            }
        }
        else
        {
            // if we don't hit the heatmap at all
            highlightQuad.SetActive(false);
            highlightInfoText.text = "";
        }
        if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
        {

            selecting = false;
            movingSelection = false;
        }
    }

    /// <summary>
    /// Handles the highlighting when the raycast hits the heatmap
    /// </summary>
    /// <param name="hitx"> The x coordinate of the hit. Measured in pixels of the texture.</param>
    /// <param name="hity">The x coordinate if the hit. Meaured in pixels of the texture.</param>
    private void HandleHitHeatmap(int hitx, int hity)
    {
        // get this groups width and xcoordinate
        float groupX, groupWidth;
        int group;
        FindGroupInfo(hitx, out groupX, out groupWidth, out group);

        int geneHit = (int)((float)((bitmapHeight - hity) - heatmapY) / heatmapHeight * genes.Length);
        float highlightMarkerWidth = groupWidth / bitmapWidth;
        float highlightMarkerHeight = ((float)heatmapHeight / bitmapHeight) / genes.Length;
        float highlightMarkerX = groupX / bitmapWidth + highlightMarkerWidth / 2 - 0.5f;
        float highlightMarkerY = -(float)heatmapY / bitmapHeight - geneHit * (highlightMarkerHeight) - highlightMarkerHeight / 2 + 0.5f;

        highlightQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, -0.001f);
        highlightQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
        highlightQuad.SetActive(true);
        highlightInfoText.text = "Group: " + group + "\nGene: " + genes[geneHit];
        // the smaller the highlight quad becomes, the larger the text has to become
        highlightInfoText.transform.localScale = new Vector3(0.003f / highlightMarkerWidth, 0.003f / highlightMarkerHeight, 0.003f);
    }

    /// <summary>
    /// Finds out some info about what group is at a x coordinate.
    /// </summary>
    /// <param name="hitx">The x coordinate that the raycast hit.</param>
    /// <param name="groupX">The leftmost x coordinate of the group that was hit.</param>
    /// <param name="groupWidth">The width of the group, measured in pixels.</param>
    /// <param name="group">The number (color) of the group.</param>
    private void FindGroupInfo(int hitx, out float groupX, out float groupWidth, out int group)
    {
        groupX = heatmapX;
        groupWidth = 0;
        group = 0;
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
    }

    /// <summary>
    /// Handles the highlighting when the raycast hits the grouping bar.
    /// The grouping bar is only 1 item tall and thus we do not care about the y coorindate.
    /// </summary>
    /// <param name="hitx"> The xcoordinate of the hit.</param>
    private void HandleHitGroupingBar(int hitx)
    {
        // get this groups width and xcoordinate
        float groupX, groupWidth;
        int group;
        FindGroupInfo(hitx, out groupX, out groupWidth, out group);

        float highlightMarkerWidth = groupWidth / bitmapWidth;
        float highlightMarkerHeight = ((float)groupBarHeight / bitmapHeight);
        float highlightMarkerX = groupX / bitmapWidth + highlightMarkerWidth / 2 - 0.5f;
        float highlightMarkerY = -(float)groupBarY / bitmapHeight - highlightMarkerHeight / 2 + 0.5f;

        highlightQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, -0.001f);
        highlightQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
        highlightQuad.SetActive(true);
        highlightInfoText.text = "";
    }

    /// <summary>
    /// Handles the highlighting of the gene list.
    /// The gene list is only 1 item wide and thus we do not care about the xcoordinate.
    /// </summary>
    /// <param name="hity">The y coordinate of the hit.</param>
    /// <returns>An index of the gene that was hit.</returns>
    private int HandleHitGeneList(int hity)
    {
        int geneHit = (int)((float)((bitmapHeight - hity) - heatmapY) / heatmapHeight * genes.Length);

        float highlightMarkerWidth = (float)geneListWidth / bitmapWidth;
        float highlightMarkerHeight = ((float)heatmapHeight / bitmapHeight) / genes.Length;
        float highlightMarkerX = (float)geneListX / bitmapWidth + highlightMarkerWidth / 2 - 0.5f;
        float highlightMarkerY = -(float)heatmapY / bitmapHeight - geneHit * (highlightMarkerHeight) - highlightMarkerHeight / 2 + 0.5f;

        highlightQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, -0.001f);
        highlightQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
        highlightQuad.SetActive(true);
        highlightInfoText.text = "";
        return geneHit;
    }

    /// <summary>
    /// Handles the highlighting when the user is holding the trigger button to select multiple groups and genes. <paramref name="hitx"/> and <paramref name="hity"/> are determined on this frame,
    /// <paramref name="selectionStartX"/> and <paramref name="selectionStartY"/> were determined when the user first pressed the trigger.
    /// </summary>
    /// <param name="hitx">The last x coordinate that the raycast hit.</param>
    /// <param name="hity">The last y coordinate that the raycast hit.</param>
    /// <param name="selectionStartX">The first x coordinate that the raycast hit.</param>
    /// <param name="selectionStartY">The first y coordinate that the raycast hit.</param>
    private void HandleBoxSelection(int hitx, int hity, int selectionStartX, int selectionStartY)
    {
        // since the groupings have irregular widths we need to iterate over the list of widths
        float boxX = heatmapX;
        float boxWidth = 0;
        for (int i = 0; i < groupWidths.Count; ++i)
        {
            if (boxX + groupWidths[i].Item2 > hitx || boxX + groupWidths[i].Item2 > selectionStartX)
            {
                do
                {
                    boxWidth += groupWidths[i].Item2;
                    i++;
                } while (boxX + boxWidth < hitx || boxX + boxWidth < selectionStartX);
                break;
            }
            boxX += groupWidths[i].Item2;
        }

        float highlightMarkerWidth = boxWidth / bitmapWidth;
        float highlightMarkerX = boxX / bitmapWidth + highlightMarkerWidth / 2 - 0.5f;

        // the genes all have the same height so no need for loops here
        int geneHit1 = (int)((float)((bitmapHeight - hity) - heatmapY) / heatmapHeight * genes.Length);
        int geneHit2 = (int)((float)((bitmapHeight - selectionStartY) - heatmapY) / heatmapHeight * genes.Length);
        int smallerGeneHit = geneHit1 < geneHit2 ? geneHit1 : geneHit2;
        float highlightMarkerHeight = ((float)heatmapHeight / bitmapHeight) / genes.Length * (Math.Abs(geneHit1 - geneHit2) + 1);
        float highlightMarkerY = -((float)heatmapY + smallerGeneHit * (heatmapHeight / genes.Length)) / bitmapHeight - highlightMarkerHeight / 2 + 0.5f;

        highlightQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, -0.001f);
        highlightQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
        highlightQuad.SetActive(true);
        highlightInfoText.text = "";

    }

    /// <summary>
    /// Checks if two coordinates are inside a rectangle.
    /// </summary>
    /// <param name="x">The x coordinate</param>
    /// <param name="y">The y coordinate</param>
    /// <param name="rectX">The rectangle's x coordinate</param>
    /// <param name="rectY">The rectangle's y coordinate</param>
    /// <param name="rectWidth">The rectangle's width</param>
    /// <param name="rectHeight">The rectangle's height</param>
    /// <returns></returns>
    private bool CoordinatesInsideRect(int x, int y, int rectX, int rectY, int rectWidth, int rectHeight)
    {
        return x >= rectX && y >= rectY && x <= rectX + rectWidth && y <= rectY + rectHeight;
    }

    /// <summary>
    /// Confirms the cells inside the rectangle drawn by the user. <paramref name="hitx"/> and <paramref name="hity"/> are determined on this frame,
    /// <paramref name="selectionStartX"/> and <paramref name="selectionStartY"/> were determined when the user first pressed the trigger.
    /// </summary>
    /// <param name="hitx">The last x coordinate that the raycast hit.</param>
    /// <param name="hity">The last y coordinate that the raycast hit.</param>
    /// <param name="selectionStartX">The first x coordinate that the raycast hit when the user first pressed the trigger.</param>
    /// <param name="selectionStartY">The first y coordinate that the raycast hit when the user first pressed the trigger.</param>
    private void ConfirmSelection(int hitx, int hity, int selectionStartX, int selectionStartY)
    {
        // since the groupings have irregular widths we need to iterate over the list of widths
        selectedBoxX = heatmapX;
        selectedBoxWidth = 0;

        selectedGroupLeft = 0;
        // the do while loop below increments selectedGroupRight one time too many, so start at -1
        selectedGroupRight = -1;
        selectedGeneBottom = 0;
        selectedGeneTop = 0;

        for (int i = 0; i < groupWidths.Count; ++i)
        {
            if (selectedBoxX + groupWidths[i].Item2 > hitx || selectedBoxX + groupWidths[i].Item2 > selectionStartX)
            {
                do
                {
                    selectedGroupRight++;
                    selectedBoxWidth += groupWidths[i].Item2;
                    i++;
                } while (selectedBoxX + selectedBoxWidth < hitx || selectedBoxX + selectedBoxWidth < selectionStartX);
                break;
            }
            selectedBoxX += groupWidths[i].Item2;
            selectedGroupLeft++;
            selectedGroupRight++;
        }

        float highlightMarkerWidth = selectedBoxWidth / bitmapWidth;
        float highlightMarkerX = selectedBoxX / bitmapWidth + highlightMarkerWidth / 2 - 0.5f;

        // the genes all have the same height so no need for loops here
        int geneHit1 = (int)((float)((bitmapHeight - hity) - heatmapY) / heatmapHeight * genes.Length);
        int geneHit2 = (int)((float)((bitmapHeight - selectionStartY) - heatmapY) / heatmapHeight * genes.Length);
        if (geneHit1 < geneHit2)
        {
            selectedGeneTop = geneHit1;
            selectedGeneBottom = geneHit2;
        }
        else
        {
            selectedGeneTop = geneHit2;
            selectedGeneBottom = geneHit1;
        }
        // have to add 1 at the end here so it includes the bottom row as well
        selectedBoxHeight = ((float)heatmapHeight) / genes.Length * (Math.Abs(geneHit1 - geneHit2) + 1);
        float highlightMarkerHeight = selectedBoxHeight / bitmapHeight;
        selectedBoxY = (float)heatmapY + selectedGeneTop * (heatmapHeight / genes.Length);
        float highlightMarkerY = -(selectedBoxY) / bitmapHeight - highlightMarkerHeight / 2 + 0.5f;

        confirmQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, -0.001f);
        confirmQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
        confirmQuad.SetActive(true);
    }

    /// <summary>
    /// Moves the <see cref="movingQuadX"/> and <see cref="movingQuadY"/> when choosing where to move a selection
    /// </summary>
    /// <param name="hitx">The x coordinate where the raycast hit the heatmap</param>
    /// <param name="hity">The y coordinate where the raycast hit the heatmap</param>
    private void HandleMovingSelection(int hitx, int hity)
    {
        if (hitx < selectedBoxX || hitx > selectedBoxX + selectedBoxWidth)
        {
            float groupX, groupWidth;
            int group;
            FindGroupInfo(hitx, out groupX, out groupWidth, out group);
            if (hitx > groupX + groupWidth / 2f)
            {
                groupX += groupWidth;
            }
            float highlightMarkerX = groupX / bitmapWidth + heatmapWidth / (2 * bitmapWidth) - 0.5f;
            movingQuadY.transform.localPosition = new Vector3(highlightMarkerX, 0f, -0.001f);
            movingQuadY.SetActive(true);
        }
        else
        {
            movingQuadY.SetActive(false);
        }
        if (bitmapHeight - hity < selectedBoxY || bitmapHeight - hity > selectedBoxY + selectedBoxHeight)
        {

            int geneHit = (int)(((bitmapHeight - hity + ((float)heatmapHeight / genes.Length) / 2) - heatmapY) / heatmapHeight * genes.Length);
            float highlightMarkerY = -((float)heatmapY + geneHit * (heatmapHeight / genes.Length)) / bitmapHeight + 0.5f;
            movingQuadX.transform.localPosition = new Vector3(0f, highlightMarkerY, -0.001f);
            movingQuadX.SetActive(true);
        }
        else
        {
            movingQuadX.SetActive(false);
        }
    }

    /// <summary>
    /// Moves a part of the heatmap to another part. This can mean moving both rows and coloumns. Entire rows and coloumns and always moved and never split.
    /// </summary>
    /// <param name="hitx">The x coordinate where the selection should be moved to.</param>
    /// <param name="hity">The y coordinate where the selection should be moved to.</param>
    /// <param name="selectedGroupLeft">The lower index of the groups that should be moved.</param>
    /// <param name="selectedGroupRight">The higher index of the groups that should be moved.</param>
    /// <param name="selectedGeneTop">The lower index of the genes that should be moved.</param>
    /// <param name="selectedGeneBottom">The higher index of the gnees that should be moved.</param>
    private void MoveSelection(int hitx, int hity, int selectedGroupLeft, int selectedGroupRight, int selectedGeneTop, int selectedGeneBottom)
    {
        bool recalculate = false;
        if (hitx < selectedBoxX || hitx > selectedBoxX + selectedBoxWidth)
        {
            int nbrOfGroups = selectedGroupRight - selectedGroupLeft + 1;
            int groupIndexToMoveTo = 0;
            float groupX = heatmapX;
            while (groupX + groupWidths[groupIndexToMoveTo].Item2 < hitx)
            {
                groupX += groupWidths[groupIndexToMoveTo].Item2;
                groupIndexToMoveTo++;
            }
            if (hitx > groupX + groupWidths[groupIndexToMoveTo].Item2 / 2f)
            {
                groupIndexToMoveTo++;
            }

            List<Tuple<int, float, int>> temp = new List<Tuple<int, float, int>>(nbrOfGroups);
            // add the groups we are moving to a temporary list
            temp.AddRange(groupWidths.GetRange(selectedGroupLeft, nbrOfGroups));
            // we have to do this for both the groups and the cells
            // figure out the index that the first group is on in the cells list
            int cellsStartIndex = 0;
            foreach (Tuple<int, float, int> t in groupWidths.GetRange(0, selectedGroupLeft))
            {
                cellsStartIndex += t.Item3;
            }
            int cellsStartIndexToMoveTo = 0;
            foreach (Tuple<int, float, int> t in groupWidths.GetRange(0, groupIndexToMoveTo))
            {
                cellsStartIndexToMoveTo += t.Item3;
            }
            // figure out how many cells the groups cover in total
            int totalNbrOfCells = 0;
            foreach (Tuple<int, float, int> t in temp)
            {
                totalNbrOfCells += t.Item3;
            }
            List<Tuple<string, int>> tempCells = new List<Tuple<string, int>>(totalNbrOfCells);
            tempCells.AddRange(cells.GetRange(cellsStartIndex, totalNbrOfCells));
            groupWidths.RemoveRange(selectedGroupLeft, nbrOfGroups);
            cells.RemoveRange(cellsStartIndex, totalNbrOfCells);
            // the correct index to move the groups to will have changed if the groups are moved to higher indices
            if (groupIndexToMoveTo > selectedGroupRight)
            {
                groupIndexToMoveTo -= nbrOfGroups;
                cellsStartIndexToMoveTo -= totalNbrOfCells;
            }
            // insert them back in on the correct index
            cells.InsertRange(cellsStartIndexToMoveTo, tempCells);
            groupWidths.InsertRange(groupIndexToMoveTo, temp);
            recalculate = true;
        }

        if (bitmapHeight - hity < selectedBoxY || bitmapHeight - hity > selectedBoxY + selectedBoxHeight)
        {
            int nbrOfGenes = selectedGeneBottom - selectedGeneTop + 1;
            int geneIndex = (int)(((bitmapHeight - hity + ((float)heatmapHeight / genes.Length) / 2) - heatmapY) / heatmapHeight * genes.Length);
            // Take the list of genes orignal genes
            List<string> original = new List<string>(genes);
            // make a temporary list with enough space for what should be moved
            List<string> temp = new List<string>(nbrOfGenes);
            // add what should be moved to the temporary list
            temp.AddRange(original.GetRange(selectedGeneTop, nbrOfGenes));
            // remove what should be moved from the original list
            original.RemoveRange(selectedGeneTop, nbrOfGenes);
            // recalculate the index if needed. Since we removed stuff from the original list the indeces might have shifted
            if (geneIndex > selectedGeneTop)
            {
                geneIndex -= nbrOfGenes;
            }
            // insert what should be moved back into the original
            original.InsertRange(geneIndex, temp);
            genes = original.ToArray();
            recalculate = true;
        }
        if (recalculate)
        {
            // rebuild the heatmap texture
            BuildTexture(groupWidths, genes);
        }
        ResetSelection();
    }

    /// <summary>
    /// Resets the selection on the heatmap.
    /// </summary>
    private void ResetSelection()
    {
        confirmQuad.SetActive(false);
        movingQuadX.SetActive(false);
        movingQuadY.SetActive(false);

        selectedBoxX = 0;
        selectedBoxY = 0;
        selectedBoxHeight = 0;
        selectedBoxWidth = 0;

        selectedGeneBottom = 0;
        selectedGeneTop = 0;
        selectedGroupLeft = 0;
        selectedGroupRight = 0;
    }

    /// <summary>
    /// Updates this heatmap's image.
    /// </summary>
    [Obsolete("Use BuildTexture")]
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
            // unless you press this button way too many times the same second
            saveFileName += "_d";
        }
        bitmap.Save(saveFileName);
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
