using UnityEngine;
using System.Collections;
using CellexalVR.General;
using CellexalVR.Extensions;

namespace CellexalVR.AnalysisObjects
{
    /// <summary>
    /// Represents a manager that handles a set of legends.
    /// </summary>
    public class LegendManager : MonoBehaviour
    {
        public GameObject backgroundPlane;
        public GroupingLegend attributeLegend;
        public GroupingLegend selectionLegend;
        public GeneExpressionHistogram geneExpressionHistogram;
        public GameObject attachPoint;

        public Legend activeLegend;
        public enum Legend { None, AttributeLegend, GeneExpressionLegend, SelectionLegend }

        private Vector3 minPos = new Vector3(-0.58539f, -0.28538f, 0f);
        private Vector3 maxPos = new Vector3(-0.0146f, 0.2852f, 0f);


        private void Start()
        {
            CellexalEvents.GraphsReset.AddListener(DeactivateLegends);
            geneExpressionHistogram.referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }

        /// <summary>
        /// Deactivates all legends.
        /// </summary>
        public void DeactivateLegends()
        {
            activeLegend = Legend.None;
            backgroundPlane.SetActive(false);
            gameObject.GetComponent<Collider>().enabled = false;
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Activates a legend.
        /// </summary>
        /// <param name="legendToActivate">The legend to activate</param>
        public void ActivateLegend(Legend legendToActivate)
        {
            DeactivateLegends();
            backgroundPlane.SetActive(true);
            gameObject.GetComponent<Collider>().enabled = true;

            switch (legendToActivate)
            {
                case Legend.AttributeLegend:
                    attributeLegend.gameObject.SetActive(true);
                    break;
                case Legend.GeneExpressionLegend:
                    geneExpressionHistogram.gameObject.SetActive(true);
                    break;
                case Legend.SelectionLegend:
                    selectionLegend.gameObject.SetActive(true);
                    break;
            }

            activeLegend = legendToActivate;
        }

        /// <summary>
        /// Projects and converts a world space coordinate to a position on the legend plane.
        /// </summary>
        /// <returns>A <see cref="Vector3"/> in the range [0,1]</returns>
        public Vector3 WorldToRelativePos(Vector3 worldPos)
        {
            Vector3 localPos = transform.InverseTransformPoint(worldPos);
            localPos.z = 0f;
            return (localPos - minPos).InverseScale(maxPos - minPos);
        }

        public Vector3 WorldToRelativeHistogramPos(Vector3 worldPos)
        {
            Vector3 localPos = transform.InverseTransformPoint(worldPos);
            //print("world and local " + worldPos + " " + localPos + " " + (localPos - geneExpressionHistogram.HistogramMinPos).InverseScale(geneExpressionHistogram.HistogramMaxPos - geneExpressionHistogram.HistogramMinPos));
            localPos.z = 0f;
            return (localPos - geneExpressionHistogram.HistogramMinPos).InverseScale(geneExpressionHistogram.HistogramMaxPos - geneExpressionHistogram.HistogramMinPos);
        }
    }
}
