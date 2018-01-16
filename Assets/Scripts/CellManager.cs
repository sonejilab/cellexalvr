using SQLiter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using VRTK;
using TMPro;

/// <summary>
/// This class represent a manager that holds all the cells.
/// </summary>
public class CellManager : MonoBehaviour
{
    #region Properties
    /// <summary>
    /// The number of frames to wait in between each shown gene expression when flashing genes.
    /// </summary>
    public int FramesBetweenEachFlash
    {
        get
        {
            return framesBetweenEachFlash;
        }
        set
        {
            if (value > 0)
            {
                framesBetweenEachFlash = value;
            }
        }
    }
    private int framesBetweenEachFlash = 2;

    /// <summary>
    /// The number of seconds to display each category when flashing genes.
    /// </summary>
    public float SecondsBetweenEachCategory;

    /// <summary>
    /// The mode for flashing flashing genes.
    /// The available options are:
    /// <list type="bullet">
    ///   <item>
    ///     <term>DoNotFlash</term>
    ///     <description>No flashing.</description>
    ///   </item>
    ///   <item>
    ///     <term>RandomWithinCategory</term>
    ///     <description>Flashes random genes from a category. Waits <see cref="SecondsBetweenEachCategory"/> seconds before switching to the next category.</description>
    ///   </item>
    ///   <item>
    ///     <term>ShuffledCategory</term>
    ///     <description>Shows the expression of every gene exactly once, in a random order for each category.</description>
    ///   </item>
    ///   <item>
    ///     <term>OrderedCategory</term>
    ///     <description>Flashes the genes in order (order that they are in the database).</description>
    ///   </item>
    /// </list>
    /// </summary>
    public FlashGenesMode CurrentFlashGenesMode
    {
        get
        {
            return currentFlashGenesMode;
        }
        set
        {
            currentFlashGenesMode = value;
            if (value != FlashGenesMode.DoNotFlash && !flashingGenes && !loadingFlashingGenes && SavedFlashGenesCategories.Length != 0)
            {
                StartCoroutine(FlashGenesCoroutine());
            }
        }
    }
    private FlashGenesMode currentFlashGenesMode;
    public enum FlashGenesMode { DoNotFlash, RandomWithinCategory, ShuffledCategory, Ordered/*, StepForwardOneGene, StepBackwardOneGene */};
    public string[] SavedFlashGenesCategories { get; set; }
    public Dictionary<string, bool> FlashGenesCategoryFilter { get; private set; }
    #endregion

    public ReferenceManager referenceManager;
    public VRTK_ControllerActions controllerActions;
    public GameObject lineBetweenTwoGraphPointsPrefab;

    private SQLite database;
    private SteamVR_TrackedObject rightController;
    private PreviousSearchesListNode topListNode;
    private Dictionary<string, Cell> cells;
    private List<GameObject> lines = new List<GameObject>();
    private GameManager gameManager;
    private SelectionToolHandler selectionToolHandler;
    private GraphManager graphManager;
    private StatusDisplay statusDisplay;
    private StatusDisplay statusDisplayHUD;
    private StatusDisplay statusDisplayFar;
    private int coroutinesWaiting;
    private TextMesh currentFlashedGeneText;
    private GameObject HUD;
    private GameObject FarDisp;
    private TextMeshProUGUI HUDflashInfo;
    private TextMeshProUGUI HUDgroupInfo;
    private TextMeshProUGUI HUDstatus;
    private TextMeshProUGUI FarFlashInfo;
    private TextMeshProUGUI FarGroupInfo;
    private TextMeshProUGUI FarStatus;
    private List<string[]> prunedGenes = new List<string[]>();
    private bool flashingGenes = false;
    private bool loadingFlashingGenes;
    private int[] savedFlashGenesLengths;

    void Awake()
    {
        cells = new Dictionary<string, Cell>();
    }

    private void Start()
    {
        database = referenceManager.database;
        rightController = referenceManager.rightController;
        topListNode = referenceManager.topListNode;
        gameManager = referenceManager.gameManager;
        statusDisplay = referenceManager.statusDisplay;
        statusDisplayHUD = referenceManager.statusDisplayHUD;
        statusDisplayFar = referenceManager.statusDisplayFar;
        selectionToolHandler = referenceManager.selectionToolHandler;
        graphManager = referenceManager.graphManager;
        currentFlashedGeneText = referenceManager.currentFlashedGeneText;
        HUD = referenceManager.HUD;
        HUDflashInfo = referenceManager.HUDFlashInfo;
        HUDgroupInfo = referenceManager.HUDGroupInfo;
        FarDisp = referenceManager.FarDisplay;
        FarFlashInfo = referenceManager.FarFlashInfo;
        FarGroupInfo = referenceManager.FarGroupInfo;
        FlashGenesCategoryFilter = new Dictionary<string, bool>();
    }

    /// <summary>
    /// Attempts to add a cell to the dictionary
    /// </summary>
    /// <param name="label"> The cell's name </param>
    /// <returns> Returns a reference to the added cell </returns>
    public Cell AddCell(string label)
    {
        if (!cells.ContainsKey(label))
        {
            cells[label] = new Cell(label, graphManager);
        }
        return cells[label];
    }

    /// <summary>
    /// Creates a new selection.
    /// </summary>
    /// <param name="graphName"> The graph that the selection originated from. </param>
    /// <param name="cellnames"> An array of all the cell names (the graphpoint labels). </param>
    /// <param name="groups"> An array of all colors that the cells should have. </param>
    public void CreateNewSelection(string graphName, string[] cellnames, int[] groups)
    {
        selectionToolHandler.CancelSelection();
        Graph graph = graphManager.FindGraph(graphName);
        for (int i = 0; i < cellnames.Length; ++i)
        {
            Cell cell = cells[cellnames[i]];
            selectionToolHandler.AddGraphpointToSelection(graph.points[cellnames[i]], groups[i], false);
            cell.SetGroup(groups[i]);
        }
    }

    /// <summary>
    /// Toggles all cells which have an expression level > 0 by showing / hiding them from the graphs.
    /// </summary>
    public void ToggleExpressedCells()
    {
        foreach (Cell c in cells.Values)
        {
            if (c.ExpressionLevel > 0)
            {
                c.ToggleGraphPoints();
            }
        }
    }
    /// <summary>
    /// Toggles all cells which have an expression level == 0 by showing / hiding them from the graphs.
    /// </summary>
    public void ToggleNonExpressedCells()
    {
        foreach (Cell c in cells.Values)
        {
            if (c.ExpressionLevel == 0)
            {
                c.ToggleGraphPoints();
            }
        }
    }

    public Cell GetCell(string label)
    {
        return cells[label];
    }

    /// <summary>
    /// Color all cells based on a gene previously colored by
    /// </summary>
    public void ColorGraphsByPreviousExpression(string geneName)
    {
        foreach (Cell c in cells.Values)
        {
            c.ColorByPreviousExpression(geneName);
        }
        GetComponent<AudioSource>().Play();
        //Debug.Log("FEEL THE PULSE");
        SteamVR_Controller.Input((int)rightController.index).TriggerHapticPulse(2000);
    }

    /// <summary>
    /// Colors all GraphPoints in all current Graphs based on their expression of a gene.
    /// </summary>
    /// <param name="geneName"> The name of the gene. </param>
    public void ColorGraphsByGene(string geneName)
    {
        //SteamVR_Controller.Input((int)right.controllerIndex).TriggerHapticPulse(2000);
        controllerActions.TriggerHapticPulse(2000, (ushort)600, 0);
        StartCoroutine(QueryDatabase(geneName));
    }

    private IEnumerator QueryDatabase(string geneName)
    {
        if (coroutinesWaiting >= 1)
        {
            // If there is already another query  waiting for the current to finish we should probably abort.
            // This is just to make sure that a bug can't create many many coroutines that will form a long queue.
            CellExAlLog.Log("WARNING: Not querying database for " + geneName + " because there is already a query waiting.");
            yield break;
        }
        coroutinesWaiting++;

        // if there is already a query running, wait for it to finish
        while (database.QueryRunning)
            yield return null;

        coroutinesWaiting--;
        database.QueryGene(geneName);
        // now we have to wait for our query to return the results.
        while (database.QueryRunning)
            yield return null;

        GetComponent<AudioSource>().Play();
        SteamVR_Controller.Input((int)rightController.index).TriggerHapticPulse(2000);
        ArrayList expressions = database._result;
        // stop the coroutine if the gene was not in the database
        if (expressions.Count == 0)
        {
            CellExAlLog.Log("WARNING: The gene " + geneName + " was not found in the database");
            yield break;
        }
        foreach (Cell c in cells.Values)
        {
            c.ColorByExpression(0);
        }
        for (int i = 0; i < expressions.Count; ++i)
        {
            Cell cell = cells[((CellExpressionPair)expressions[i]).Cell];
            cell.Show();
            cell.ColorByExpression((int)((CellExpressionPair)expressions[i]).Expression);
        }

        var removedGene = topListNode.UpdateList(geneName + " " + graphManager.GeneExpressionColoringMethod);
        //Debug.Log(topListNode.GeneName);
        foreach (Cell c in cells.Values)
        {
            c.SaveExpression(geneName + " " + graphManager.GeneExpressionColoringMethod, removedGene);
        }
        CellExAlEvents.GraphsColoredByGene.Invoke();
        CellExAlLog.Log("Colored " + expressions.Count + " points according to the expression of " + geneName);
    }

    /// <summary>
    /// Prepares the cellmanager to flash some gene expressions.
    /// </summary>
    /// <param name="genes"> An array of arrays of strings containing the genes to flash.
    /// Each array (genes[x] for any x) should contain a category.
    /// The first element in each array (genes[x][0] for any x) should contain the the category name, the rest of the array should contain the gene names to flash.
    /// A gene may be in more than one category.</param>
    public void SaveFlashGenesData(string[][] genes)
    {
        StartCoroutine(GetGeneExpressionsToFlashCoroutine(genes));
    }

    /// <summary>
    /// Queries the database for the gene expressions that should be flashed.
    /// </summary>
    /// <param name="genes"> An array of arrays of strings containing the genes to flash.
    /// Each array (genes[x] for any x) should contain a category.
    /// The first element in each array (genes[x][0] for any x) should contain the the category name, the rest of the array should contain the gene names to flash.
    /// A gene may be in more than one category.</param>
    private IEnumerator GetGeneExpressionsToFlashCoroutine(string[][] genes)
    {
        CellExAlLog.Log("Querying database for genes to flash");
        loadingFlashingGenes = true;
        CellExAlEvents.FlashGenesFileStartedLoading.Invoke();
        prunedGenes.Clear();
        foreach (Cell c in cells.Values)
        {
            c.ClearFlashingExpressions();
        }
        string[] categories = new string[genes.Length];
        int i = 0;
        int statusid = statusDisplay.AddStatus("");
        int statusidHUD = statusDisplayHUD.AddStatus("");
        int statusidFar = statusDisplayFar.AddStatus("");
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        for (; i < genes.Length; ++i)
        {
            statusDisplay.UpdateStatus(statusid, "Query " + (i + 1) + "/" + genes.Length + " in progress");
            statusDisplayHUD.UpdateStatus(statusidHUD, "Query " + (i + 1) + "/" + genes.Length + " in progress");
            statusDisplayFar.UpdateStatus(statusidFar, "Query " + (i + 1) + "/" + genes.Length + " in progress");
            string[] categoryOfGenes = genes[i];
            categories[i] = categoryOfGenes[0];
            database.QueryMultipleGenesFlashingExpression(categoryOfGenes);

            // now we have to wait for our query to return the results.
            while (database.QueryRunning)
                yield return null;
        }
        stopwatch.Stop();
        CellExAlLog.Log("Finished " + genes.Length + " queries in " + stopwatch.Elapsed.ToString());
        statusDisplay.RemoveStatus(statusid);
        statusDisplayHUD.RemoveStatus(statusidHUD);
        statusDisplayFar.RemoveStatus(statusidFar);
        Cell cell = null;
        foreach (Cell c in cells.Values)
        {
            // This is a dumb way of getting a cell.
            cell = c;
            break;
        }

        CellExAlLog.Log("Number of genes that were present in the database:");
        Dictionary<string, int> categoryLengths = cell.GetCategoryLengths();
        FlashGenesCategoryFilter.Clear();
        int[] lengths = new int[categories.Length];
        for (i = 0; i < categories.Length; ++i)
        {
            lengths[i] = categoryLengths[categories[i]];
            string percentage = ((lengths[i] * 100f) / genes[i].Length).ToString();
            if (percentage.Length > 5)
            {
                percentage = percentage.Substring(0, 5);
            }
            CellExAlLog.Log("\t" + categories[i] + ":\t" + lengths[i] + "/" + genes[i].Length + " \t(" + percentage + "%)");
            FlashGenesCategoryFilter[categories[i]] = true;
        }
        SavedFlashGenesCategories = categories;
        savedFlashGenesLengths = lengths;
        CellExAlEvents.FlashGenesFileFinishedLoading.Invoke();
        loadingFlashingGenes = false;
        // StartCoroutine(FlashGenesCoroutine(categories, lengths));
    }

    private IEnumerator FlashGenesCoroutine()
    {
        CellExAlLog.Log("Starting to flash genes");
        flashingGenes = true;
        System.Random rng = new System.Random();
        while (CurrentFlashGenesMode != FlashGenesMode.DoNotFlash)
        {
            // Go through each category
            for (int i = 0; i < SavedFlashGenesCategories.Length; ++i)
            {
                string category = SavedFlashGenesCategories[i];
                // make sure this category is activated
                if (!FlashGenesCategoryFilter[category])
                {
                    // make sure there is atleast one category activated
                    if (!FlashGenesCategoryFilter.ContainsValue(true))
                    {
                        // if there is not at least one category activated, we wait one frame to not get stuck in an infinite loop
                        yield return null;
                    }
                    continue;
                }

                if (CurrentFlashGenesMode == FlashGenesMode.RandomWithinCategory)
                {
                    // Flash genes within this category for 10 seconds
                    var timeStarted = Time.time;
                    var timeToStop = timeStarted + 10f;
                    while (Time.time < timeToStop && CurrentFlashGenesMode == FlashGenesMode.RandomWithinCategory)
                    {
                        int randomGene = rng.Next(0, savedFlashGenesLengths[i]);
                        currentFlashedGeneText.text = "Category:\t" + category + "\nGene:\t\t" + prunedGenes[i][randomGene];
                        foreach (Cell c in cells.Values)
                        {
                            c.ColorByGeneInCategory(category, randomGene);
                        }
                        for (int j = 0; j < FramesBetweenEachFlash; ++j)
                            yield return null;
                    }
                }
                else if (CurrentFlashGenesMode == FlashGenesMode.ShuffledCategory)
                {
                    // Shuffle a category.
                    int[] geneOrder = new int[savedFlashGenesLengths[i]];
                    int j = 0;
                    // Fill the array with {0, 1, 2...}
                    for (; j < geneOrder.Length; ++j)
                    {
                        geneOrder[j] = j;
                    }
                    // Fisher-Yates shuffling algorithm
                    for (j = 0; j < geneOrder.Length - 2; ++j)
                    {
                        int k = rng.Next(j, geneOrder.Length);
                        int tmp = geneOrder[j];
                        geneOrder[j] = geneOrder[k];
                        geneOrder[k] = tmp;
                    }
                    // Go through the array of shuffled indices
                    for (j = 0; j < geneOrder.Length && CurrentFlashGenesMode == FlashGenesMode.ShuffledCategory; ++j)
                    {
                        currentFlashedGeneText.text = "Category:\t" + category + "\nGene:\t\t" + prunedGenes[i][j];
                        foreach (Cell c in cells.Values)
                        {
                            c.ColorByGeneInCategory(category, geneOrder[j]);
                        }
                        for (int k = 0; k < FramesBetweenEachFlash; ++k)
                            yield return null;
                    }
                }
                else if (CurrentFlashGenesMode == FlashGenesMode.Ordered)
                {

                    for (int j = 0; j < savedFlashGenesLengths[i] && CurrentFlashGenesMode == FlashGenesMode.Ordered; ++j)
                    {
                        if (HUD.activeSelf)
                        {
                            HUDflashInfo.text = "Category: " + category + "\nGene: " + prunedGenes[i][j];
                        }
                        if (FarDisp.activeSelf)
                        {
                            FarFlashInfo.text = "Category: " + category + "\nGene: " + prunedGenes[i][j];
                        }
                        currentFlashedGeneText.text = "Category:\t" + category + "\nGene:\t\t" + prunedGenes[i][j];
                        foreach (Cell c in cells.Values)
                        {
                            c.ColorByGeneInCategory(category, j);
                        }
                        for (int k = 0; k < FramesBetweenEachFlash; ++k)
                            yield return null;
                    }
                }
                else
                {
                    CellExAlLog.Log("Unknown flashing genes mode: " + CurrentFlashGenesMode);
                    flashingGenes = false;
                    yield break;
                }
            }
        }
        flashingGenes = false;
    }

    /// <summary>
    /// Used by the database to tell the cellmanager which genes were actually in the database.
    /// </summary>
    /// <param name="genesToAdd"> An array of genes that was in the database. </param>
    public void AddToPrunedGenes(string[] genesToAdd)
    {
        prunedGenes.Add(genesToAdd);
    }

    /// <summary>
    /// Removes all cells.
    /// </summary>
    public void DeleteCells()
    {
        cells.Clear();
        SavedFlashGenesCategories = new string[0];
    }

    /// <summary>
    /// Color all cells that belong to a certain attribute.
    /// </summary>
    public void ColorByAttribute(string attributeType, bool color)
    {
        CellExAlLog.Log("Colored genes by " + attributeType);
        foreach (Cell cell in cells.Values)
        {
            cell.ColorByAttribute(attributeType, color);
        }
    }

    /// <summary>
    /// Adds an attribute to a cell. 
    /// </summary>
    /// <param name="cellname"> The cells name. </param>
    /// <param name="attributeType"> The attribute type / name </param>
    /// <param name="group"> The attribute value </param>
    public void AddAttribute(string cellname, string attributeType, int group)
    {
        cells[cellname].AddAttribute(attributeType, group);
    }

    internal void AddFacs(string cellName, string facs, int index)
    {
        if (index < 0 || index >= CellExAlConfig.NumberOfExpressionColors)
        {
            // value hasn't been normalized correctly
            print(facs + " " + index);
        }
        cells[cellName].AddFacs(facs, index);
    }

    /// <summary>
    /// Color all graphpoints according to a column in the index.facs file.
    /// </summary>
    public void ColorByIndex(string name)
    {
        CellExAlLog.Log("Colored genes by " + name);
        foreach (Cell cell in cells.Values)
        {
            cell.ColorByIndex(name);
        }
    }
    /// <summary>
    /// Draws lines between all points that share the same label.
    /// </summary>
    /// <param name="points"> The graphpoints to draw the lines from. </param>
    public void DrawLinesBetweenGraphPoints(List<GraphPoint> points)
    {
        foreach (GraphPoint g in points)
        {
            Color color = g.Material.color;
            foreach (GraphPoint sameCell in g.Cell.GraphPoints)
            {
                if (sameCell != g)
                {
                    LineBetweenTwoPoints line = Instantiate(lineBetweenTwoGraphPointsPrefab).GetComponent<LineBetweenTwoPoints>();
                    line.t1 = sameCell.transform;
                    line.t2 = g.transform;
                    LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
                    lineRenderer.startColor = color;
                    lineRenderer.endColor = color;
                    lines.Add(line.gameObject);
                    sameCell.Graph.Lines.Add(line.gameObject);
                    g.Graph.Lines.Add(line.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Removes all lines between graphs.
    /// </summary>
    public void ClearLinesBetweenGraphPoints()
    {
        foreach (GameObject line in lines)
        {
            Destroy(line, 0.05f);
        }

        lines.Clear();
        graphManager.ClearLinesBetweenGraphs();
    }

    /// <summary>
    /// Saves a series of expressions that should be flashed.
    /// </summary>
    /// <param name="cell"> The cell that these expressions belong to. </param>
    /// <param name="category"> The expressions' category. </param>
    /// <param name="expr"> An array containing integers int he range [0,29] that denotes the cell's expression of the gene corresponding to that index. </param>
    internal void SaveFlashingExpression(string[] cell, string category, int[][] expr)
    {
        for (int i = 0; i < cell.Length; ++i)
        {
            cells[cell[i]].InitSaveSingleFlashingGenesExpression(category, expr.Length);
        }

        for (int i = 0; i < expr.Length; ++i)
        {
            for (int j = 0; j < expr[i].Length; ++j)
            {
                cells[cell[j]].SaveSingleFlashingGenesExpression(category, i, expr[i][j]);
            }
        }
    }
}
