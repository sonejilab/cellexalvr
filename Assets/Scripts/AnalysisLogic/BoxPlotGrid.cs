using CellexalVR.AnalysisObjects;
using CellexalVR.DesktopUI;
using CellexalVR.General;
using CellexalVR.Interaction;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.AnalysisLogic
{
    public class BoxPlotGrid : MonoBehaviour
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
        private int layerMask;
        private Selection selection;
        private BoxPlot hoveredBoxplot;

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
            layerMask = 1 << LayerMask.NameToLayer("EnvironmentButtonLayer");
            layerMask += 1 << LayerMask.NameToLayer("MenuLayer");
            CellexalEvents.RightTriggerPressed.AddListener(OnTriggerDown);
        }

        private void Update()
        {
            foreach (var boxPlot in boxPlots)
            {
                boxPlot.SetInfoTextActive(false);
            }
            Raycast();
        }

        private void Raycast()
        {
            var raycastingSource = referenceManager.rightLaser.transform;
            Physics.Raycast(raycastingSource.position, raycastingSource.TransformDirection(Vector3.forward), out var hit, 100f, layerMask);
            if (hit.collider && (/*hit.collider.gameObject == gameObject ||*/ hit.collider.gameObject.GetComponent<BoxPlot>()))
            {
                BoxPlot hitBoxPlot = hit.collider.gameObject.GetComponent<BoxPlot>();
                hitBoxPlot.SetInfoTextActive(true);
                hoveredBoxplot = hitBoxPlot;
            }
            else
            {
                hoveredBoxplot = null;
            }
        }

        private void OnTriggerDown()
        {
            if (hoveredBoxplot)
            {
                referenceManager.cellManager.ColorByIndex(hoveredBoxplot.facsNameString);
            }
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
        /// <param name="selection">A list of graphpoints to use for the boxplots.</param>
        public void GenerateBoxPlots(Selection selection)
        {
            this.selection = selection;
            string[] facsNames = referenceManager.cellManager.Facs;

            float globalMinValue = float.MaxValue;
            float globalMaxValue = float.MinValue;

            OffsetGrab grabScript = gameObject.GetComponent<OffsetGrab>();

            for (int i = 0; i < facsNames.Length; ++i)
            {
                List<float> values = new List<float>();
                string facs = facsNames[i];
                string lowerFacs = facs.ToLower();
                foreach (Graph.GraphPoint graphPoint in selection)
                {
                    values.Add(referenceManager.cellManager.GetCell(graphPoint.Label).Facs[lowerFacs]);
                }

                values.Sort();

                int middleIndex = values.Count / 2;
                BoxPlot newBoxPlot = Instantiate(boxPlotPrefab);
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
        /// Removes all boxp lots, does not remove the background or parent gameobject. <see cref="GenerateBoxPlots(List{Graph.GraphPoint})"/> can be called to populate this <see cref="BoxPlotGrid"/> again.
        /// </summary>
        public void ClearBoxPlots()
        {
            for (int i = 0; i < boxPlots.Count; ++i)
            {
                Destroy(boxPlots[i].gameObject);
            }
            boxPlots.Clear();
            currentBoxPlotsOrder.Clear();
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
        /// Recolors all graphs to the selection that was used to generate the box plots.
        /// </summary>
        public void RecolorSelection()
        {
            ReferenceManager.instance.graphManager.ColorAllGraphsBySelection(selection);
        }
    }
}
