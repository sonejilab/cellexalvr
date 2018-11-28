using SQLiter;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using VRTK;
using TMPro;
using System.IO;
using CellexalExtensions;

/// <summary>
/// This class represent a manager that holds all the cells.
/// </summary>
public class CellManager : MonoBehaviour
{
    #region Properties
    public string[] Attributes { get; set; }
    public string[] Facs { get; set; }
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
    private PreviousSearchesList previousSearchesList;
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
    private int coloringInfoStatusId;
    private Dictionary<Cell, int> recolored;
    private List<KeyValuePair<GraphPoint, int>> selectionList;


    void Awake()
    {
        cells = new Dictionary<string, Cell>();
    }

    private void Start()
    {
        CellexalEvents.GraphsReset.AddListener(GraphsChanged);
        CellexalEvents.GraphsUnloaded.AddListener(GraphsChanged);

        database = referenceManager.database;
        rightController = referenceManager.rightController;
        previousSearchesList = referenceManager.previousSearchesList;
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
        recolored = new Dictionary<Cell, int>();
        selectionList = new List<KeyValuePair<GraphPoint, int>>();
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
    /// <param name="groupingColors">Optional parameter, used if a custom color scheme should be used. Maps groups to colors.</param>
    public void CreateNewSelection(string graphName, string[] cellnames, int[] groups, Dictionary<int, Color> groupingColors = null)
    {
        selectionToolHandler.CancelSelection();
        Graph graph = graphManager.FindGraph(graphName);
        if (!graph)
        {
            graph = graphManager.FindGraph("");
        }
        if (groupingColors == null)
        {
            for (int i = 0; i < cellnames.Length; ++i)
            {
                Cell cell = cells[cellnames[i]];
                //cell.SetGroup(groups[i], true);
                selectionToolHandler.AddGraphpointToSelection(graph.points[cellnames[i]], groups[i], false);
                //graphManager.FindGraphPoint(graphName, cell.Label).SetOutLined(true, groups[i]);
            }
        }
        else
        {
            for (int i = 0; i < cellnames.Length; ++i)
            {
                Cell cell = cells[cellnames[i]];
                //cell.SetGroup(groups[i], false);
                selectionToolHandler.AddGraphpointToSelection(graph.points[cellnames[i]], groups[i], false, groupingColors[groups[i]]);
                //graphManager.FindGraphPoint(graphName, cell.Label).SetOutLined(true, groupingColors[groups[i]]);
            }
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
    /// Color all graphs by the expression of something.
    /// </summary>
    /// <param name="type">The type of the thing we are coloring by.</param>
    /// <param name="name">The name of what we are coloring by.</param>
    /// <param name="coloringMethod">The method of coloring that should be used.</param>
    public void ColorGraphsBy(Definitions.Measurement type, string name, GraphManager.GeneExpressionColoringMethods coloringMethod)
    {

        switch (type)
        {
            case Definitions.Measurement.GENE:
                ColorGraphsByGene(name, coloringMethod);
                gameManager.InformColorGraphsByGene(name);
                break;
            case Definitions.Measurement.ATTRIBUTE:
                ColorByAttribute(name, true);
                break;
            case Definitions.Measurement.FACS:
                ColorByIndex(name);
                break;
        }
    }

    [ConsoleCommand("cellManager", "colorbygene", "cbg")]
    public void ColorGraphsByGene(string geneName)
    {
        ColorGraphsByGene(geneName, graphManager.GeneExpressionColoringMethod, true);
    }

    /// <summary>
    /// Colors all GraphPoints in all current Graphs based on their expression of a gene.
    /// </summary>
    /// <param name="geneName"> The name of the gene. </param>
    public void ColorGraphsByGene(string geneName, bool triggerEvent = true)
    {
        ColorGraphsByGene(geneName, graphManager.GeneExpressionColoringMethod, triggerEvent);
    }

    /// <summary>
    /// Colors all GraphPoints in all current Graphs based on their expression of a gene.
    /// </summary>
    /// <param name="geneName"> The name of the gene. </param>
    public void ColorGraphsByGene(string geneName, GraphManager.GeneExpressionColoringMethods coloringMethod, bool triggerEvent = true)
    {
        try
        {
            StartCoroutine(QueryDatabase(geneName, coloringMethod, triggerEvent));

        }
        catch (Exception e)
        {
            CellexalLog.Log("Failed to colour by expression - " + e.StackTrace);
            CellexalError.SpawnError("Could not colour by gene expression", "Find stack trace in cellexal log");
        }
        if (rightController.isActiveAndEnabled)
        {
            controllerActions.TriggerHapticPulse(2000, (ushort)600, 0);
        }
    }

    private IEnumerator QueryDatabase(string geneName, GraphManager.GeneExpressionColoringMethods coloringMethod, bool triggerEvent)
    {
        if (coroutinesWaiting >= 1)
        {
            // If there is already another query  waiting for the current to finish we should probably abort.
            // This is just to make sure that a bug can't create many many coroutines that will form a long queue.
            CellexalLog.Log("WARNING: Not querying database for " + geneName + " because there is already a query waiting.");
            yield break;
        }
        coroutinesWaiting++;

        // if there is already a query running, wait for it to finish
        while (database.QueryRunning)
            yield return null;

        coroutinesWaiting--;
        database.QueryGene(geneName, coloringMethod);
        // now we have to wait for our query to return the results.
        while (database.QueryRunning)
            yield return null;

        GetComponent<AudioSource>().Play();
        SteamVR_Controller.Input((int)rightController.index).TriggerHapticPulse(2000);
        ArrayList expressions = database._result;
        // stop the coroutine if the gene was not in the database
        if (expressions.Count == 0)
        {
            CellexalLog.Log("WARNING: The gene " + geneName + " was not found in the database");
            yield break;
        }
        foreach (Cell c in cells.Values)
        {
            c.ColorByExpression(0);
            c.MakeTransparent();
        }

        //        Dictionary<string, int> sortedCells = new Dictionary<string, int>();
        //        for (int i = 0; i < expressions.Count; ++i)
        //        {
        //            Cell cell = cells[((CellExpressionPair)expressions[i]).Cell];
        //            //cell.Hide();
        //            cell.ColorByExpression((int)((CellExpressionPair)expressions[i]).Expression);
        //            sortedCells.Add(((CellExpressionPair)expressions[i]).Cell, (int)((CellExpressionPair)expressions[i]).Expression);
        //        }
        //
        //        int n = (int)Math.Round(0.01 * cells.Count);
        //        HighlightTopExpressedCells(sortedCells, n);
        //
        //        yield return new WaitForSeconds(2);
        //
        //        foreach (Cell c in cells.Values)
        //        {
        //            c.Show();
        //        }
        graphManager.ColorAllGraphsByGeneExpression(expressions);

        float percentInResults = (float)database._result.Count / cells.Values.Count;
        statusDisplay.RemoveStatus(coloringInfoStatusId);
        coloringInfoStatusId = statusDisplay.AddStatus(String.Format("Stats for {0}:\nlow: {1:0.####}, high: {2:0.####}, above 0: {3:0.##%}", geneName, database.LowestExpression, database.HighestExpression, percentInResults));

        if (!previousSearchesList.Contains(geneName, Definitions.Measurement.GENE, coloringMethod))
        {
            var removedGene = previousSearchesList.AddEntry(geneName, Definitions.Measurement.GENE, coloringMethod);
            foreach (Cell c in cells.Values)
            {
                c.SaveExpression(geneName + " " + coloringMethod, removedGene);
            }
        }
        if (triggerEvent)
        {
            CellexalEvents.GraphsColoredByGene.Invoke();
        }
        CellexalLog.Log("Colored " + expressions.Count + " points according to the expression of " + geneName);
    }

    /// <summary>
    /// Draws attention to the top expressed cells of the queried gene. 
    /// </summary>
    /// <param name="sortedCells">Sorted list of cells so the top ones can be picked.</param>
    /// <param name="nrOfCells">Nr of cells to highlight. E.g. Top 100 cells or top 10 cells.</param>
    private void HighlightTopExpressedCells(Dictionary<string, int> sortedCells, int nrOfCells)
    {
        var items = from pair in sortedCells
                    orderby pair.Value descending
                    select pair;

        var topCells = items.Take(nrOfCells);
        foreach (KeyValuePair<string, int> pair in topCells)
        {
            Cell cell = cells[pair.Key];
            cell.ColorByExpression(pair.Value);
            if (pair.Value > 0)
            {
                cell.Highlight();
            }
            //print(String.Format("{0}: {1}", pair.Key, pair.Value));
        }
    }

    /// <summary>
    /// Queries the database for all genes and sorts them based on the chosen mode.
    /// </summary>
    /// <param name="mode">The chosen mode. <see cref="SQLite.QueryTopGenesRankingMode"/></param>
    public void QueryTopGenes(SQLite.QueryTopGenesRankingMode mode)
    {
        StartCoroutine(QueryTopGenesCoroutine(mode));
    }

    private IEnumerator QueryTopGenesCoroutine(SQLite.QueryTopGenesRankingMode mode)
    {
        CellexalEvents.QueryTopGenesStarted.Invoke();
        while (database.QueryRunning)
        {
            yield return null;
        }

        database.QueryTopGenes(mode);

        while (database.QueryRunning)
        {
            yield return null;
        }
        Pair<string, float>[] results = (Pair<string, float>[])database._result.ToArray(typeof(Pair<string, float>));
        Array.Sort(results, (Pair<string, float> x, Pair<string, float> y) => y.Second.CompareTo(x.Second));
        string[] genes = new string[20];
        float[] values = new float[20];
        if (mode == SQLite.QueryTopGenesRankingMode.Mean)
        {
            for (int i = 0; i < 10; ++i)
            {
                genes[i] = results[i].First;
                values[i] = results[i].Second;
            }
            for (int i = 0; i < 10; ++i)
            {
                genes[i + 10] = results[results.Length - (i + 1)].First;
                values[i + 10] = results[results.Length - (i + 1)].Second;
            }
        }
        else if (mode == SQLite.QueryTopGenesRankingMode.TTest)
        {
            for (int i = 0; i < 10; ++i)
            {
                genes[i] = results[i].First;
                values[i] = results[i].Second;
            }
            for (int i = 0; i < 10; ++i)
            {
                genes[i + 10] = results[results.Length - (i + 1)].First;
                values[i + 10] = results[results.Length - (i + 1)].Second;
            }
        }
        CellexalLog.Log("Overwriting file: " + CellexalUser.UserSpecificFolder + "\\gene_expr_diff.txt with new results");
        StreamWriter stream = new StreamWriter(CellexalUser.UserSpecificFolder + "\\gene_expr_diff.txt", false);
        foreach (Pair<string, float> p in results)
        {
            stream.Write(p.First + "\t\t " + p.Second + "\n");
        }
        stream.Flush();
        stream.Close();

        CellexalEvents.QueryTopGenesFinished.Invoke();
        referenceManager.colorByGeneMenu.CreateGeneButtons(genes, values);
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
        CellexalLog.Log("Querying database for genes to flash");
        loadingFlashingGenes = true;
        CellexalEvents.FlashGenesFileStartedLoading.Invoke();
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
        CellexalLog.Log("Finished " + genes.Length + " queries in " + stopwatch.Elapsed.ToString());
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

        CellexalLog.Log("Number of genes that were present in the database:");
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
            CellexalLog.Log("\t" + categories[i] + ":\t" + lengths[i] + "/" + genes[i].Length + " \t(" + percentage + "%)");
            FlashGenesCategoryFilter[categories[i]] = true;
        }
        SavedFlashGenesCategories = categories;
        savedFlashGenesLengths = lengths;
        CellexalEvents.FlashGenesFileFinishedLoading.Invoke();
        loadingFlashingGenes = false;
        // StartCoroutine(FlashGenesCoroutine(categories, lengths));
    }

    private IEnumerator FlashGenesCoroutine()
    {
        CellexalLog.Log("Starting to flash genes");
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
                    CellexalLog.Log("Unknown flashing genes mode: " + CurrentFlashGenesMode);
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
        Attributes = null;
        Facs = null;
    }

    /// <summary>
    /// Color all cells that belong to a certain attribute.
    /// </summary>
    /// <param name="attributeType">The name of the attribute.</param>
    /// <param name="color">True if the graphpoints should be colored to the attribute's color, false if they should be white.</param>
    [ConsoleCommand("cellManager", "colorbyattribute", "cba")]
    public void ColorByAttribute(string attributeType, bool color)
    {
        //if (!previousSearchesList.Contains(attributeType, Definitions.Measurement.ATTRIBUTE, graphManager.GeneExpressionColoringMethod))
        //    previousSearchesList.AddEntry(attributeType, Definitions.Measurement.ATTRIBUTE, graphManager.GeneExpressionColoringMethod);
        CellexalLog.Log("Colored graphs by " + attributeType);
        foreach (Cell cell in cells.Values)
        {
            cell.ColorByAttribute(attributeType, color);
            GraphPoint gp = cell.GraphPoints[0];
            if (cell.Attributes.ContainsKey(attributeType.ToLower()))
            {
                if (color)
                {
                    selectionList.Add(new KeyValuePair<GraphPoint, int>(gp, cell.Attributes[attributeType.ToLower()]));
                    //graphManager.referenceManager.selectionToolHandler.AddGraphpointToSelection(cell.GraphPoints[0], cell.Attributes[attributeType.ToLower()], false, g.Material.color);
                }
                if (!color)
                {
                    selectionList.Remove(new KeyValuePair<GraphPoint, int>(gp, cell.Attributes[attributeType.ToLower()]));
                }
            }
        }

    }

    public void SendToSelection()
    {
        foreach (KeyValuePair<GraphPoint, int> entry in selectionList)
        {
            graphManager.referenceManager.selectionToolHandler.AddGraphpointToSelection(entry.Key, entry.Value, false, graphManager.AttributeMaterials[entry.Value].color);
        }
    }

    /// <summary>
    /// Colors all graphs based on a boolean expression of attributes.
    /// </summary>
    /// <param name="expression">The root of the tree representing a boolean expression of attributes.</param>
    public void ColorByAttributeLogic(BooleanExpression.Expr expression)
    {
        if (expression == null)
        {
            foreach (var cell in cells.Values)
            {
                if (recolored.ContainsKey(cell))
                    cell.SetGroup(recolored[cell], true);
                else
                    cell.ResetColor();

                cell.ColorByAttributeLogic(expression);
            }
        }
        else
        {
            foreach (var cell in cells.Values)
            {
                if (recolored.ContainsKey(cell))
                    cell.SetGroup(recolored[cell], true);
                else
                    cell.ResetColor();
            }
        }
    }

    /// <summary>
    /// Color all cells based on an expression of attributes
    /// </summary>
    /// <param name="expr">The root of the tree representing a boolean expression of attributes.</param>
    public void ColorByAttributeExpression(BooleanExpression.Expr expr)
    {
        foreach (var cell in cells.Values)
        {
            if (expr.Eval(cell))
            {
                cell.SetGroup(selectionToolHandler.currentColorIndex, true);
            }
            else
            {
                if (recolored.ContainsKey(cell))
                    cell.SetGroup(recolored[cell], true);
                else
                    cell.SetGroup(-1, true);
            }
        }
    }

    public void AddCellsToSelection(BooleanExpression.Expr attributes, int group)
    {
        if (attributes == null)
            return;
        int numAdded = 0;
        foreach (var cell in cells.Values)
        {
            if (attributes.Eval(cell))
            {
                numAdded++;
                // more_cells selectionToolHandler.AddGraphpointToSelection(cell.GraphPoints[0], group, false);
                recolored[cell] = selectionToolHandler.currentColorIndex;
            }
        }
        CellexalLog.Log("Added " + numAdded + " cells to selection");
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
        if (index < 0 || index >= CellexalConfig.NumberOfExpressionColors)
        {
            // value hasn't been normalized correctly
            print(facs + " " + index);
        }
        cells[cellName].AddFacs(facs, index);
    }

    /// <summary>
    /// Color all graphpoints according to a column in the index.facs file.
    /// </summary>
    [ConsoleCommand("cellManager", "colorbyindex", "cbi")]
    public void ColorByIndex(string name)
    {
        if (!previousSearchesList.Contains(name, Definitions.Measurement.FACS, graphManager.GeneExpressionColoringMethod))
            previousSearchesList.AddEntry(name, Definitions.Measurement.FACS, graphManager.GeneExpressionColoringMethod);
        CellexalLog.Log("Colored graphs by " + name);
        foreach (Cell cell in cells.Values)
        {
            cell.ColorByIndex(name);
        }
    }

    /// <summary>
    /// Draws lines between all points that share the same label.
    /// </summary>
    /// <param name="points"> The graphpoints to draw the lines from. </param>
    public void DrawLinesBetweenGraphPoints(List<CombinedGraph.CombinedGraphPoint> points)
    {
        // more_cells   foreach (GraphPoint g in points)
        // more_cells   {
        // more_cells       Color color = g.Material.color;
        // more_cells       foreach (GraphPoint sameCell in g.Cell.GraphPoints)
        // more_cells       {
        // more_cells           if (sameCell != g)
        // more_cells           {
        // more_cells               LineBetweenTwoPoints line = Instantiate(lineBetweenTwoGraphPointsPrefab).GetComponent<LineBetweenTwoPoints>();
        // more_cells               line.t1 = sameCell.transform;
        // more_cells               line.t2 = g.transform;
        // more_cells               line.graphPoint = g;
        // more_cells               line.selectionToolHandler = selectionToolHandler;
        // more_cells               LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
        // more_cells               lineRenderer.startColor = color;
        // more_cells               lineRenderer.endColor = color;
        // more_cells               lines.Add(line.gameObject);
        // more_cells               sameCell.Graph.Lines.Add(line.gameObject);
        // more_cells               g.Graph.Lines.Add(line.gameObject);
        // more_cells               if (!sameCell.Graph.GraphActive)
        // more_cells               {
        // more_cells                   line.gameObject.SetActive(false);
        // more_cells               }
        // more_cells               g.lineBetweenCellsCubes.Add(line.cube);
        // more_cells           }
        // more_cells       }
        // more_cells   }
    }

    public void DrawLinesBetweenGraphPoints(List<GraphPoint> points, Graph fromGraph, Graph toGraph)
    {
        foreach (GraphPoint g in points)
        {
            Color color = g.Material.color;
            var sourceCell = fromGraph.points[g.label];
            var toCell = toGraph.points[g.label];
            LineBetweenTwoPoints line = Instantiate(lineBetweenTwoGraphPointsPrefab).GetComponent<LineBetweenTwoPoints>();
            line.t1 = toCell.transform;
            line.t2 = sourceCell.transform;
            line.graphPoint = sourceCell;
            line.selectionToolHandler = selectionToolHandler;
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lines.Add(line.gameObject);
            fromGraph.Lines.Add(line.gameObject);
            toGraph.Lines.Add(line.gameObject);
            if (!toCell.Graph.GraphActive)
            {
                line.gameObject.SetActive(false);
            }

            g.lineBetweenCellsCubes.Add(line.cube);
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
            line.GetComponent<LineBetweenTwoPoints>().graphPoint.lineBetweenCellsCubes.Clear();
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

    private void GraphsChanged()
    {
        statusDisplay.RemoveStatus(coloringInfoStatusId);
        recolored.Clear();
        ClearLinesBetweenGraphPoints();


    }
}
