using SQLiter;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using CellexalVR.General;
using CellexalVR.Interaction;
using CellexalVR.AnalysisObjects;
using CellexalVR.DesktopUI;
using CellexalVR.Extensions;
using CellexalVR.AnalysisLogic.H5reader;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Entities;
using CellexalVR.AnalysisLogic;

namespace CellexalVR.AnalysisLogic
{
    /// <summary>
    /// This class represent a manager that holds all the cells.
    /// </summary>
    public class CellManager : MonoBehaviour
    {
        #region Properties

        public List<string> Attributes { get; set; }
        public string[] Facs { get; set; }
        public string[] Facs_values { get; set; }
        public string[] NumericalAttributes { get; set; }

        #endregion

        public float LowestExpression { get; private set; }
        public float HighestExpression { get; private set; }

        public ReferenceManager referenceManager;

        /// <summary>
        /// Lowest and highest range of facs measurements. <see cref="Tuple{T1, T2}.Item1"/> is the lowest value and <see cref="Tuple{T1, T2}.Item2"/> is the highest.
        /// </summary>
        public Dictionary<string, Tuple<float, float>> FacsRanges { get; private set; }
        public Dictionary<string, Tuple<float, float>> NumericalAttributeRanges { get; private set; }

        public Dictionary<string, GameObject> convexHulls = new Dictionary<string, GameObject>();

        public List<string> cellNames = new List<string>();
        private SQLite database;
        private ActionBasedController rightController;
        private PreviousSearchesList previousSearchesList;
        private Dictionary<string, Cell> cells;
        private SelectionToolCollider selectionToolCollider;
        private SelectionManager selectionManager;
        private GraphManager graphManager;
        private LineBundler lineBundler;
        private int coroutinesWaiting;
        private readonly List<string[]> prunedGenes = new List<string[]>();
        private Dictionary<Cell, int> recolored;
        private Dictionary<Graph.GraphPoint, int> selectionList;
        private AudioSource audioSource;


        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Awake()
        {
            cells = new Dictionary<string, Cell>();
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            CellexalEvents.GraphsReset.AddListener(GraphsChanged);
            CellexalEvents.GraphsUnloaded.AddListener(GraphsChanged);

            database = referenceManager.database;
            rightController = referenceManager.rightController;
            previousSearchesList = referenceManager.previousSearchesList;
            selectionManager = referenceManager.selectionManager;
            lineBundler = GetComponent<LineBundler>();
            selectionToolCollider = referenceManager.selectionToolCollider;
            graphManager = referenceManager.graphManager;
            recolored = new Dictionary<Cell, int>();
            selectionList = new Dictionary<Graph.GraphPoint, int>();
            FacsRanges = new Dictionary<string, Tuple<float, float>>();
            NumericalAttributeRanges = new Dictionary<string, Tuple<float, float>>();
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
        /// Finds cell and returns it.
        /// </summary>
        /// <param name="label">The label(id) of the cell.</param>
        /// <returns></returns>
        public Cell GetCell(string label)
        {
            return cells[label];
        }

        /// <summary>
        /// Returns all cells.
        /// </summary>
        /// <returns></returns>
        public Cell[] GetCells()
        {
            return cells.Values.ToArray();
        }

        /// <summary>
        /// Returns cells that belongs to a certain attribute.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public Cell[] GetCells(string attribute)
        {
            return cells.Values.Where(x => x.Attributes.ContainsKey(attribute.ToLower())).ToArray();
        }

        /// <summary>
        /// Returns cell that belong to a certain selection group.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public Cell[] GetCells(int group)
        {
            return cells.Values.Where(x => (x.GraphPoints[0].Group == group)).ToArray();
        }

        /// <summary>
        /// Returns cell that belong to a certain selection group within a certain sub selection of cells.
        /// </summary>
        /// <param name="group">The cells beloning to this particular group will be returned.</param>
        /// <param name="subSelection">The subselection of cells to select from.</param>
        /// <returns></returns>
        public Cell[] GetCells(int group, Cell[] subSelection)
        {
            return subSelection.Where(x => (x.GraphPoints[0].Group == group)).ToArray();
        }


        public int GetNumberOfCells()
        {
            return cells.Count;
        }

        public void HighlightCells(Cell[] cellsToHighlight, bool highlight)
        {
            foreach (Graph graph in graphManager.Graphs)
            {
                graph.MakeAllPointsTransparent(highlight);
            }

            foreach (Cell cell in cellsToHighlight)
            {
                foreach (Graph.GraphPoint gp in cell.GraphPoints)
                {
                    gp.HighlightGraphPoint(highlight);
                }
            }
        }

        public void HighlightCells(Cell[] cellsToHighlight, bool highlight, int group)
        {
            foreach (Graph graph in graphManager.Graphs)
            {
                graph.MakeAllPointsTransparent(highlight);
            }

            referenceManager.multiuserMessageSender.SendMessageHighlightCells(group, highlight);
            foreach (Cell cell in cellsToHighlight)
            {
                foreach (Graph.GraphPoint gp in cell.GraphPoints)
                {
                    gp.HighlightGraphPoint(highlight);
                }
            }
        }

        /// <summary>
        /// Creates a new selection.
        /// </summary>
        /// <param name="graphName"> The graph that the selection originated from. </param>
        /// <param name="cellnames"> An array of all the cell names (the graphpoint labels). </param>
        /// <param name="groups"> An array of all colors that the cells should have. </param>
        /// <param name="groupingColors">Optional parameter, used if a custom color scheme should be used. Maps groups to colors.</param>
        public void CreateNewSelection(string graphName, string[] cellnames, int[] groups,
            Dictionary<int, Color> groupingColors = null)
        {
            selectionManager.CancelSelection();
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
                    selectionManager.AddGraphpointToSelection(graph.points[cellnames[i]], groups[i], false);
                }
            }
            else
            {
                for (int i = 0; i < cellnames.Length; ++i)
                {
                    Cell cell = cells[cellnames[i]];
                    selectionManager.AddGraphpointToSelection(graph.points[cellnames[i]], groups[i], false,
                        groupingColors[groups[i]]);
                }
            }
        }


        [ConsoleCommand("cellManager", aliases: new string[] { "colorbygene", "cbg" })]
        public void ColorGraphsByGene(string geneName)
        {
            if (ScarfManager.instance.scarfActive)
            {
                StartCoroutine(ScarfManager.instance.ColorByGene(geneName));
            }
            else
            {
                ColorGraphsByGene(geneName, graphManager.GeneExpressionColoringMethod, true);
            }
            rightController.SendHapticImpulse(0.7f, 0.15f);
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
        public void ColorGraphsByGene(string geneName, GraphManager.GeneExpressionColoringMethods coloringMethod,
            bool triggerEvent = true)
        {
            try
            {
                KeyValuePair<string, H5Reader> kvp;
                if (referenceManager.inputReader.h5readers.Count > 0)
                {
                    kvp = referenceManager.inputReader.h5readers.First();
                    StartCoroutine(QueryHDF5(kvp.Value, geneName, coloringMethod, triggerEvent));
                }
                else
                {
                    StartCoroutine(QueryDatabase(geneName, coloringMethod, triggerEvent));
                }
            }
            catch (Exception e)
            {
                CellexalLog.Log("Failed to colour by expression - " + e.StackTrace);
                CellexalError.SpawnError("Could not colour by gene expression", "Find stack trace in cellexal log");
            }

            referenceManager.heatmapGenerator.HighLightGene(geneName);
            referenceManager.networkGenerator.HighLightGene(geneName);
        }

        /// <summary>
        /// query the h5 file for gene expression data
        /// </summary>
        /// <param name="geneName">Gene name</param>
        /// <param name="coloringMethod">coloring method</param>
        /// <param name="triggerEvent">trigger event?</param>
        /// <returns></returns>
        private IEnumerator QueryHDF5(H5Reader h5Reader, string geneName,
            GraphManager.GeneExpressionColoringMethods coloringMethod, bool triggerEvent)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();

            stopwatch.Start();
            /*
            if (coroutinesWaiting >= 1)
            {
                // If there is already another query  waiting for the current to finish we should probably abort.
                // This is just to make sure that a bug can't create many many coroutines that will form a long queue.
                CellexalLog.Log("WARNING: Not querying database for " + geneName + " because there is already a query waiting.");
                CellexalEvents.CommandFinished.Invoke(false);
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
            */
            try
            {
                StartCoroutine(h5Reader.ColorByGene(geneName, coloringMethod));
            }
            catch (Exception e)
            {
                print("bug" + e);
            }

            while (h5Reader.busy)
                yield return null;

            audioSource.Play();
            rightController.SendHapticImpulse(0.7f, 0.15f);
            ArrayList expressions = h5Reader._expressionResult;


            // stop the coroutine if the gene was not in the database
            if (expressions.Count == 0)
            {
                CellexalLog.Log("WARNING: The gene " + geneName + " was not found in the database");
                CellexalEvents.CommandFinished.Invoke(false);
                yield break;
            }

            graphManager.ColorAllGraphsByGeneExpression(geneName, expressions);

            if (!previousSearchesList.Contains(geneName, Definitions.Measurement.GENE, coloringMethod))
            {
                var removedGene = previousSearchesList.AddEntry(geneName, Definitions.Measurement.GENE, coloringMethod);
                foreach (Cell c in cells.Values)
                {
                    c.SaveExpression(geneName + " " + coloringMethod, removedGene);
                }
            }

            if (!referenceManager.sessionHistoryList.Contains(geneName, Definitions.HistoryEvent.GENE))
            {
                referenceManager.sessionHistoryList.AddEntry(geneName, Definitions.HistoryEvent.GENE);
            }

            if (triggerEvent)
            {
                CellexalEvents.GraphsColoredByGene.Invoke();
            }

            CellexalLog.Log("Colored " + expressions.Count + " points according to the expression of " + geneName);
            stopwatch.Stop();
            CellexalEvents.CommandFinished.Invoke(true);
            print("python3 - anndata.h5py " + stopwatch.ElapsedMilliseconds);
        }


        private IEnumerator QueryDatabase(string geneName, GraphManager.GeneExpressionColoringMethods coloringMethod,
            bool triggerEvent)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            if (coroutinesWaiting >= 1)
            {
                // If there is already another query  waiting for the current to finish we should probably abort.
                // This is just to make sure that a bug can't create many many coroutines that will form a long queue.
                CellexalLog.Log("WARNING: Not querying database for " + geneName +
                                " because there is already a query waiting.");
                CellexalEvents.CommandFinished.Invoke(false);
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
            ArrayList expressions = database._result;

            // stop the coroutine if the gene was not in the database
            if (expressions.Count == 0)
            {
                string message = "The gene " + geneName + " was not found in the database";
                CellexalLog.Log(message);
                StartCoroutine(referenceManager.geneKeyboard.ShowMessage(message, 5f));
                CellexalEvents.CommandFinished.Invoke(false);
                yield break;
            }

            graphManager.ColorAllGraphsByGeneExpression(geneName, expressions);
            if (!previousSearchesList.Contains(geneName, Definitions.Measurement.GENE, coloringMethod))
            {
                var removedGene = previousSearchesList.AddEntry(geneName, Definitions.Measurement.GENE, coloringMethod);
                foreach (Cell c in cells.Values)
                {
                    c.SaveExpression(geneName + " " + coloringMethod, removedGene);
                }
            }

            if (!referenceManager.sessionHistoryList.Contains(geneName, Definitions.HistoryEvent.GENE))
            {
                referenceManager.sessionHistoryList.AddEntry(geneName, Definitions.HistoryEvent.GENE);
            }

            if (triggerEvent)
            {
                CellexalEvents.GraphsColoredByGene.Invoke();
            }

            CellexalLog.Log("Colored " + expressions.Count + " points according to the expression of " + geneName);
            RScriptRunner.WriteToServer("# colored graphs by " + geneName);
            stopwatch.Stop();
            CellexalEvents.CommandFinished.Invoke(true);
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

            Pair<string, float>[] results =
                (Pair<string, float>[])database._result.ToArray(typeof(Pair<string, float>));
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

            CellexalLog.Log("Overwriting file: " + CellexalUser.UserSpecificFolder +
                            "\\gene_expr_diff.txt with new results");
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
            Attributes = null;
            Facs = null;
        }

        /// <summary>
        /// Color all cells that belong to a certain attribute.
        /// </summary>
        /// <param name="attributeType">The name of the attribute.</param>
        /// <param name="color">True if the graphpoints should be colored to the attribute's color, false if they should be white.</param>
        [ConsoleCommand("cellManager", aliases: new string[] { "colorbyattribute", "cba" })]
        public void ColorByAttribute(string attributeType, bool color, bool subGraph = false, int colIndex = 0)
        {
            if (!subGraph)
            {
                if (color)
                {
                    referenceManager.attributeSubMenu.attributes.Add(attributeType);
                }
                else
                {
                    referenceManager.attributeSubMenu.attributes.Remove(attributeType);
                }
            }

            //World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<TextureHandler>().ColorCluster(attributeType, color);

            CellexalLog.Log("Colored graphs by " + attributeType);
            RScriptRunner.WriteToServer("# colored graphs by " + attributeType);
            int numberOfCells = 0;

            foreach (Cell cell in cells.Values)
            {
                cell.ColorByAttribute(attributeType, colIndex, color);
                if (cell.GraphPoints.Count == 0) continue;
                Graph.GraphPoint gp = cell.GraphPoints[0];
                if (cell.Attributes.ContainsKey(attributeType.ToLower()))
                {
                    numberOfCells++;
                    if (color && !selectionList.ContainsKey(gp))
                    {
                        selectionList.Add(gp, colIndex);
                    }

                    if (!color)
                    {
                        selectionList.Remove(gp);
                    }


                }
            }

            int attributeIndex = Attributes.IndexOf(attributeType);
            Color attributeColor =
                CellexalConfig.Config.SelectionToolColors[
                    attributeIndex % CellexalConfig.Config.SelectionToolColors.Length];
            referenceManager.legendManager.desiredLegend = LegendManager.Legend.AttributeLegend;
            if (color)
            {
                referenceManager.legendManager.attributeLegend.AddEntry(attributeType, numberOfCells, attributeColor);
            }
            else
            {
                referenceManager.legendManager.attributeLegend.RemoveEntry(attributeType);
            }

            if (referenceManager.legendManager.currentLegend != referenceManager.legendManager.desiredLegend)
            {
                referenceManager.legendManager.ActivateLegend(referenceManager.legendManager.desiredLegend);
            }

            CellexalEvents.CommandFinished.Invoke(true);
        }

        /// <summary>
        /// Used by <see cref="ScarfManager"/>. Adds or removes cells to the current selection.
        /// </summary>
        /// <param name="cellClusters">An array mapping a cellname (index) to a cluster.</param>
        /// <param name="color">True if the cells in <paramref name="cellClusters"/> should be added to the current the selection, false if they should be removed."/></param>
        public void ColorAllClusters(float[] cellClusters, bool color)
        {
            int cluster;
            for (int i = 0; i < cellClusters.Length; i++)
            {
                cluster = (int)cellClusters[i];
                Cell c = cells[i.ToString()];
                if (color && !selectionList.ContainsKey(c.GraphPoints[0]))
                {
                    selectionList.Add(c.GraphPoints[0], cluster);
                }

                if (!color)
                {
                    selectionList.Remove(c.GraphPoints[0]);
                }
                c.ColorByCluster(cluster, true);

            }
        }

        /// <summary>
        /// Used be <see cref="ScarfManager"/>. Colors the graphs based off the colors in <paramref name="values"/>. Indices are cellnames
        /// </summary>
        /// <param name="values">An array of floats mapping cellnames (indices) to an expression value.</param>
        public void ColorByGene(float[] values)
        {
            Dictionary<int, float> valuesNoZeroes = new Dictionary<int, float>();
            ArrayList expressions = new ArrayList();
            LowestExpression = float.MaxValue;
            HighestExpression = float.MinValue;
            for (int i = 0; i < values.Length; i++)
            {
                float val = values[i];
                if (val > 0)
                {
                    valuesNoZeroes[i] = val;
                    CellExpressionPair pair = new CellExpressionPair(i.ToString(), val, -1);
                    expressions.Add(pair);
                }
                else
                    continue;
                if (val < LowestExpression)
                    LowestExpression = val;
                if (val > HighestExpression)
                    HighestExpression = val;
            }
            if (LowestExpression == HighestExpression)
                HighestExpression += 1f;

            HighestExpression *= 1.0001f;
            float binSize = (HighestExpression - LowestExpression) / CellexalConfig.Config.GraphNumberOfExpressionColors;

            foreach (CellExpressionPair pair in expressions)
            {
                pair.Color = (int)((pair.Expression - LowestExpression) / binSize);
            }
            ReferenceManager.instance.graphManager.ColorAllGraphsByGeneExpression("CD14", expressions);
        }

        /// <summary>
        /// Adds the currently selected attributes as a selection.
        /// </summary>
        public void SendToSelection()
        {
            foreach (KeyValuePair<Graph.GraphPoint, int> entry in selectionList)
            {
                selectionManager.AddGraphpointToSelection(entry.Key, entry.Value, false);
            }
        }

        /// <summary>
        /// Color all cells based on an expression of attributes
        /// </summary>
        /// <param name="expr">The root of the tree representing a boolean expression of attributes.</param>
        public void ColorByAttributeExpression(BooleanExpression.Expr expr)
        {
            if (expr == null)
            {
                graphManager.ResetGraphsColor();
            }

            foreach (var cell in cells.Values)
            {
                if (expr.Eval(cell))
                {
                    cell.SetGroup(selectionToolCollider.CurrentColorIndex, true);
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

        /// <summary>
        /// Adds an attribute to a cell. 
        /// </summary>
        /// <param name="cellname"> The cells name. </param>
        /// <param name="attributeType"> The attribute type / name </param>
        /// <param name="group"> The attribute value </param>
        public void AddAttribute(string cellname, string attributeType, int group)
        {
            try
            {
                cells[cellname].AddAttribute(attributeType, group);
            }
            catch (Exception e)
            {
                // could not find cell
                // CellexalLog.Log($"Could not find cell : {cellname}"); // if many points are missing this logging them becomes very laggy...
            }
        }

        internal void AddFacs(string cellName, string facs, float value)
        {
            cells[cellName].AddFacs(facs, value);
        }

        internal void AddFacsValue(string cellName, string facs, string value)
        {
            cells[cellName].AddFacsValue(facs, value);
        }

        internal void AddNumericalAttribute(string cellName, string attribute, float value)
        {
            cells[cellName].AddNumericalAttribute(attribute, value);
        }


        /// <summary>
        /// Color all graphpoints according to a column in the index.facs file.
        /// </summary>
        [ConsoleCommand("cellManager", aliases: new string[] { "colorbyindex", "cbi" })]
        public void ColorByIndex(string name)
        {
            if (!previousSearchesList.Contains(name, Definitions.Measurement.FACS, graphManager.GeneExpressionColoringMethod))
            {
                previousSearchesList.AddEntry(name, Definitions.Measurement.FACS, graphManager.GeneExpressionColoringMethod);
            }

            CellexalLog.Log("Colored graphs by " + name);
            RScriptRunner.WriteToServer("# colored graphs by " + name);
            if (CellexalConfig.Config.GraphMostExpressedMarker)
            {
                foreach (Graph graph in graphManager.Graphs)
                {
                    graph.ClearTopExprCircles();
                }
            }

            name = name.ToLower();
            int nColors = CellexalConfig.Config.GraphNumberOfExpressionColors;
            foreach (Cell cell in cells.Values)
            {
                Tuple<float, float> range = FacsRanges[name];
                float expr = cell.Facs[name];
                int group = 0;
                if (expr == range.Item2)
                {
                    group = nColors - 1;
                }
                else
                {
                    group = (int)((cell.Facs[name] - range.Item1) / (range.Item2 - range.Item1) * nColors);
                }

                cell.ColorByGeneExpression(group);
            }

            if (!referenceManager.sessionHistoryList.Contains(name, Definitions.HistoryEvent.FACS))
            {
                referenceManager.sessionHistoryList.AddEntry(name, Definitions.HistoryEvent.FACS);
            }

            CellexalEvents.GraphsColoredByIndex.Invoke();
            CellexalEvents.CommandFinished.Invoke(true);
            rightController.SendHapticImpulse(0.7f, 0.15f);
        }

        [ConsoleCommand("cellManager", aliases: new string[] { "colorclusters", "cc" })]
        public void ColorClusters(string path)
        {
            if (!File.Exists(path))
            {
                CellexalLog.Log("Could not find file:" + path);
                return;
            }

            int numPointsAdded = 0;
            using (StreamReader streamReader = new StreamReader(path))
            {
                string header = streamReader.ReadLine();
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();
                    string[] words = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    Cell cell = GetCell(words[0]);
                    cell.ColorByCluster(int.Parse(words[1]), true);
                    numPointsAdded++;
                }
            }

            CellexalEvents.CommandFinished.Invoke(true);
            CellexalEvents.SelectedFromFile.Invoke();
            File.Delete(path);
            PythonInterpreter.WriteToOutput($"Colored {numPointsAdded} graph points according to cluster...");
        }

        public void ColorByNumericalAttribute(string name)
        {
            name = name.ToLower();
            int nColors = CellexalConfig.Config.GraphNumberOfExpressionColors;
            foreach (Cell cell in cells.Values)
            {
                Tuple<float, float> range = NumericalAttributeRanges[name];
                float expr = cell.NumericalAttributes[name];
                int group = 0;
                if (expr == range.Item2)
                {
                    group = nColors - 1;
                }
                else
                {
                    group = (int)((cell.NumericalAttributes[name] - range.Item1) / (range.Item2 - range.Item1) * nColors);
                }
                cell.ColorByGeneExpression(group);
            }

            CellexalEvents.GraphsColoredByIndex.Invoke();
            CellexalEvents.CommandFinished.Invoke(true);
        }

        private void GraphsChanged()
        {
            //statusDisplay.RemoveStatus(coloringInfoStatusId);
            recolored.Clear();
            lineBundler.ClearLinesBetweenGraphPoints();
            referenceManager.attributeSubMenu.attributes.Clear();
            selectionList.Clear();
        }

        /// <summary>
        /// Returns a subset of all cells in the dataset based on a boolean expression.
        /// </summary>
        public List<Cell> SubSet(BooleanExpression.Expr expr)
        {
            List<Cell> result = new List<Cell>();
            foreach (Cell cell in cells.Values)
            {
                if (expr.Eval(cell))
                {
                    result.Add(cell);
                }
            }

            return result;
        }
    }
}