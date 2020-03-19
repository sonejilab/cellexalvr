using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CellexalVR.AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;

namespace CellexalVR.Interaction
{
    public class AnnotationTextPanel : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public List<Cell> Cells { get; set; }
        
        private int myColor = 1;
        private int[] otherColor;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
            {
                referenceManager.cellManager.HighlightCells(Cells.ToArray(), true);
                // foreach (Graph.GraphPoint gp in graphPoints)
                // {
                //     gp.HighlightGraphPoint(true);
                // }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            referenceManager.cellManager.HighlightCells(Cells.ToArray(), false);
            // foreach (Graph.GraphPoint gp in graphPoints)
            // {
            //     gp.HighlightGraphPoint(false);
            // }
        }
    }
}