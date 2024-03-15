using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Interaction;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.AnalysisLogic
{
    public class BoxPlotGrid : EnvironmentTab
    {
        public ReferenceManager referenceManager;

        public BoxPlot boxPlotPrefab;

        /// <summary>
        /// Represent the different orders that the box plots can be sorted by. Default is the order the FACS names are written in the input files
        /// </summary>
        public enum SortOrder { DEFAULT, MEDIAN, BOX_HEIGHT }

        private List<BoxPlot> boxPlots = new List<BoxPlot>();
        private List<BoxPlot> currentBoxPlotsOrder = new List<BoxPlot>();
        private int plotsPerRow = 10;
        private float rowInc; // initialised in Awake()
        private float colInc = 0.16f;
        private Vector3 boxPlotStartPos;
        private Selection selection;
        private int group = -1;

        private float globalMinValue = float.MaxValue;
        private float globalMaxValue = float.MinValue;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Awake()
        {
            rowInc = 0.8f / plotsPerRow;
            boxPlotStartPos = boxPlotPrefab.transform.localPosition;
        }


        /// <summary>
        /// Helper method to lay out the box plots in a grid
        /// </summary>
        /// <param name="index">The index of the box plot, 0 being the top left one.</param>
        /// <returns>A <see cref="Vector3"/> in local space at the center point of a box plot that can be used to place the <see cref="boxPlotPrefab"/>.</returns>
        private Vector3 BoxPlotLocalPosition(int index)
        {
            return new Vector3(boxPlotStartPos.x + (index % plotsPerRow) * rowInc, boxPlotStartPos.y - (index / plotsPerRow) * colInc, boxPlotStartPos.z);
        }

        /// <summary>
        /// Generates new box plots from a given selection.
        /// </summary>
        /// <param name="selection">A list of graphpoints to use for the boxplots. Or <c>null</c> for all cells in the current dataset.</param>
        public void GenerateBoxPlots(Selection selection)
        {
            this.selection = selection;
            string[] facsNames = referenceManager.cellManager.Facs;

            OffsetGrab grabScript = gameObject.GetComponent<OffsetGrab>();

            for (int i = 0; i < facsNames.Length; ++i)
            {
                List<float> values = new List<float>();
                string facs = facsNames[i];
                string lowerFacs = facs.ToLower();
                if (selection is not null)
                {
                    foreach (Graph.GraphPoint graphPoint in selection)
                    {
                        values.Add(referenceManager.cellManager.GetCell(graphPoint.Label).Facs[lowerFacs]);
                    }
                }
                else
                {
                    foreach (Cell cell in referenceManager.cellManager.GetCells())
                    {
                        values.Add(cell.Facs[lowerFacs]);
                    }
                }

                values.Sort();

                int middleIndex = values.Count / 2;
                BoxPlot newBoxPlot = Instantiate(boxPlotPrefab, contentParent.transform);
                boxPlots.Add(newBoxPlot);
                currentBoxPlotsOrder.Add(newBoxPlot);
                grabScript.colliders.Add(newBoxPlot.GetComponent<Collider>());

                float median = values.Count % 2 == 0 ? (values[middleIndex - 1] + values[middleIndex]) / 2f : values[middleIndex];
                float percentile5th = values[(int)(values.Count * 0.05f)];
                float percentile95th = values[(int)(values.Count * 0.95f)];
                float minValue = values[0];
                float maxValue = values[^1];

                newBoxPlot.InitBoxPlot(facs, median, percentile5th, percentile95th, minValue, maxValue);

                if (values[0] < globalMinValue)
                {
                    globalMinValue = values[0];
                }
                if (values[^1] > globalMaxValue)
                {
                    globalMaxValue = values[^1];
                }
            }

            for (int i = 0; i < facsNames.Length; ++i)
            {
                BoxPlot boxPlot = boxPlots[i];
                boxPlot.gameObject.SetActive(true);
                boxPlot.transform.parent = transform;
                boxPlot.transform.localPosition = BoxPlotLocalPosition(i);
                boxPlot.transform.localRotation = Quaternion.identity;
                boxPlot.ResizeComponents(globalMinValue, globalMaxValue);
            }

            grabScript.interactionManager.UnregisterInteractable(grabScript.GetComponent<IXRInteractable>());
            grabScript.interactionManager.RegisterInteractable(grabScript.GetComponent<IXRInteractable>());
        }

        /// <summary>
        /// Creates a new box plot from the given values.
        /// </summary>
        /// <param name="values">A list of values to create the box plot from.</param>
        /// <param name="facs">The label to put over the box plot.</param>
        public void GenerateBoxPlot(List<float> values, string facs)
        {
            values.Sort();

            int middleIndex = values.Count / 2;
            BoxPlot newBoxPlot = Instantiate(boxPlotPrefab, contentParent.transform);
            boxPlots.Add(newBoxPlot);
            currentBoxPlotsOrder.Add(newBoxPlot);

            float median = values.Count % 2 == 0 ? (values[middleIndex - 1] + values[middleIndex]) / 2f : values[middleIndex];
            float percentile5th = values[(int)(values.Count * 0.05f)];
            float percentile95th = values[(int)(values.Count * 0.95f)];
            float minValue = values[0];
            float maxValue = values[^1];

            newBoxPlot.InitBoxPlot(facs, median, percentile5th, percentile95th, minValue, maxValue);

            if (values[0] < globalMinValue)
            {
                globalMinValue = values[0];
            }
            if (values[^1] > globalMaxValue)
            {
                globalMaxValue = values[^1];
            }

            newBoxPlot.gameObject.SetActive(true);
            newBoxPlot.transform.localPosition = BoxPlotLocalPosition(boxPlots.Count - 1);
            newBoxPlot.transform.localRotation = Quaternion.identity;
            newBoxPlot.ResizeComponents(globalMinValue, globalMaxValue);

        }

        /// <summary>
        /// Sets the selection that this collection of boxplots represents.
        /// </summary>
        /// <param name="selection">The selection that is used to create the box plots.</param>
        /// <param name="group">Optional. One of the selection's groups, from <see cref="Selection.groups"/>, if the boxplots corresponds to only one group of the selection.</param>
        public void SetSelection(Selection selection, int group = -1)
        {
            this.selection = selection;
            this.group = group;
        }

        /// <summary>
        /// Resizes all boxplots depending on the global minimum and maximum values in them. Should be called once all boxplots have been generated with <see cref="GenerateBoxPlot"/>.
        /// </summary>
        public void ResizeAllBoxPlots()
        {
            foreach (BoxPlot boxPlot in boxPlots)
            {
                boxPlot.ResizeComponents(globalMinValue, globalMaxValue);
            }
        }

        /// <summary>
        /// Removes all box lots, does not remove the background or parent gameobject. <see cref="GenerateBoxPlots(List{Graph.GraphPoint})"/> can be called to populate this <see cref="BoxPlotGrid"/> again.
        /// </summary>
        public void ClearBoxPlots()
        {
            OffsetGrab grabScript = gameObject.GetComponent<OffsetGrab>();
            for (int i = 0; i < boxPlots.Count; ++i)
            {
                grabScript.colliders.Remove(boxPlots[i].GetComponent<Collider>());
                Destroy(boxPlots[i].gameObject);
            }
            boxPlots.Clear();
            currentBoxPlotsOrder.Clear();

            globalMinValue = float.MaxValue;
            globalMaxValue = float.MinValue;
        }

        /// <summary>
        /// Destroys this gameobject and all box plots it contains.
        /// </summary>
        public void Remove()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Sorts all box plots by their median value, in ascending order.
        /// </summary>
        public void SortBoxPlotsByMedian()
        {
            currentBoxPlotsOrder.Sort((b1, b2) => b1.median.CompareTo(b2.median));
            for (int i = 0; i < currentBoxPlotsOrder.Count; ++i)
            {
                currentBoxPlotsOrder[i].transform.localPosition = BoxPlotLocalPosition(i);
            }
        }
        /// <summary>
        /// Sorts all box plots by their boxes' heights, in descending order.
        /// </summary>
        public void SortBoxPlotsByHeight()
        {
            currentBoxPlotsOrder.Sort((b1, b2) =>
            {
                float b1Height = b1.percentile95th - b1.percentile5th;
                float b2Height = b2.percentile95th - b2.percentile5th;
                return b2Height.CompareTo(b1Height);
            });
            for (int i = 0; i < currentBoxPlotsOrder.Count; ++i)
            {
                currentBoxPlotsOrder[i].transform.localPosition = BoxPlotLocalPosition(i);
            }
        }

        /// <summary>
        /// Sorts all box plots by the default order they were written in the input files.
        /// </summary>
        public void SortBoxPlotsByDefault()
        {
            for (int i = 0; i < boxPlots.Count; ++i)
            {
                boxPlots[i].transform.localPosition = BoxPlotLocalPosition(i);
            }
        }

        /// <summary>
        /// Highlights this the group in all graphs that this grid of boxplots was made from.
        /// </summary>
        public void HighlightSelection()
        {
            if (group != -1)
            {
                foreach (Graph graph in ReferenceManager.instance.graphManager.Graphs)
                {
                    graph.HighlightSelectionGroup(selection, group);
                }
            }
        }

        /// <summary>
        /// Clears the group highlighting from <see cref="HighlightSelection"/>.
        /// </summary>
        public void ClearHighlight()
        {
            if (group != -1)
            {
                foreach (Graph graph in ReferenceManager.instance.graphManager.Graphs)
                {
                    graph.ResetHighlight();
                }
            }

        }
    }
}
