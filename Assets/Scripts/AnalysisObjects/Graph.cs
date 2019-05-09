using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CellexalVR.General;
using CellexalVR.Interaction;
using SQLiter;
using TMPro;
using UnityEngine;
using VRTK;

namespace CellexalVR.AnalysisObjects
{

    /// <summary>
    /// Represents a graph consisting of multiple GraphPoints.
    /// </summary>
    public class Graph : MonoBehaviour
    {
        public GameObject skeletonPrefab;
        public GameObject emptySkeletonPrefab;
        public GameObject skeletonCubePrefab;
        public Material skeletonMaterial;
        public Material lineMaterial;
        public GameObject movingOutlineCircle;
        public GameObject convexHull;
        //public string DirectoryName { get; set; }
        public List<GameObject> Lines { get; set; }
        [HideInInspector]
        public GraphManager graphManager;
        public TextMeshPro graphNameText;
        public TextMeshPro graphInfoText;
        public TextMeshPro graphNrText;
        public GameObject axes;
        public string[] axisNames = new string[3];
        public bool GraphActive = true;
        public Dictionary<string, GraphPoint> points = new Dictionary<string, GraphPoint>();
        public Dictionary<string, GraphPoint> subSelectionPoints = new Dictionary<string, GraphPoint>();
        public List<Vector3> pointsPositions;
        public ReferenceManager referenceManager;
        public List<GameObject> graphPointClusters = new List<GameObject>();
        public int textureWidth;
        public int textureHeight;
        public Texture2D texture;
        private bool textureChanged;
        public Vector3 minCoordValues = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        public Vector3 maxCoordValues = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        public Vector3 diffCoordValues;
        public float longestAxis;
        public Vector3 scaledOffset;
        public int nbrOfClusters;
        public string GraphName
        {
            get { return graphName; }
            set
            {
                graphName = value;
                graphNameText.text = value;
                this.name = graphName;
                this.gameObject.name = graphName;
            }
        }
        public string FolderName
        {
            get { return folderName; }
            set
            {
                folderName = value;
                graphNameText.text = folderName + "_" + graphName;
                //graphNameText.text = value;
            }
        }
        public int GraphNumber
        {
            get { return graphNr; }
            set
            {
                graphNr = value;
                graphNrText.text = value.ToString();
            }
        }
        
        private ControllerModelSwitcher controllerModelSwitcher;
        private GameManager gameManager;
        private Vector3 startPosition;
        private List<Vector3> nodePosition;
        private List<Vector3> nodeSizes;

        // For minimization animation
        private bool minimize;
        private bool maximize;
        private float speed;

        private bool minimized;
        //private Vector3 targetPos;
        private float targetMinScale;
        private float targetMaxScale;
        private float shrinkSpeed;
        private Vector3 oldPos;
        private Quaternion oldRot;
        private Vector3 oldScale;

        // Debug stuff
        private Vector3 debugGizmosPos;
        private Vector3 debugGizmosMin;
        private Vector3 debugGizmosMax;
        private string graphName;
        private string folderName;
        private int graphNr;
        private int nbrOfExpressionColors;
        /// <summary>
        /// The name of this graph. Should just be the filename that the graph came from.
        /// </summary>


        private static LayerMask selectionToolLayerMask;

        public OctreeNode octreeRoot;
        private GraphGenerator graphGenerator;
        void Start()
        {
            speed = 1.5f;
            //targetPos = Vector3.zero;
            shrinkSpeed = 2f;
            targetMinScale = 0.05f;
            targetMaxScale = 1f;
            oldPos = new Vector3();
            graphManager = referenceManager.graphManager;
            gameManager = referenceManager.gameManager;
            graphManager = referenceManager.graphManager;
            Lines = new List<GameObject>();
            controllerModelSwitcher = referenceManager.controllerModelSwitcher;
            graphGenerator = referenceManager.graphGenerator;
            selectionToolLayerMask = 1 << LayerMask.NameToLayer("SelectionToolLayer");
            startPosition = transform.position;
            nbrOfExpressionColors = CellexalConfig.Config.GraphNumberOfExpressionColors;
        }

        private void Update()
        {
            if (textureChanged && texture != null)
            {
                texture.Apply();
                textureChanged = false;
            }

            if (GetComponent<VRTK_InteractableObject>().IsGrabbed())
            {
                gameManager.InformMoveGraph(GraphName, transform.position, transform.rotation, transform.localScale);
            }
            if (minimize)
            {
                Minimize();
            }
            if (maximize)
            {
                Maximize();
            }
        }

        internal void ShowGraph()
        {
            transform.position = referenceManager.leftController.transform.position;
            GraphActive = true;
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                r.enabled = true;

            //foreach (GameObject line in Lines)
            //    line.SetActive(true);
            maximize = true;
        }

        /// <summary>
        /// Animation for showing graph.
        /// </summary>
        void Maximize()
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, oldPos, step);
            transform.localScale += Vector3.one * Time.deltaTime * shrinkSpeed;
            transform.Rotate(Vector3.one * Time.deltaTime * -100);
            //gameManager.InformMoveGraph(GraphName, transform.position, transform.rotation, transform.localScale);
            if (transform.localScale.x >= targetMaxScale)
            {
                transform.localScale = oldScale;
                transform.localPosition = oldPos;
                CellexalLog.Log("Maximized object" + name);
                maximize = false;
                GraphActive = true;
                foreach (Collider c in GetComponentsInChildren<Collider>())
                    c.enabled = true;
                minimized = false;
                //foreach (GameObject line in Lines)
                //    line.SetActive(true);
            }
        }
        /// <summary>
        /// Turns off all renderers and colliders for this graph. And starts the minimazation process by sett minimize to true.
        /// </summary>
        internal void HideGraph()
        {
            GraphActive = false;
            //targetPos = referenceManager.minimizedObjectHandler.transform.position;
            foreach (Collider c in GetComponentsInChildren<Collider>())
                c.enabled = false;
            foreach (GameObject line in Lines)
                line.SetActive(false);
            oldPos = transform.position;
            oldScale = transform.localScale;
            minimize = true;
        }



        /// <summary>
        /// Same as above but called with arguments when you want to modify the pos and min scale.
        /// </summary>
        /// <param name="pos">Position to minimize towards.</param>
        /// <param name="targetScale">Minimum scale before hiding object completely.</param>
        //internal void HideGraph(Vector3 pos, float targetScale)
        //{
        //    GraphActive = false;
        //    targetMinScale = targetScale;
        //    targetPos = pos;
        //    foreach (Collider c in GetComponentsInChildren<Collider>())
        //        c.enabled = false;
        //    foreach (GameObject line in Lines)
        //        line.SetActive(false);
        //    oldPos = transform.position;
        //    oldScale = transform.localScale;
        //    minimize = true;
        //}

        /// <summary>
        /// Animation for hiding graph.
        /// </summary>
        void Minimize()
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, referenceManager.leftController.transform.position, step);
            transform.localScale -= Vector3.one * Time.deltaTime * shrinkSpeed;
            transform.Rotate(Vector3.one * Time.deltaTime * 100);
            //gameManager.InformMoveGraph(GraphName, transform.position, transform.rotation, transform.localScale);
            if (transform.localScale.x <= targetMinScale)
            {
                minimize = false;
                GraphActive = false;
                foreach (Renderer r in GetComponentsInChildren<Renderer>())
                    r.enabled = false;
                foreach (GameObject line in Lines)
                    line.SetActive(false);
                referenceManager.minimizeTool.GetComponent<Light>().range = 0.04f;
                referenceManager.minimizeTool.GetComponent<Light>().intensity = 0.8f;
                minimized = true;
            }
        }

        public Vector3 ScaleCoordinates(Vector3 coords)
        {
            // move one of the graph's corners to origo
            coords -= minCoordValues;

            // uniformly scale all axes down based on the longest axis
            // this makes the longest axis have length 1 and keeps the proportions of the graph
            coords /= longestAxis;

            // move the graph a bit so (0, 0, 0) is the center point
            coords -= scaledOffset;
            return coords;
        }

        public class GraphPoint
        {
            private static int indexCounter = 0;

            public string Label;
            public Vector3 Position;
            public GameObject cluster;
            public int index;
            public Vector2Int textureCoord;
            public Graph parent;
            private int group;
            public int Group
            {
                get { return group; }
                set { SetGroup(value); }
            }
            public bool unconfirmedInSelection;
            public List<Selectable> lineBetweenCellsCubes;
            public Vector3 WorldPosition { get { return parent.transform.TransformPoint(Position); } }
            public OctreeNode node;

            public GraphPoint(string label, float x, float y, float z, Graph parent)
            {
                Label = label;
                Position = new Vector3(x, y, z);
                this.parent = parent;
                index = indexCounter;
                group = -1;
                unconfirmedInSelection = false;
                indexCounter++;
                lineBetweenCellsCubes = new List<Selectable>();
            }

            public override string ToString()
            {
                return Label;
            }

            public void ScaleCoordinates()
            {
                // move one of the graph's corners to origo
                Position -= parent.minCoordValues;

                // uniformly scale all axes down based on the longest axis
                // this makes the longest axis have length 1 and keeps the proportions of the graph
                Position /= parent.longestAxis;

                // move the graph a bit so (0, 0, 0) is the center point
                Position -= parent.scaledOffset;
            }

            public void SetTextureCoord(Vector2Int newPos)
            {
                textureCoord = newPos;
            }

            public void RecolorGeneExpression(int i, bool outline)
            {
                parent.RecolorGraphPointGeneExpression(this, i, outline);
            }

            public void RecolorSelectionColor(int i, bool outline)
            {
                parent.RecolorGraphPointSelectionColor(this, i, outline);
                Group = i;
            }

            public void ResetColor()
            {
                parent.ResetGraphPointColor(this);
                Group = -1;
            }

            public Color GetColor()
            {
                return parent.GetGraphPointColor(this);
            }

            private void SetGroup(int group)
            {
                this.group = group;
                if (node.Group != group)
                    node.Group = group;
            }
        }

        /// <summary>
        /// Private class to represent one node in the octree used for collision detection
        /// </summary>
        public class OctreeNode
        {
            public OctreeNode parent;
            /// <summary>
            /// Not always of length 8. Empty children are removed when the octree is constructed to save some memory.
            /// </summary>
            public OctreeNode[] children;

            /// <summary>
            /// Null if this is not a leaf. Only leaves represents points in the graph.
            /// </summary>
            public GraphPoint point;
            public Vector3 pos;
            /// <summary>
            /// Not the geometrical center, just a point inside the node that defines corners for its children, unless this is a leaf node.
            /// </summary>
            public Vector3 center;
            public Vector3 size;
            private int group = -1;
            private bool raycasted;
            /// <summary>
            /// The group that this node belongs to. -1 means no group, 0 or a positive number means some group.
            /// If this is a leaf node, this should be the same as the group that the selection tool has given the <see cref="GraphPoint"/>.
            /// If this is not a leaf node, this is not -1 if all its children are of that group.
            /// </summary>
            public int Group
            {
                get { return group; }
                set { SetGroup(value); }
            }
            public bool rejected;
            public bool completelyInside;

            public OctreeNode() { }

            public bool nodeIterated;
            /// <summary>
            /// Returns a string representation of this node and all its children. May produce very long strings for very large trees.
            /// </summary>
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                ToStringRec(ref sb);
                return sb.ToString();
            }

            private void ToStringRec(ref StringBuilder sb)
            {
                if (point != null)
                {
                    sb.Append(point.Label);
                }
                else
                {
                    if (children.Length > 0)
                    {
                        sb.Append("(");
                        foreach (var child in children)
                        {
                            child.ToStringRec(ref sb);
                            sb.Append(", ");
                        }
                        sb.Remove(sb.Length - 2, 2);
                        sb.Append(")");
                    }
                }
            }

            private void SetGroup(int group)
            {
                this.group = group;
                foreach (var child in children)
                {
                    child.SetGroup(group);
                }
                if (parent != null && parent.group != group)
                {
                    parent.NotifyGroupChange(group);
                }
            }

            private void NotifyGroupChange(int group)
            {
                int setGroupTo = group;
                // only set this node's group if all children are also of that group
                foreach (var child in children)
                {
                    if (child.group != group)
                    {
                        // not all children were of that group, set this node's group to -1
                        setGroupTo = -1;
                        break;
                    }
                }
                this.group = setGroupTo;
                if (parent != null)
                {
                    parent.NotifyGroupChange(group);
                }
            }

            /// <summary>
            /// Changes the group of this node and all of its children (and grand-children and so on) recursively.
            /// </summary>
            /// <param name="group">The new group.</param>
            public void ChangeGroupRecursively(int group)
            {
                this.group = group;
                foreach (var child in children)
                {
                    child.ChangeGroupRecursively(group);
                }
            }

            public List<Vector3> GetSkeletonNodesRecursive(OctreeNode node, List<Vector3> nodePositions = null, int nodeLevel = 0)
            {
                if (nodePositions == null)
                {
                    nodePositions = new List<Vector3>();
                }
                foreach (OctreeNode child in node.children)
                {
                    if (nodeLevel == 4)
                    {
                        nodePositions.Add(child.center);
                        return nodePositions;
                    }
                    else
                    {
                        child.GetSkeletonNodesRecursive(child, nodePositions, nodeLevel + 1);
                    }
                }
                return nodePositions;
            }

            /// <summary>
            /// Returns the smallest node that contains a point. The retured node might not be a leaf.
            /// </summary>
            /// <param name="point">The point to look for a node around.</param>
            /// <returns>The that contains the point.</returns>
            public OctreeNode NodeContainingPoint(Vector3 point)
            {
                if (children.Length == 0)
                {
                    return this;
                }
                foreach (OctreeNode child in children)
                {
                    if (child.PointInside(point))
                    {
                        return child.NodeContainingPoint(point);
                    }
                }
                return this;
            }

            /// <summary>
            /// Checks if a point is inside this node.
            /// </summary>
            public bool PointInside(Vector3 point)
            {
                return point.x >= pos.x && point.x <= pos.x + size.x
                    && point.y >= pos.y && point.y <= pos.y + size.y
                    && point.z >= pos.z && point.z <= pos.z + size.z;
            }

            /// <summary>
            /// Checks if a point is inside the selection tool by raycasting.
            /// </summary>
            /// <param name="selectionToolCenter">The selection tool's position in world space.</param>
            /// <param name="pointPosWorldSpace">This node's position in world space. (use Transform.TransformPoint(node.splitCenter) )</param>
            /// <returns>True if <paramref name="pointPosWorldSpace"/> is inside the selection tool.</returns>
            public bool PointInsideSelectionTool(Vector3 selectionToolCenter, Vector3 pointPosWorldSpace)
            {
                raycasted = true;
                Vector3 difference = selectionToolCenter - pointPosWorldSpace;
                return !Physics.Raycast(pointPosWorldSpace, difference, difference.magnitude, Graph.selectionToolLayerMask);
            }

            #region DEBUG_FUNCTIONS
            public void DebugColorLeaves(Color color)
            {
                if (point != null)
                {
                    point.RecolorSelectionColor(0, false);
                }
                foreach (var child in children)
                {
                    child.DebugColorLeaves(color);
                }
            }

            public void DrawDebugCubesRecursive(Vector3 gameobjectPos, bool onlyLeaves, int i)
            {
                Gizmos.color = GizmoColors(i++);
                if (!onlyLeaves || children.Length == 0 && onlyLeaves)
                {
                    Gizmos.DrawWireCube(gameobjectPos + pos + size / 2, size / 0.95f);
                }
                foreach (var child in children)
                {
                    child.DrawDebugCubesRecursive(gameobjectPos, onlyLeaves, i);
                }

            }

            public IEnumerator DrawSkeletonCubes(List<Vector3> nodePositions, List<Vector3> nodeSizes, int nodeLevel)
            {
                if (nodePositions == null)
                {
                    nodePositions = new List<Vector3>();
                }
                foreach (OctreeNode child in children)
                {
                    foreach (OctreeNode c in child.children)
                    {
                        foreach (OctreeNode on in c.children)
                        {
                            nodeSizes.Add(c.size);
                            nodePositions.Add(c.center);
                            //print("adding nodes");
                            //foreach (OctreeNode onc in on.children)
                            //{

                            //}
                        }
                    }
                    yield return null;
                }
                nodeIterated = true;

            }


            public void DrawDebugLines(Vector3 gameobjectPos)
            {
                //if (children.Length != 0)
                //{
                //    Gizmos.color = Color.red;
                //    Gizmos.DrawWireSphere(gameobjectPos + center, 0.03f);
                //    Gizmos.color = Color.white;
                //}
                foreach (var child in children)
                {
                    Gizmos.DrawLine(gameobjectPos + center, gameobjectPos + child.center);
                    child.DrawDebugLines(gameobjectPos);
                }
            }

            public void DrawDebugRaycasts(Vector3 gameobjectPos, Vector3 selectionToolPos)
            {
                if (raycasted)
                {
                    Gizmos.DrawLine(center + gameobjectPos, selectionToolPos);
                    raycasted = false;
                }
                else
                {
                    foreach (var child in children)
                    {
                        child.DrawDebugRaycasts(gameobjectPos, selectionToolPos);
                    }
                }
            }

            public void DrawDebugGroups(Vector3 gameobjectPos)
            {
                Gizmos.color = GizmoColors(group);
                Gizmos.DrawWireCube(gameobjectPos + pos + size / 2, size / 0.95f);
                foreach (var child in children)
                {
                    child.DrawDebugGroups(gameobjectPos);
                }
            }

            private Color GizmoColors(int i)
            {
                if (i == -1)
                {
                    return Color.white;
                }
                i = i % CellexalConfig.Config.SelectionToolColors.Length;
                return CellexalConfig.Config.SelectionToolColors[i];
            }
            #endregion
        }

        public void OnDrawGizmos()
        {
            if (octreeRoot != null)
            {
                if (graphManager.drawDebugCubes)
                {
                    octreeRoot.DrawDebugCubesRecursive(transform.position, false, 0);
                }
                if (graphManager.drawDebugLines)
                {
                    Gizmos.color = Color.white;
                    octreeRoot.DrawDebugLines(transform.position);
                }
            }
            if (graphManager.drawSelectionToolDebugLines)
            {
                Gizmos.color = Color.green;
                DrawDebugCube(Color.green, transform.TransformPoint(debugGizmosMin), transform.TransformPoint(debugGizmosMax));
                Gizmos.DrawSphere(debugGizmosMin, 0.01f);
                Gizmos.DrawSphere(debugGizmosMax, 0.01f);
                //print("debug lines: " + debugGizmosMin.ToString() + " " + debugGizmosMax.ToString());
            }
            if (graphManager.drawDebugRaycast)
            {
                octreeRoot.DrawDebugRaycasts(transform.position, debugGizmosPos);
            }
            if (graphManager.drawDebugRejectionApprovedCubes)
            {
                DrawRejectionApproveCubes(octreeRoot);
            }
            if (graphManager.drawDebugGroups)
            {
                octreeRoot.DrawDebugGroups(transform.position);
            }

        }


        /// <summary>
        /// Coroutine to create the graph skeleton using the nodes from the octree. Each octreenode on a certain level will be represented by a transparant cube.
        /// </summary>
        /// <returns></returns>
        public IEnumerator CreateGraphSkeletonCoroutine()
        {
            //MeshFilter convexHull = null;
            convexHull = Instantiate(skeletonPrefab);
            convexHull.gameObject.SetActive(false);
            List<Vector3> nodePositions = new List<Vector3>();
            List<Vector3> sizes = new List<Vector3>();
            List<MeshFilter> meshes = new List<MeshFilter>();
            StartCoroutine(octreeRoot.DrawSkeletonCubes(nodePositions, sizes, 0));
            //octreeRoot.DrawSkeletonCubes(nodePositions, sizes, 0);
            //octreeRoot.DrawSkeletonCubes(ref nodePositions, ref sizes, 0);
            int i = 0;
            while (!octreeRoot.nodeIterated)
            {
                yield return null;
            }
            foreach (Vector3 pos in nodePositions)
            {
                var cube = Instantiate(skeletonCubePrefab, convexHull.transform);
                meshes.Add(cube.GetComponent<MeshFilter>());
                cube.transform.localPosition = pos;
                cube.transform.localScale = Vector3.one * 0.025f;
                //cube.transform.localScale = sizes[i];
                i++;
            }
            CombineInstance[] combine = new CombineInstance[meshes.Count];
            i = 0;
            while (i < meshes.Count)
            {
                combine[i].mesh = meshes[i].sharedMesh;
                combine[i].transform = meshes[i].transform.localToWorldMatrix;
                meshes[i].gameObject.SetActive(false);
                //yield return null;
                i++;
            }
            convexHull.GetComponent<MeshFilter>().mesh = new Mesh();
            convexHull.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
            convexHull.GetComponent<MeshRenderer>().material = skeletonMaterial;

            convexHull.gameObject.SetActive(true);

            //CreateGraphSkeleton(convexHull);
            //return convexHull.gameObject;
        }

        /// <summary>
        /// Method to greate lines between nodes in the octree. This gives a sort of skeleton representation of the graph.
        /// </summary>
        /// <param name="convhull"></param>
        /// <returns></returns>
        public IEnumerator CreateGraphSkeleton(bool empty = false)
        {
            if (empty)
            {
                convexHull = Instantiate(emptySkeletonPrefab);
            }
            else
            {
                convexHull = Instantiate(skeletonPrefab);
            }
            var nodes = octreeRoot.GetSkeletonNodesRecursive(octreeRoot, null, 0);
            int posCount = nodes.Count;
            nodes.OrderBy(v => v.x).ToList();
            var sortedNodes = new List<Vector3>();
            var subNodes = nodes;
            int frameCount = 0;
            while (nodes.Count > 0)
            {
                var firstNode = subNodes[0];
                sortedNodes.Add(firstNode);
                nodes.Remove(firstNode);
                subNodes = nodes.OrderBy(v => Vector3.Distance(firstNode, v)).ToList();
                frameCount++;
                if (nodes.Count % 100 == 0)
                    yield return null;
            }
            LineRenderer line = convexHull.gameObject.AddComponent<LineRenderer>();
            line.material = lineMaterial;
            line.startWidth = line.endWidth = 0.02f;
            line.useWorldSpace = false;
            line.alignment = LineAlignment.TransformZ;
            line.positionCount = posCount;
            line.SetPositions(sortedNodes.ToArray());

            convexHull.GetComponent<BoxCollider>().size = GetComponent<BoxCollider>().size;
            convexHull.SetActive(true);

            //return convexHull;
        }

        private void DrawDebugCube(Color color, Vector3 min, Vector3 max)
        {
            min += transform.position;
            max += transform.position;
            Gizmos.color = color;
            Vector3[] corners = new Vector3[]
            {
           min,
           new Vector3(max.x, min.y, min.z),
           new Vector3(min.x, max.y, min.z),
           new Vector3(min.x, min.y, max.z),
           new Vector3(max.x, max.y, min.z),
           new Vector3(max.x, min.y, max.z),
           new Vector3(min.x, max.y, max.z),
           max
            };

            Gizmos.DrawLine(corners[0], corners[1]);
            Gizmos.DrawLine(corners[0], corners[2]);
            Gizmos.DrawLine(corners[0], corners[3]);
            Gizmos.DrawLine(corners[1], corners[4]);
            Gizmos.DrawLine(corners[1], corners[5]);
            Gizmos.DrawLine(corners[2], corners[4]);
            Gizmos.DrawLine(corners[2], corners[6]);
            Gizmos.DrawLine(corners[3], corners[5]);
            Gizmos.DrawLine(corners[3], corners[6]);
            Gizmos.DrawLine(corners[4], corners[7]);
            Gizmos.DrawLine(corners[5], corners[7]);
            Gizmos.DrawLine(corners[6], corners[7]);

        }

        private void DrawRejectionApproveCubes(OctreeNode node)
        {
            if (node.rejected)
            {
                node.rejected = false;
                DrawDebugCube(Color.red, node.pos, node.pos + node.size);
            }
            else if (node.completelyInside)
            {
                node.completelyInside = false;
                DrawDebugCube(Color.green, node.pos, node.pos + node.size);
            }

            foreach (var child in node.children)
            {
                DrawRejectionApproveCubes(child);
            }
        }

        /// <summary>
        /// Tells this graph that all graphpoints are added to this graph and we can update the info text.
        /// </summary>
        public void SetInfoText()
        {
            //graphInfoText.transform.parent.localPosition = ScaleCoordinates(maxCoordValues.x - (maxCoordValues.x - minCoordValues.x) / 2, maxCoordValues.y, maxCoordValues.z);
            //graphInfoText.text = "Points: " + points.Count;
            graphNameText.text += "\n Points: " + points.Count;
            //SetInfoTextVisible(true);
        }

        /// <summary>
        /// Set this graph's info panel visible or not visible.
        /// </summary>
        /// <param name="visible"> True for visible, false for invisible </param>
        public void SetInfoTextVisible(bool visible)
        {
            graphInfoText.transform.parent.gameObject.SetActive(visible);
        }

        /// <summary>
        /// Set this graph's axes visible or not visible.
        /// </summary>
        /// <param name="visible"> True for visible, false for invisible </param>
        public void SetAxesVisible(bool visible)
        {
            if (axes != null)
            {
                axes.SetActive(visible);
            }
        }

        public void ResetColorsAndPosition()
        {
            ResetColors();
            transform.position = startPosition;
        }

        public void ResetSizeAndRotation()
        {
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Resets the color of all graphpoints in this graph to white.
        /// </summary>
        public void ResetColors()
        {
            for (int i = 0; i < textureWidth; ++i)
            {
                for (int j = 0; j < textureHeight; ++j)
                {
                    texture.SetPixel(i, j, Color.red);
                }
            }
            texture.Apply();

            //foreach (CombinedGraphPoint p in points.Values)
            //{
            //    p.Group = -1;
            //}
            octreeRoot.Group = -1;
        }

        public Color GetGraphPointColor(GraphPoint gp)
        {
            int group = (int)(255 * texture.GetPixel(gp.textureCoord.x, gp.textureCoord.y).r);
            return graphGenerator.graphPointColors.GetPixel(group, 0);
        }

        public void Party()
        {
            for (int i = 0; i < textureWidth; ++i)
            {
                for (int j = 0; j < textureHeight; ++j)
                {
                    texture.SetPixel(i, j, Color.blue);
                }
            }
            texture.Apply();
        }

        /// <summary>
        /// Resets this graphs position, scale and color.
        /// </summary>
        //    public void ResetGraph()
        //    {
        //        transform.localScale = defaultScale;
        //        transform.position = defaultPos;
        //        transform.rotation = Quaternion.identity;
        //        foreach (GraphPoint point in points.Values)
        //        {
        //            point.gameObject.SetActive(true);
        //            point.ResetCoords();
        //            point.ResetColor();
        //        }
        //    }

        /// <summary>
        /// Recolors a single graphpoint.
        /// </summary>
        /// <param name="graphPoint">The graphpoint to recolor.</param>
        /// <param name="color">The graphpoint's new color.</param>
        public void RecolorGraphPointGeneExpression(GraphPoint graphPoint, int i, bool outline)
        {
            //byte greenChannel = (byte)(outline || i > 27 ? 27 : 0);
            if (i > 27 && CellexalConfig.Config.GraphMostExpressedMarker)
            {
                var circle = Instantiate(movingOutlineCircle);
                circle.GetComponent<MovingOutlineCircle>().camera = referenceManager.headset.transform;
                circle.transform.position = graphPoint.WorldPosition;
                circle.transform.parent = transform;
            }
            else if (i == -1)
            {
                i = 255;
            }
            Color32 finalColor = new Color32((byte)i, 0, 0, 255);
            texture.SetPixels32(graphPoint.textureCoord.x, graphPoint.textureCoord.y, 1, 1, new Color32[] { finalColor });
            textureChanged = true;
        }

        public void RecolorGraphPointSelectionColor(GraphPoint graphPoint, int i, bool outline)
        {
            byte greenChannel = (byte)(outline ? 5 : 0);
            byte redChannel;
            if (i == -1)
            {
                redChannel = 255;
            }
            else
            {
                redChannel = (byte)(nbrOfExpressionColors + i);
            }
            Color32 finalColor = new Color32(redChannel, greenChannel, 0, 255);
            texture.SetPixels32(graphPoint.textureCoord.x, graphPoint.textureCoord.y, 1, 1, new Color32[] { finalColor });
            textureChanged = true;
        }

        public void ResetGraphPointColor(GraphPoint graphPoint)
        {
            texture.SetPixels32(graphPoint.textureCoord.x, graphPoint.textureCoord.y, 1, 1, new Color32[] { new Color32(255, 0, 0, 255) });
            textureChanged = true;
        }

        public GraphPoint FindGraphPoint(string label)
        {
            return points[label];
        }

        /// <summary>
        /// Color all graphpoints in this graph with the expression of some gene.
        /// </summary>
        /// <param name="expressions">An arraylist with <see cref="CellExpressionPair"/>.</param>
        public void ColorByGeneExpression(ArrayList expressions)
        {
            // expression values are saved in the textures red channel
            // cells that have 0 (or whatever the lowest is) expression are not in the results
            // fill entire background with the lowest expression color
            for (int i = 0; i < textureWidth; ++i)
            {
                for (int j = 0; j < textureHeight; ++j)
                {
                    texture.SetPixel(i, j, Color.black);
                }
            }

            int nbrOfExpressionColors = CellexalConfig.Config.GraphNumberOfExpressionColors;
            Color32[][] colorValues = new Color32[nbrOfExpressionColors][];
            for (byte i = 0; i < nbrOfExpressionColors - 3; ++i)
            {
                colorValues[i] = new Color32[] { new Color32(i, 0, 0, 1) };
            }
            for (byte i = (byte)(nbrOfExpressionColors - 3); i < nbrOfExpressionColors; ++i)
            {
                // the highest expression levels get 27 (somewhere between 0.1 and 0.2 when converted to a float) in the green channel to get an outline by the shader
                colorValues[i] = new Color32[] { new Color32(i, 0, 0, 1) };
            }
            int topExpressedThreshold = (int)(nbrOfExpressionColors - nbrOfExpressionColors / 10f);
            foreach (CellExpressionPair pair in expressions)
            {
                // If this is a subgraph it does not contain all cells...
                if (points.ContainsKey(pair.Cell))
                {
                    Vector2Int pos = points[pair.Cell].textureCoord;
                    int expressionColorIndex = pair.Color;
                    if (pair.Color >= nbrOfExpressionColors)
                    {
                        expressionColorIndex = nbrOfExpressionColors - 1;
                    }

                    if (CellexalConfig.Config.GraphMostExpressedMarker && pair.Color >= topExpressedThreshold && !minimized)
                    {
                        var circle = Instantiate(movingOutlineCircle);
                        circle.GetComponent<MovingOutlineCircle>().camera = referenceManager.headset.transform;
                        circle.transform.position = points[pair.Cell].WorldPosition;
                        circle.transform.parent = transform;
                    }
                    texture.SetPixels32(pos.x, pos.y, 1, 1, colorValues[expressionColorIndex]);
                }
            }

            texture.Apply();
            //var pixels = texture.GetPixels32(0);
            //int[] hist = new int[nbrOfExpressionColors];
            //for (int i = 0; i < pixels.Length; ++i)
            //{
            //    hist[pixels[i].r]++;
            //}

            graphPointClusters[0].GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
        }

        /// <summary>
        /// Finds all <see cref="GraphPoint"/> that are inside the selection tool. This is done by traversing the generated Octree and dismissing subtrees using Minkowski differences.
        /// Ultimately, raycasting is used to find collisions because the selection tool is not a box.
        /// </summary>
        /// <param name="selectionToolPos">The selection tool's position in world space.</param>
        /// <param name="selectionToolBoundsCenter">The selection tool's bounding box's center in world space.</param>
        /// <param name="selectionToolBoundsExtents">The selection tool's bounding box's extents in world space.</param>
        /// <param name="group">The group that the selection tool is set to color the graphpoints by.</param>
        /// <returns>A <see cref="List{CombinedGraphPoint}"/> with all <see cref="GraphPoint"/> that are inside the selecion tool.</returns>
        public List<GraphPoint> MinkowskiDetection(Vector3 selectionToolPos, Vector3 selectionToolBoundsCenter, Vector3 selectionToolBoundsExtents, int group, float ms)
        {
            List<GraphPoint> result = new List<GraphPoint>(64);
            //int calls = 0;
            //int callsEntirelyInside = 0;

            // calculate a new (non-minimal) bounding box for the selection tool, in the graph's local space
            Vector3 center = transform.InverseTransformPoint(selectionToolBoundsCenter);

            Vector3 axisX = transform.InverseTransformVector(selectionToolBoundsExtents.x, 0, 0);
            Vector3 axisY = transform.InverseTransformVector(0, selectionToolBoundsExtents.y, 0);
            Vector3 axisZ = transform.InverseTransformVector(0, 0, selectionToolBoundsExtents.z);

            float extentsx = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
            float extentsy = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
            float extentsz = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

            Vector3 extents = new Vector3(extentsx, extentsy, extentsz);
            Vector3 min = center - extents;
            Vector3 max = center + extents;

            //debugGizmosMin = min;
            //debugGizmosMax = max;

            debugGizmosPos = selectionToolPos;

            MinkowskiDetectionRecursive(selectionToolPos, min, max, octreeRoot, group, ref result, ms);
            return result;
        }

        /// <summary>
        /// Recursive function to traverse the Octree and find collisions with the selection tool.
        /// </summary>
        /// <param name="selectionToolWorldPos">The selection tool's position in world space.</param>
        /// <param name="boundingBoxMin">A <see cref="Vector3"/> comprised of the smallest x, y and z coordinates of the selection tool's bounding box.</param>
        /// <param name="boundingBoxMax">A <see cref="Vector3"/> comprised of the largest x, y and z coordinates of the selection tool's bounding box.</param>
        /// <param name="node">The <see cref="OctreeNode"/> to evaluate.</param>
        /// <param name="group">The group to assign the node.</param>
        /// <param name="result">All leaf nodes found so far.</param>
        private void MinkowskiDetectionRecursive(Vector3 selectionToolWorldPos, Vector3 boundingBoxMin, Vector3 boundingBoxMax, OctreeNode node, int group, ref List<GraphPoint> result, float ms)
        {
            //print(Time.realtimeSinceStartup);
            //if (Time.realtimeSinceStartup - ms > 25)
            //{
            //    print("stopped due to stopwatch - " + (Time.realtimeSinceStartup - ms));
            //    return;
            //}
            //calls++;
            // minkowski difference selection tool bounding box and node
            // take advantage of both being AABB
            // check if result contains (0,0,0)
            if (boundingBoxMin.x - node.pos.x - node.size.x <= 0
                && boundingBoxMax.x - node.pos.x >= 0
                && boundingBoxMin.y - node.pos.y - node.size.y <= 0
                && boundingBoxMax.y - node.pos.y >= 0
                && boundingBoxMin.z - node.pos.z - node.size.z <= 0
                && boundingBoxMax.z - node.pos.z >= 0)
            {
                // check if this node is entirely inside the bounding box
                if (boundingBoxMin.x < node.pos.x && boundingBoxMax.x > node.pos.x + node.size.x &&
                    boundingBoxMin.y < node.pos.y && boundingBoxMax.y > node.pos.y + node.size.y &&
                    boundingBoxMin.z < node.pos.z && boundingBoxMax.z > node.pos.z + node.size.z)
                {
                    // just find the leaves and check if they are inside

                    if (node.Group != group)
                    {
                        node.completelyInside = true;
                        CheckIfLeavesInside(selectionToolWorldPos, node, group, ref result, ms);
                    }
                    return;
                }

                // check if this is a leaf node that is inside the selection tool. Can't rely on bounding boxes here, have to raycast to find collisions
                if (node.point != null && node.Group != group && node.PointInsideSelectionTool(selectionToolWorldPos, transform.TransformPoint(node.center)))
                {
                    node.Group = group;
                    result.Add(node.point);
                }
                else
                {
                    // recursion
                    foreach (var child in node.children)
                    {
                        if (child.Group != group)
                        {
                            MinkowskiDetectionRecursive(selectionToolWorldPos, boundingBoxMin, boundingBoxMax, child, group, ref result, ms);
                        }
                    }
                }
            }
            else
            {
                node.rejected = true;
            }
        }

        /// <summary>
        /// Recursive function to traverse the Octree and find collisions with the selection tool.
        /// </summary>
        /// <param name="selectionToolWorldPos">The selection tool's position in world space.</param>
        /// <param name="node">The to evaluate.</param>
        /// <param name="group">The group to assign the node.</param>
        /// <param name="result">All leaf nodes found so far.</param>
        private void CheckIfLeavesInside(Vector3 selectionToolWorldPos, OctreeNode node, int group, ref List<GraphPoint> result, float ms)
        {
            //if (Time.realtimeSinceStartup - ms > 250)
            //{
            //    print("stopped due to stopwatch - " + (Time.realtimeSinceStartup - ms));
            //    return;
            //}
            //callsEntirelyInside++;
            if (node.point != null && node.PointInsideSelectionTool(selectionToolWorldPos, transform.TransformPoint(node.center)))
            {
                node.Group = group;
                result.Add(node.point);
                return;
            }
            foreach (var child in node.children)
            {
                if (child.Group == group)
                {
                    continue;
                }
                if (child.point != null && child.PointInsideSelectionTool(selectionToolWorldPos, transform.TransformPoint(child.center)))
                {
                    child.Group = group;
                    result.Add(child.point);
                }
                else
                {
                    CheckIfLeavesInside(selectionToolWorldPos, child, group, ref result, ms);
                }
            }
        }
    }
}