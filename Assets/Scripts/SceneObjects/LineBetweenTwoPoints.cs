using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Interaction;
using UnityEngine;
namespace CellexalVR.SceneObjects
{

    /// <summary>
    /// Represents a line between two graphpoints and moves the line accordingly when the graphpoints move.
    /// </summary>
    class LineBetweenTwoPoints : MonoBehaviour
    {

        public Transform t1, t2;
        public Graph.GraphPoint graphPoint1;
        public Graph.GraphPoint graphPoint2;
        public SelectionManager selectionManager;
        public Selectable cube;

        private LineRenderer lineRenderer;
        private Vector3 middle;
        private Vector3 pos1, pos2;

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            pos1 = t1.TransformPoint(graphPoint1.Position);
            pos2 = t2.TransformPoint(graphPoint2.Position);
            lineRenderer.SetPositions(new Vector3[] { pos1, pos2 });
            middle = (pos1 + pos2) / 2f;
            cube.transform.position = middle;
            cube.selectionManager = selectionManager;
            cube.graphPoint = graphPoint1;
        }

        private void Update()
        {
            if (t1.hasChanged || t2.hasChanged)
            {
                pos1 = t1.TransformPoint(graphPoint1.Position);
                pos2 = t2.TransformPoint(graphPoint2.Position);
                lineRenderer.SetPosition(0, pos1);
                lineRenderer.SetPosition(1, pos2);
                middle = (pos1 + pos2) / 2f;
                cube.transform.position = middle;
            }
        }
    }
}
