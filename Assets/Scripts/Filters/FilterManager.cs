using CellexalVR.AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.DesktopUI;
using CellexalVR.General;
using CellexalVR.SceneObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

namespace CellexalVR.Filters
{
    public class FilterManager : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject wirePrefab;
        public FilterCreatorResultBlock resultBlock;
        public TextMeshPro filterPreviewText;
        public Filter currentFilter;

        private List<Tuple<Graph.GraphPoint, int>> queuedCells = new List<Tuple<Graph.GraphPoint, int>>(256);
        private List<Tuple<Graph.GraphPoint, int>> cellsToEvaluate = new List<Tuple<Graph.GraphPoint, int>>(256);
        private Coroutine runningSwapPercentCoroutine;
        private bool evaluating = false;
        private string currentFilterPath;
        private string[] currentFilterGenes;
        private FilterCreatorBlockPort previouslyClickedPort;
        private GameObject previewWire;
        private bool portClickedThisFrame = false;
        private bool cullingFilter = false;

        // Key.Item1 is a gene name, Key.Item2 is a cell name, Value is the expression
        public Dictionary<Tuple<string, string>, float> GeneExprs { get; set; }

        private void Start()
        {
            GeneExprs = new Dictionary<Tuple<string, string>, float>(new TupleComparer());
            previewWire = Instantiate(wirePrefab, this.transform);
            previewWire.SetActive(false);
            CellexalEvents.GraphsUnloaded.AddListener(OnGraphsUnloaded);
            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
        }

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
                if (referenceManager.filterBlockBoard)
                {
                    resultBlock = referenceManager.filterBlockBoard.GetComponentInChildren<FilterCreatorResultBlock>();
                    filterPreviewText = referenceManager.filterBlockBoard.transform.Find("Filter Preview Text").GetComponent<TextMeshPro>();
                }
            }
        }

        private void OnGraphsUnloaded()
        {
            StopAllCoroutines();
            runningSwapPercentCoroutine = null;
            cellsToEvaluate.Clear();
            evaluating = false;
            ClearBoard();
            DeactivateFilterCreatorBoard();
            currentFilter = null;
        }

        /// <summary>
        /// Loads a filter from a path.
        /// </summary>
        /// <param name="path">A path to the file containing the filter.</param>
        [ConsoleCommand("filterManager", folder: "Data\\Mouse_HSPC", aliases: new string[] { "loadfilter", "lf" })]
        public void LoadFilter(string path)
        {
            currentFilterPath = path;
            //path = Directory.GetCurrentDirectory() + "\\Data\\" + CellexalUser.DataSourceFolder + "\\" + path;
            Filter newFilter = new Filter();
            newFilter.Expression = BooleanExpression.ParseFile(path);
            //referenceManager.selectionManager.CurrentFilter = currentFilter;
            CellexalLog.Log("Loaded filter " + path);
            referenceManager.filterMenu.AddFilterButton(newFilter, currentFilterPath);
            //StartCoroutine(SwapPercentExpressions());
        }

        /// <summary>
        /// Saves the current filter as a text file.
        /// </summary>
        public void SaveFilter()
        {
            print("Saving filter: " + currentFilter.Expression.ToString());
            //var nameKeyboard = referenceManager.filterNameKeyboard;
            //nameKeyboard.gameObject.SetActive(true);
            string filterString = currentFilter.Expression.ToString();
            List<string> itemsInFilter = new List<string>();
            currentFilter.Expression.GetGenes(ref itemsInFilter);
            currentFilter.Expression.GetFacs(ref itemsInFilter);
            currentFilter.Expression.GetAttributes(ref itemsInFilter);
            string fileName = string.Join("_", itemsInFilter);
            string filterPath = Path.Combine(CellexalUser.UserSpecificFolder, fileName + ".fil");

            while (File.Exists(filterPath))
            {
                fileName += "_2";
                filterPath = Path.Combine(CellexalUser.UserSpecificFolder, fileName + ".fil");
            }

            currentFilterPath = filterPath;
            FileStream fileStream = new FileStream(filterPath, FileMode.Create, FileAccess.Write, FileShare.None);

            using (StreamWriter streamWriter = new StreamWriter(fileStream))
            {
                streamWriter.Write(filterString);
            }
            resultBlock.SetLoadingTextState(FilterCreatorResultBlock.LoadingTextState.FILTER_SAVED);
            referenceManager.filterMenu.AddFilterButton(currentFilter, currentFilterPath);
        }

        /// <summary>
        /// Coroutine that swaps percent expressions for absolute ones. Percents are always calculated based on the highest and lowest gene/facs expressions in a dataset.
        /// </summary>
        private IEnumerator SwapPercentExpressions()
        {
            if (resultBlock.isActiveAndEnabled)
            {
                resultBlock.SetLoadingTextState(FilterCreatorResultBlock.LoadingTextState.LOADING);
            }

            yield return StartCoroutine(ValidateFilterCoroutine(currentFilter));
            if (currentFilter == null)
            {
                runningSwapPercentCoroutine = null;
                yield break;
            }

            string[] genes = currentFilter.GetGenes(true).ToArray();
            string[] facs = currentFilter.GetFacs(true).ToArray();

            List<Tuple<string, float, float>> ranges = new List<Tuple<string, float, float>>();
            // swap genes
            if (genes.Length > 0)
            {
                SQLiter.SQLite database = referenceManager.database;
                while (database.QueryRunning)
                {
                    yield return null;
                }
                database.QueryGeneRanges(genes);
                while (database.QueryRunning)
                {
                    yield return null;
                }
                var results = database._result;
                for (int i = 0; i < results.Count; ++i)
                {
                    Tuple<string, float, float> range = (Tuple<string, float, float>)results[i];
                    ranges.Add(new Tuple<string, float, float>(range.Item1.ToLower(), range.Item2, range.Item3));
                }
            }

            // swap facs
            if (facs.Length > 0)
            {
                var facsRanges = referenceManager.cellManager.FacsRanges;
                for (int i = 0; i < facs.Length; ++i)
                {
                    Tuple<float, float> facsRange = facsRanges[facs[i]];
                    ranges.Add(new Tuple<string, float, float>(facs[i].ToLower(), facsRange.Item1, facsRange.Item2));
                }
            }
            currentFilter.Expression.SwapPercentExpressions(ranges.ToArray());
            currentFilter.Expression.SetFilterManager(this);
            string filterAsText = currentFilter.Expression.ToString();
            filterPreviewText.text = filterAsText;
            referenceManager.multiuserMessageSender.SendMessageSetFilter(filterAsText);
            if (resultBlock.isActiveAndEnabled)
            {
                resultBlock.SetLoadingTextState(FilterCreatorResultBlock.LoadingTextState.FINISHED);
            }
            runningSwapPercentCoroutine = null;
            //GameObject obj = GameObject.Find("CullingCube(Clone)");
            //if (obj)
            //{
            //    obj.GetComponent<CullingCube>().ActivateFilter();
            //}
        }
        /// <summary>
        /// Changes the filter temporarily so it does not add points to selection but instead makes them clippable in the shader.
        /// </summary>
        public void ActivateCullingFilter()
        {
            cullingFilter = true;

        }
        /// <summary>
        /// Adds a cell to evaluate later with the current filter.
        /// </summary>
        /// <param name="graphPoint">The graphpoint representing the cell.</param>
        /// <param name="group">The group to give the cell if it passes the filter.</param>
        public void AddCellToEval(Graph.GraphPoint graphPoint, int group)
        {
            queuedCells.Add(new Tuple<Graph.GraphPoint, int>(graphPoint, group));
        }

        /// <summary>
        /// Adds multiple cells to evaluate later with the current filter.
        /// </summary>
        /// <param name="points">The points to evaluate later.</param>
        /// <param name="group">The group to give the points if they pass the filter.</param>
        public void AddCellsToEval(IEnumerable<Graph.GraphPoint> points, int group)
        {
            foreach (Graph.GraphPoint point in points)
            {
                queuedCells.Add(new Tuple<Graph.GraphPoint, int>(point, group));
            }
        }

        private void Update()
        {
            if (!evaluating && currentFilter != null && queuedCells.Count > 0)
            {
                StartCoroutine(EvalQueuedCellsCoroutine());
            }

        }

        private void OnTriggerClick()
        {
            if (!portClickedThisFrame && previouslyClickedPort != null)
            {
                previewWire.SetActive(false);
                previouslyClickedPort = null;
            }
            portClickedThisFrame = false;
        }

        /// <summary>
        /// Coroutine to evaluate selected cells based on the current filter
        /// </summary>
        private IEnumerator EvalQueuedCellsCoroutine()
        {
            evaluating = true;
            cellsToEvaluate.AddRange(queuedCells);
            queuedCells.Clear();
            string[] cells = cellsToEvaluate.Select((p) => p.Item1.Label).ToArray();
            if (currentFilterGenes.Length > 0)
            {
                SQLiter.SQLite database = referenceManager.database;
                database.QueryGenesInCells(currentFilterGenes, cells);
                while (database.QueryRunning)
                {
                    yield return null;
                }

                // results are ready to be read
                string geneName = "";
                for (int i = 0; i < database._result.Count; ++i)
                {
                    Tuple<string, float> tuple = (Tuple<string, float>)database._result[i];
                    string lowerCaseGeneName = tuple.Item1.ToLower();
                    if (currentFilterGenes.Contains(lowerCaseGeneName))
                    {

                        // new gene, first two tuples are highest and lowest expression
                        geneName = lowerCaseGeneName;
                        // skip the second tuple
                        i++;
                    }
                    else
                    {
                        GeneExprs[new Tuple<string, string>(geneName, tuple.Item1)] = tuple.Item2;
                    }
                }
            }

            CellManager cellManager = referenceManager.cellManager;
            SelectionManager selectionManager = referenceManager.selectionManager;

            List<Graph.GraphPoint> pointsPassed = new List<Graph.GraphPoint>();
            int prevGroup = -1;
            foreach (var t in cellsToEvaluate)
            {
                Graph.GraphPoint gp = t.Item1;
                int group = t.Item2;
                if (currentFilter.Pass(cellManager.GetCell(gp.Label)))
                {
                    if (cullingFilter)
                    {
                        foreach (Graph g in referenceManager.graphManager.Graphs)
                        {
                            Graph.GraphPoint otherGp = g.FindGraphPoint(gp.Label);
                            if (otherGp != null)
                                g.MakePointUnCullable(otherGp, true);
                        }
                    }
                    else
                    {
                        Color newColor = selectionManager.GetColor(group);
                        referenceManager.multiuserMessageSender.SendMessageSelectedAdd(gp.parent.GraphName, gp.Label, group, newColor);
                        if (prevGroup != group && pointsPassed.Count > 0)
                        {
                            // group change! add the previous group to the selection
                            ReferenceManager.instance.selectionManager.AddGraphPointsToSelection(pointsPassed, prevGroup);
                            pointsPassed.Clear();
                        }
                        prevGroup = group;
                        pointsPassed.Add(gp);
                    }
                }

            }
            // add the remaining cells that passed to the selection
            if (pointsPassed.Count > 0)
            {
                ReferenceManager.instance.selectionManager.AddGraphPointsToSelection(pointsPassed, prevGroup);
            }

            cellsToEvaluate.Clear();
            GeneExprs.Clear();
            // wait half a second before evaluating again
            yield return new WaitForSeconds(0.5f);
            evaluating = false;
            cullingFilter = false;
        }

        /// <summary>
        /// Tells the filter manager that a port was clicked. Handles the preview wire.
        /// </summary>
        /// <param name="clickedPort">The clicked port.</param>
        public void PortClicked(FilterCreatorBlockPort clickedPort)
        {
            portClickedThisFrame = true;
            if (previouslyClickedPort == null)
            {
                previewWire.SetActive(true);
                var follow = previewWire.GetComponent<LineRendererFollowTransforms>();
                follow.transform1 = referenceManager.rightController.transform;
                follow.transform2 = clickedPort.transform;
                previouslyClickedPort = clickedPort;
                clickedPort.Disconnect();
            }
            else
            {
                previewWire.SetActive(false);
                clickedPort.ConnectTo(previouslyClickedPort);
                previouslyClickedPort = null;
            }
        }

        /// <summary>
        /// Parses a string and turns it into a filter.
        /// </summary>
        /// <param name="filter">The string to parse.</param>
        public void ParseFilter(string filter)
        {
            Filter newFilter = new Filter();
            newFilter.Expression = BooleanExpression.ParseFilter(filter);
            if (newFilter.Expression == null)
            {
                return;
            }
            newFilter.Expression.SetFilterManager(this);
            currentFilter = newFilter;
            currentFilterGenes = currentFilter.GetGenes(false).ToArray();
        }

        /// <summary>
        /// Updates the current filter based on the blocks in the filter creator.
        /// </summary>
        public void UpdateFilterFromFilterCreator()
        {
            if (runningSwapPercentCoroutine != null)
            {
                StopCoroutine(runningSwapPercentCoroutine);
            }

            Filter newFilter = resultBlock.ToFilter();
            if (newFilter == null)
            {
                resultBlock.SetLoadingTextState(FilterCreatorResultBlock.LoadingTextState.INVALID_FILTER);
                filterPreviewText.text = BooleanExpression.ErrorMessage;
                return;
            }
            currentFilter = newFilter;
            currentFilterGenes = currentFilter.GetGenes(false).ToArray();
            runningSwapPercentCoroutine = StartCoroutine(SwapPercentExpressions());
        }

        /// <summary>
        /// Validates if the current filter is contains only genes, attrubutes and facs that exist in the loaded dataset. This function sets <see cref="currentFilter"/> to <see langword="null"/> if the filter is invalid or to <paramref name="filter"/> if it is valid.        /// </summary>
        /// <param name="filter">The filter to validate.</param>
        private IEnumerator ValidateFilterCoroutine(Filter filter)
        {
            // check that genes exists
            string[] genes = filter.GetGenes().ToArray();
            // TODO: ask johan
            //Array.ForEach(genes, (s) => s.ToLower());
            //SQLiter.SQLite database = referenceManager.database;
            //while (database.QueryRunning)
            //{
            //    yield return null;
            //}
            //database.QueryGenesIds(genes);
            //while (database.QueryRunning)
            //{
            //    yield return null;
            //}
            //foreach (Tuple<string, string> geneId in database._result)
            //{
            //    if (!genes.Contains(geneId.Item1.ToLower()))
            //    {
            //        resultBlock.SetLoadingTextState(FilterCreatorResultBlock.LoadingTextState.INVALID_FILTER);
            //        filterPreviewText.text = "FILTER ERROR: Gene " + geneId.Item1 + " not found";
            //        currentFilter = null;
            //        yield break;
            //    }
            //}

            // check that facs exists
            CellManager cellManager = referenceManager.cellManager;
            List<string> facsList = filter.GetFacs();

            for (int i = 0; i < facsList.Count; i++) // string facs in facsList)
            {
                string facs = facsList[i];
                if (!cellManager.FacsRanges.ContainsKey(facs))
                {
                    resultBlock.SetLoadingTextState(FilterCreatorResultBlock.LoadingTextState.INVALID_FILTER);
                    filterPreviewText.text = "FILTER ERROR: Facs " + facs + " not found";
                    currentFilter = null;
                    yield break;
                }
            }

            // check attributes
            List<string> attributes = filter.GetAttributes();
            foreach (string attribute in attributes)
            {
                //print($"filter {attribute}");
                if (!cellManager.AttributesNames.Contains(attribute, StringComparer.CurrentCultureIgnoreCase))
                {
                    //print("invalid");
                    resultBlock.SetLoadingTextState(FilterCreatorResultBlock.LoadingTextState.INVALID_FILTER);
                    filterPreviewText.text = "FILTER ERROR: Attribute " + attribute + " not found";
                    currentFilter = null;
                    yield break;
                }
            }

            // check numerical attributes
            List<string> numericalAttributes = filter.GetNumericalAttributes();
            foreach (string attribute in numericalAttributes)
            {
                if (!cellManager.NumericalAttributes.Contains(attribute, StringComparer.OrdinalIgnoreCase))
                {
                    print($"invalid NUm");
                    resultBlock.SetLoadingTextState(FilterCreatorResultBlock.LoadingTextState.INVALID_FILTER);
                    filterPreviewText.text = "FILTER ERROR: Attribute " + attribute + " not found";
                    currentFilter = null;
                    yield break;
                }
            }
            currentFilter = filter;
        }

        /// <summary>
        /// Updates the filter based on a filter stored with a button.
        /// </summary>
        /// <param name="filter">The filter stored in the button.</param>
        public void UpdateFilterFromFilterButton(Filter filter)
        {
            if (runningSwapPercentCoroutine != null)
            {
                StopCoroutine(runningSwapPercentCoroutine);
            }
            currentFilter = filter;
            currentFilterGenes = currentFilter.GetGenes(false).ToArray();
            runningSwapPercentCoroutine = StartCoroutine(SwapPercentExpressions());
        }

        /// <summary>
        /// Clears the filter creator board of all blocks.
        /// </summary>
        private void ClearBoard()
        {
            Transform filterBoardTransform = referenceManager.filterBlockBoard.transform;
            foreach (Transform child in filterBoardTransform)
            {
                // only get direct children, no grand children
                if (child.parent == filterBoardTransform)
                {
                    FilterCreatorBlock blockScript = child.GetComponent<FilterCreatorBlock>();
                    if (blockScript != null && !(blockScript is FilterCreatorResultBlock))
                    {
                        blockScript.DisconnectAllPorts();
                        Destroy(child.gameObject);
                    }
                }
            }
            UpdateFilterFromFilterCreator();
        }

        private void DeactivateFilterCreatorBoard()
        {
            referenceManager.filterBlockBoard.SetActive(false);
        }

        public void ResetFilter(/*bool informMultiUser = true*/)
        {
            currentFilter = null;
            StopAllCoroutines();
            runningSwapPercentCoroutine = null;
            cellsToEvaluate.Clear();
            evaluating = false;
            resultBlock.DisconnectAllPorts();
            CellexalEvents.FilterDeactivated.Invoke();
        }

        /// <summary>
        /// Helper class to compare tuples of strings.
        /// </summary>
        private class TupleComparer : IEqualityComparer<Tuple<string, string>>
        {
            public bool Equals(Tuple<string, string> x, Tuple<string, string> y)
            {
                return x.Item1.Equals(y.Item1) && x.Item2.Equals(y.Item2);
            }

            public int GetHashCode(Tuple<string, string> obj)
            {
                return obj.Item1.GetHashCode() ^ obj.Item2.GetHashCode();
            }
        }
    }
}
