using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Interaction;
using System.Collections.Generic;
using UnityEngine;
namespace CellexalVR.SceneObjects
{

    /// <summary>
    /// Represents a line between two graphpoints and moves the line accordingly when the graphpoints move.
    /// Either the line is a line from one graphpoint to another (having one mid point as the graphpoint in the graph between).
    /// Or it is clustered line. In this case it goes from a centroid of a cluster to another and has two anchorpoints more so 5 points in total.
    /// </summary>
    class LineBetweenTwoPoints : MonoBehaviour
    {
        public Transform t1, t2, t3;
        public Vector3 fromGraphCentroid;
        public Vector3 midGraphCentroid;
        public Vector3 toGraphCentroid;
        public Vector3 fromClusterHull;
        public Vector3 midClusterHull;
        public Vector3 toClusterHull;
        public bool centroids;
        public GameObject spherePrefab;
        public Material sphereMaterial;
        public Color LineColor { get; set; }

        public Graph.GraphPoint graphPoint1;
        public Graph.GraphPoint graphPoint2;
        public Graph.GraphPoint graphPoint3;
        public SelectionManager selectionManager;
        public Graph.OctreeNode fromClusterNode;
        public Graph.OctreeNode toClusterNode;

        private LineRenderer lineRenderer;
        private Vector3[] linePosistions;
        private Vector3 fromPos, toPos, midPos, firstAnchor, secondAnchor;
        private Vector3 middle;
        private Vector3 currentTarget;
        private Vector3 currentPos;
        private bool initAnimate;
        private float x;
        private AnimationCurve curve;
        private int posCtr = 0;
        private GameObject fromSphere;
        private GameObject midSphere;
        private GameObject toSphere;


        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (centroids)
            {
                fromPos = t1.TransformPoint(fromGraphCentroid);
                toPos = t2.TransformPoint(toGraphCentroid);
                midPos = t3.TransformPoint(midGraphCentroid);
                firstAnchor = (fromPos + midPos) / 2f;
                secondAnchor = (midPos + toPos) / 2f;
                lineRenderer.positionCount = 5;
                linePosistions = new Vector3[] { fromPos, firstAnchor, midPos, secondAnchor, toPos };
                lineRenderer.SetPositions(new Vector3[] { fromPos, fromPos, fromPos, fromPos, fromPos });
                currentPos = linePosistions[0];
                currentTarget = linePosistions[1];
                lineRenderer.startWidth = lineRenderer.endWidth += 0.01f;
                initAnimate = true;

            }
            else
            {
                fromPos = t1.TransformPoint(graphPoint1.Position);
                toPos = t2.TransformPoint(graphPoint2.Position);
                midPos = t3.TransformPoint(graphPoint3.Position);
                lineRenderer.positionCount = 3;
                linePosistions = new Vector3[] { fromPos, midPos, toPos };
                lineRenderer.SetPositions(new Vector3[] { fromPos, fromPos, fromPos });
                currentPos = linePosistions[0];
                currentTarget = linePosistions[1];
                initAnimate = true;
            }
        }

        private void Update()
        {
            if (initAnimate)
            {
                InitLine();
            }
            else if (t1.hasChanged || t2.hasChanged)
            {
                if (centroids)
                {
                    fromPos = t1.TransformPoint(fromGraphCentroid);
                    toPos = t2.TransformPoint(toGraphCentroid);
                    midPos = t3.TransformPoint(midGraphCentroid);
                    firstAnchor = (fromPos + midPos) / 2f;
                    secondAnchor = (midPos + toPos) / 2f;
                    fromSphere.transform.position = fromPos;
                    midSphere.transform.position = midPos;
                    toSphere.transform.position = toPos;
                    lineRenderer.SetPositions(new Vector3[] { fromPos, firstAnchor, midPos, secondAnchor, toPos });
                }
                else
                {
                    fromPos = t1.TransformPoint(graphPoint1.Position);
                    toPos = t2.TransformPoint(graphPoint2.Position);
                    midPos = t3.TransformPoint(graphPoint3.Position);
                    lineRenderer.SetPositions(new Vector3[] { fromPos, midPos, toPos });
                }
            }
        }
        /// <summary>
        /// Animation that shows line progressivly move towards anchor points and lastly enpoint.
        /// </summary>
        private void InitLine()
        {
            float dist = Vector3.Distance(currentPos, currentTarget);
            x += Time.deltaTime * 2f;
            float increment = Mathf.Lerp(0, dist, x);
            if (posCtr == 0)
            {
                Vector3 pointAlongLine = (2 * increment) * Vector3.Normalize(currentTarget - currentPos) + currentPos;
                //lineRenderer.positionCount++;
                posCtr++;
                for (int i = posCtr; i < lineRenderer.positionCount; i++)
                {
                    lineRenderer.SetPosition(i, pointAlongLine);
                }
            }
            else if (dist > increment)
            {
                Vector3 pointAlongLine = increment * Vector3.Normalize(currentTarget - currentPos) + currentPos;
                for (int i = posCtr; i < lineRenderer.positionCount; i++)
                {
                    lineRenderer.SetPosition(i, pointAlongLine);
                }
                //lineRenderer.SetPosition(posCtr, pointAlongLine);
            }
            else if (dist <= increment)
            {
                if (posCtr + 1 == linePosistions.Length)
                {
                    if (centroids)
                    {
                        Color col = new Color(LineColor.r, LineColor.g, LineColor.b, 0.5f);
                        fromSphere = Instantiate(spherePrefab, transform);
                        fromSphere.GetComponent<Renderer>().material.color = col;
                        fromSphere.transform.localScale = fromClusterHull * 200;
                        fromSphere.transform.position = fromPos;

                        midSphere = Instantiate(spherePrefab, transform);
                        midSphere.GetComponent<Renderer>().material.color = col; 
                        midSphere.transform.localScale = midClusterHull * 100;
                        midSphere.transform.position = midPos;

                        toSphere = Instantiate(spherePrefab, transform);
                        toSphere.GetComponent<Renderer>().material.color = col;
                        toSphere.transform.localScale = toClusterHull * 200;
                        toSphere.transform.position = toPos;
                        //curve = new AnimationCurve();
                        //curve.AddKey(1.0f, 1.0f);
                        //curve.AddKey(0.2f, 0.10f);
                        //curve.AddKey(0.5f, 0.1f);
                        //curve.AddKey(0.8f, 0.10f);
                        //curve.AddKey(0.0f, 1.0f);
                        //lineRenderer.widthMultiplier = fromRadius;
                        //lineRenderer.widthMultiplier = 0.15f;
                        //lineRenderer.widthCurve = curve;
                        //Gradient gradient = new Gradient();
                        //gradient.SetKeys(
                        //    new GradientColorKey[] { new GradientColorKey(LineColor, 0.0f),
                        //                                new GradientColorKey(LineColor, 1.0f) },
                        //    new GradientAlphaKey[] { new GradientAlphaKey(0.1f, 0.0f), new GradientAlphaKey(0.1f, 0.2f),
                        //                                new GradientAlphaKey(0.5f, 0.2f), new GradientAlphaKey(0.1f, 0.8f),
                        //                                new GradientAlphaKey(0.1f, 1.0f) }
                        //);
                        //lineRenderer.colorGradient = gradient;
                    }
                    initAnimate = false;
                    return;
                }
                //lineRenderer.positionCount++;
                posCtr++;
                currentPos = linePosistions[posCtr - 1];
                currentTarget = linePosistions[posCtr];
                x = 0f;
            }
        }
    }
}


