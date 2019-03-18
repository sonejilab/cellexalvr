using UnityEngine;
using CellexalVR.AnalysisObjects;
namespace CellexalVR.Interaction
{
    /// <summary>
    /// Selectable gameobject. Selectable means adding to selection of cells.
    /// </summary>
    public class Selectable : MonoBehaviour
    {

        public SelectionToolHandler selectionToolHandler;
        public Graph.GraphPoint graphPoint;

        private void OnTriggerEnter(Collider other)
        {
            selectionToolHandler.AddGraphpointToSelection(graphPoint);
        }
    }
}