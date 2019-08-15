using CellexalVR.AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.DesktopUI;
using CellexalVR.Filters;
using CellexalVR.General;
using CellexalVR.SceneObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CellexalVR.Filters
{
    public class FilterManager : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject wirePrefab;
        public FilterCreatorResultBlock resultBlock;

        private SteamVR_TrackedObject rightController;
        private List<Tuple<Graph.GraphPoint, int>> queuedCells = new List<Tuple<Graph.GraphPoint, int>>(256);
        private List<Tuple<Graph.GraphPoint, int>> cellsToEvaluate = new List<Tuple<Graph.GraphPoint, int>>(256);
        private bool loadingFilter = false;
        private Coroutine runningSwapPercentCoroutine;
        private bool evaluating = false;
        private Filter currentFilter;
        private string[] currentFilterGenes;
        private FilterCreatorBlockPort previouslyClickedPort;
        private GameObject previewWire;
        private bool portClickedThisFrame = false;

        // Key.Item1 is a gene name, Key.Item2 is a cell name, Value is the expression
        public Dictionary<Tuple<string, string>, float> GeneExprs { get; set; }

        private void Start()
        {
            GeneExprs = new Dictionary<Tuple<string, string>, float>(new TupleComparer());
            previewWire = Instantiate(wirePrefab, this.transform);
            previewWire.SetActive(false);
            rightController = referenceManager.rightController;
        }

        private void OnValidate()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }

        /// <summary>
        /// Loads a filter from a path.
        /// </summary>
        /// <param name="path">A path to the file containing the filter.</param>
        [ConsoleCommand("filterManager", folder: "Data\\Mouse_HSPC", aliases: new string[] { "loadfilter", "lf" })]
        public void LoadFilter(string path)
        {
            loadingFilter = true;
            path = Directory.GetCurrentDirectory() + "\\Data\\" + CellexalUser.DataSourceFolder + "\\" + path;
            currentFilter = new Filter();
            currentFilter.Expression = BooleanExpression.ParseFile(path);
            //referenceManager.selectionManager.CurrentFilter = currentFilter;
            CellexalLog.Log("Loaded filter " + path);
            StartCoroutine(SwapPercentExpressions());
        }

        private IEnumerator SwapPercentExpressions()
        {
            resultBlock.SetLoadingTextState(FilterCreatorResultBlock.LoadingTextState.LOADING);
            // swap out percent expressions
            string[] genes = currentFilter.GetGenes(true).ToArray();
            string[] facs = currentFilter.GetFacs(true).ToArray();
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
                Tuple<string, float, float>[] ranges = new Tuple<string, float, float>[results.Count];
                for (int i = 0; i < results.Count; ++i)
                {
                    Tuple<string, float, float> range = (Tuple<string, float, float>)results[i];
                    ranges[i] = new Tuple<string, float, float>(range.Item1.ToLower(), range.Item2, range.Item3);
                }
                var facsRanges = referenceManager.cellManager.FacsRanges;
                for (int i = genes.Length, j = 0; i < ranges.Length; ++i, ++j)
                {
                    if (!facsRanges.ContainsKey(facs[j]))
                    {
                        CellexalLog.Log("FILTER ERROR: Facs " + facs[j] + " not found.");
                        yield break;
                    }

                    Tuple<float, float> facsRange = facsRanges[facs[j]];
                    ranges[i] = new Tuple<string, float, float>(facs[j].ToLower(), facsRange.Item1, facsRange.Item2);
                }

                currentFilter.Expression.SwapPercentExpressions(ranges);
                currentFilter.Expression.SetFilterManager(this);
            }
            referenceManager.selectionManager.CurrentFilter = currentFilter;
            print(currentFilter.Expression.ToString());
            loadingFilter = false;
            resultBlock.SetLoadingTextState(FilterCreatorResultBlock.LoadingTextState.FINISHED);
            runningSwapPercentCoroutine = null;
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

        private void Update()
        {
            if (!evaluating && currentFilter != null && queuedCells.Count > 0)
            {
                StartCoroutine(EvalQueuedCellsCoroutine());
            }

            var device = SteamVR_Controller.Input((int)rightController.index);
            if (!portClickedThisFrame && previouslyClickedPort != null && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                previewWire.SetActive(false);
                previouslyClickedPort = null;
            }
            portClickedThisFrame = false;
        }

        private IEnumerator EvalQueuedCellsCoroutine()
        {
            evaluating = true;
            //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();
            cellsToEvaluate.AddRange(queuedCells);
            queuedCells.Clear();

            //CellexalLog.Log("Evaluating " + cellsToEvaluate.Count + " queued cells for filter");
            string[] cells = cellsToEvaluate.Select((p) => p.Item1.Label).ToArray();
            //print("found " + currentFilterGenes.Length + " in filter");
            if (currentFilterGenes.Length > 0)
            {
                SQLiter.SQLite database = referenceManager.database;
                database.QueryGenesInCells(currentFilterGenes, cells);
                while (database.QueryRunning)
                {
                    yield return null;
                }
                //print("got " + database._result.Count + " gene ranges from database");

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

            foreach (var t in cellsToEvaluate)
            {
                //print("evaluating cell " + t.Item1.Label);
                //string cellname = t.Item1.Label;
                //foreach (string gene in GeneExprs.Keys.Select((Tuple<string, string> tuple) => tuple.Item1))
                //{
                //    print("evaluating " + t.Item1.Label + " with " + gene + "expression " + GeneExprs[new Tuple<string, string>(cellname, gene)]);
                //}
                if (currentFilter.Pass(cellManager.GetCell(t.Item1.Label)))
                {
                    selectionManager.AddGraphpointToSelection(t.Item1, t.Item2, false, selectionManager.GetColor(t.Item2));
                }
            }

            cellsToEvaluate.Clear();
            GeneExprs.Clear();
            //stopwatch.Stop();
            //CellexalLog.Log("Finished evaluating cells for filter in " + stopwatch.Elapsed);
            // wait half a second before evaluating again
            yield return new WaitForSeconds(0.5f);
            evaluating = false;
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

        public void UpdateFilterFromFilterCreator()
        {
            if (runningSwapPercentCoroutine != null)
            {
                StopCoroutine(runningSwapPercentCoroutine);
            }

            loadingFilter = true;
            Filter newFilter = resultBlock.ToFilter();
            if (newFilter == null)
            {
                resultBlock.SetLoadingTextState(FilterCreatorResultBlock.LoadingTextState.INVALID_FILTER);
                return;
            }
            currentFilter = newFilter;
            currentFilterGenes = currentFilter.GetGenes(false).ToArray();
            runningSwapPercentCoroutine = StartCoroutine(SwapPercentExpressions());
        }

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