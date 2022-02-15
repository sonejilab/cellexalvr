using AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Interaction;
using DefaultNamespace;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;
using DG.Tweening;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// Represents one graph with the same z - coordinate (one slice of the spatial graph).
    /// Each slice can be moved independently if in slice mode otherwise they should be moved together as one object.
    /// </summary>
    public class GraphSlice : MonoBehaviour
    {
        public bool sliceMode;
        public GameObject replacement;
        public GameObject wire;
        public bool buildingSlice;
        public Texture2D[] textures;
        public Texture2D positionTextureMap;
        public Texture2D targetTextureMap;
        public bool controllerInsideSomeBox;
        public int SliceNr
        {
            get { return sliceNr; }
            set { sliceNr = value; }
        }
        public Vector3 sliceCoords = new Vector3();
        //public Dictionary<int, float3> points = new Dictionary<int, float3>();
        public List<Point> points = new List<Point>();
        public SpatialGraph spatialGraph;
        public GraphSlice parentSlice;
        public List<GraphSlice> childSlices = new List<GraphSlice>();
        public PointCloud pointCloud;
        public GameObject referenceOrgan;
        public SlicerBox slicerBox;

        protected Graph graph;

        private Vector3 originalPos;
        private Vector3 originalSc;
        private Quaternion originalRot;
        private GameObject wirePrefab;
        private GameObject replacementPrefab;
        private Color replacementCol;
        private Color replacementHighlightCol;
        private bool grabbing;
        private int flipped = 1;
        private int sliceNr;
        private bool controllerInside;
        private List<Point> sortedPointsX;
        private List<Point> sortedPointsY;
        private List<Point> sortedPointsZ;
        private BoxCollider boxCollider;
        private int frameCount;

        private void Start()
        {
            originalPos = Vector3.zero; //transform.localPosition;
            originalRot = transform.localRotation;
            originalSc = transform.localScale;
            slicerBox = GetComponentInChildren<SlicerBox>(true);
            if (parentSlice == null)
            {
                parentSlice = this;
            }
            pointCloud = GetComponent<PointCloud>();
            boxCollider = GetComponent<BoxCollider>();
            CellexalEvents.GraphsColoredByGene.AddListener(UpdateColorTexture);
            CellexalEvents.GraphsReset.AddListener(UpdateColorTexture);
            //GetComponent<Rigidbody>().drag = Mathf.Infinity;
            //GetComponent<Rigidbody>().angularDrag = Mathf.Infinity;
        }

        private void Update()
        {
            //if (SelectionToolCollider.instance.selActive)
            //{
            //    UpdateColorTexture();
            //}

            //if (++frameCount > 10)
            //{
            //    frameCount = 0;
            //}

            //CheckForController();

        }

        //private void CheckForController()
        //{
        //    if (!boxCollider.enabled) return;
        //    Collider[] colliders = Physics.OverlapBox(transform.TransformPoint(boxCollider.center), boxCollider.size / 2, transform.rotation, 1 << LayerMask.NameToLayer("Controller") | LayerMask.NameToLayer("Player"));
        //    if (colliders.Any(x => x.CompareTag("Player") || x.CompareTag("GameController")))
        //    {
        //        //parentSlice.controllerInsideSomeBox = true;
        //        controllerInside = true;
        //        slicerBox.box.SetActive(true);
        //    }
        //    else
        //    {
        //        controllerInside = false;
        //        //parentSlice.controllerInsideSomeBox = false;
        //        if (slicerBox != null && !slicerBox.Active)
        //        {
        //            slicerBox.box.SetActive(false);
        //        }
        //    }

         
        //}

        public void UpdateColorTexture()
        {
            if (points.Count > 0)
            {
                Color[] carray = pointCloud.colorTextureMap.GetPixels();
                Color[] aarray = new Color[carray.Length];
                Texture2D parentTexture = parentSlice.pointCloud.colorTextureMap;
                Texture2D parentATexture = parentSlice.pointCloud.alphaTextureMap;
                for (int i = 0; i < points.Count; i++)
                {
                    Point p = points[i];
                    int ind = (p.yindex) * PointCloudGenerator.textureWidth + (p.xindex);
                    carray[ind] = parentTexture.GetPixel(p.orgXIndex, p.orgYIndex);
                    aarray[ind] = parentATexture.GetPixel(p.orgXIndex, p.orgYIndex);
                }

                pointCloud.colorTextureMap.SetPixels(carray);
                pointCloud.alphaTextureMap.SetPixels(aarray);
                pointCloud.colorTextureMap.Apply();
                pointCloud.alphaTextureMap.Apply();
            }
        }

        public void ClearSlices()
        {
            foreach (GraphSlice slice in childSlices)
            {
                Destroy(slice.gameObject);
            }
            childSlices.Clear();
        }

        public void MoveToGraph()
        {
            StartCoroutine(MoveToGraphCoroutine());
        }

        /// <summary>
        /// Animation to move the slice back to its original position within the parent object.
        /// </summary>
        /// <returns></returns>
        public IEnumerator MoveToGraphCoroutine()
        {
            transform.parent = parentSlice.transform;
            Vector3 startPos = transform.localPosition;
            Quaternion startRot = transform.localRotation;
            Quaternion targetRot = Quaternion.identity;

            float time = 1f;
            float t = 0f;
            while (t < time)
            {
                float progress = Mathf.SmoothStep(0, time, t);
                transform.localPosition = Vector3.Lerp(startPos, originalPos, progress);
                transform.localRotation = Quaternion.Lerp(startRot, targetRot, progress);
                t += (Time.deltaTime / time);
                yield return null;
            }

            transform.localPosition = originalPos;
            transform.localRotation = Quaternion.identity;
            //interactableObjectBasic.isGrabbable = false;
            GetComponent<BoxCollider>().enabled = false;
            //parentSlice.GetComponent<VisualEffect>().enabled = true;
            //yield return new WaitForSeconds(0.3f);
            //gameObject.SetActive(false);
            //wire.SetActive(false);
            //replacement.GetComponent<Renderer>().material.color = replacementCol;
            //replacement.SetActive(false);
        }


        /// <summary>
        /// Add replacement prefab instance. A replacement is spawned when slices is removed from parent to show where it came from.
        /// </summary>
        public void AddReplacement()
        {
            wirePrefab = spatialGraph.wirePrefab;
            replacementPrefab = spatialGraph.replacementPrefab;
            replacement = Instantiate(replacementPrefab, transform.parent);
            Vector3 maxCoords = graph.ScaleCoordinates(graph.maxCoordValues);
            replacement.transform.localPosition = new Vector3(0, maxCoords.y + 0.2f, sliceCoords.z);
            replacement.gameObject.name = "repl" + this.gameObject.name;
            replacementCol = replacement.GetComponent<Renderer>().material.color;
            replacementHighlightCol = new Color(replacementCol.r, replacementCol.g, replacementCol.b, 1.0f);
            //replacementCol = new Color(0, 205, 255, 0.3f);
            replacement.SetActive(false);

            wire = Instantiate(wirePrefab, transform.parent);
            LineRenderer lr = wire.GetComponent<LineRenderer>();
            lr.startColor = lr.endColor = new Color(255, 255, 255, 0.1f);
            lr.startWidth = lr.endWidth /= 2;
            wire.SetActive(false);
        }

        public void ActivateSlices(bool toggle)
        {
            foreach (GraphSlice gs in childSlices)
            {
                if (toggle)
                {
                    gameObject.SetActive(false);
                    //interactableObjectBasic.isGrabbable = false;
                    gs.gameObject.SetActive(true);
                    GetComponent<BoxCollider>().enabled = false;
                    gs.ActivateSlice(toggle, true);
                }

                else
                {
                    //slicerBox.gameObject.SetActive(false);
                    gameObject.SetActive(true);
                    //interactableObjectBasic.isGrabbable = true;
                    GetComponent<BoxCollider>().enabled = true;
                    gs.slicerBox.Active = false;
                    //gs.slicerBox.box.SetActive(false);
                    gs.MoveToGraph();
                }
            }

        }

        /// <summary>
        /// Activate/Deactivate a slice. Activating means the slice can be moved individually away from the parent object.
        /// When activating the slices are pulled apart slighly to make it easier to grab them.
        /// </summary>
        /// <param name="activate"></param>
        /// <returns></returns>
        public void ActivateSlice(bool activate, bool move = true)
        {
            //if (interactableObjectBasic == null)
            //{
            //    interactableObjectBasic = GetComponent<InteractableObjectBasic>();
            //}
            foreach (BoxCollider bc in GetComponents<BoxCollider>())
            {
                bc.enabled = activate;
            }

            if (activate)
            {
                //interactableObjectBasic.isGrabbable = true;
                transform.parent = null;
                //Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
                //if (rigidbody == null)
                //{
                //    rigidbody = gameObject.AddComponent<Rigidbody>();
                //}

                //rigidbody.useGravity = false;
                //rigidbody.isKinematic = false;
                //rigidbody.drag = 10;
                //rigidbody.angularDrag = 15;
                sliceMode = true;
                if (move)
                {
                    transform.DOLocalMove(sliceCoords, 0.8f).SetEase(Ease.InOutQuad);

                }
            }
            else
            {
                //Destroy(GetComponent<Rigidbody>());
                sliceMode = false;
            }
        }

        public IEnumerator FlipSlice(float animationTime)
        {
            flipped *= -1;
            Vector3 center = GetComponent<BoxCollider>().bounds.center;
            float t = 0f;
            float angle = 5f;
            float finalAngle = 0f;
            while (finalAngle <= 180)
            {
                transform.RotateAround(center, transform.up, angle);
                finalAngle += angle;
                yield return null;
            }
        }


        public void BuildPointCloud(Transform oldPc)
        {
            PointCloudGenerator.instance.creatingGraph = false;
            parentSlice = oldPc.GetComponent<GraphSlice>();
            if (pointCloud == null)
            {
                pointCloud = GetComponent<PointCloud>();
            }
            PointCloudGenerator.instance.SpawnPoints(pointCloud, parentSlice.pointCloud, points);
        }


        //public IEnumerator SliceAxisCoroutine(int axis, List<Point> points, int nrOfSlices)
        //{
        //    Thread t = t = new Thread(() => SliceAxis(axis, points, nrOfSlices));
        //    t.Start();
        //    while (t.IsAlive)
        //    {
        //        yield return null;
        //    }
        //}

        public IEnumerator SliceAxis(int axis, List<Point> points, int nrOfSlices)
        {
            GraphSlice[] oldSlices = childSlices.ToArray();
            foreach (GraphSlice gs in oldSlices)
            {
                parentSlice.childSlices.Remove(gs);
                Destroy(gs.gameObject);
            }
            List<Point> sortedPoints = new List<Point>(points.Count);
            if (axis == 0)
            {
                if (sortedPointsX == null)
                {
                    sortedPointsX = SliceGraphSystem.SortPoints(points, 0);
                }

                sortedPoints = sortedPointsX;
            }

            else if (axis == 1)
            {
                if (sortedPointsY == null)
                {
                    sortedPointsY = SliceGraphSystem.SortPoints(points, 1);
                }

                sortedPoints = sortedPointsY;
            }
            else if (axis == 2)
            {
                if (sortedPointsZ == null)
                {
                    sortedPointsZ = SliceGraphSystem.SortPoints(points, 2);
                }

                sortedPoints = sortedPointsZ;
            }

            //ClearSlices();
            List<GraphSlice> slices = new List<GraphSlice>();
            int sliceNr = 0;
            PointCloud newPc = PointCloudGenerator.instance.CreateFromOld(pointCloud.transform);
            GraphSlice slice = newPc.GetComponent<GraphSlice>();
            slice.transform.position = pointCloud.transform.position;
            slice.sliceCoords = pointCloud.transform.position;
            slice.SliceNr = ++sliceNr;
            slice.gameObject.name = "Slice" + sliceNr;
            //GraphSlice parentSlice = pc.GetComponent<GraphSlice>();
            slice.SliceNr = SliceNr;
            slices.Add(slice);
            childSlices.Add(slice);
            slice.gameObject.name = pointCloud.gameObject.name + "_" + SliceNr;


            //NativeList<Point> pointsInSlice = new NativeList<Point>(Allocator.Temp);
            float currentCoord, diff, prevCoord;
            Point point = sortedPoints[0];
            float firstCoord = prevCoord = point.offset[axis];
            float lastCoord = sortedPoints[sortedPoints.Count - 1].offset[axis];
            float epsilonToUse = math.abs(firstCoord - lastCoord) / (float)nrOfSlices;
            BoxCollider bc = newPc.GetComponent<BoxCollider>();
            Vector3 bcPos = bc.center;
            bcPos[axis] = firstCoord + epsilonToUse / 2;
            //bc.center = bcPos;
            Vector3 bcSize = bc.size;
            bcSize[axis] /= nrOfSlices;
            newPc.SetCollider(bcPos, bcSize);
            if (axis == 2)
            {
                epsilonToUse = 0.01f;
            }

            for (int i = 1; i < sortedPoints.Count; i++)
            {
                point = sortedPoints[i];
                currentCoord = point.offset[axis];
                // when we reach new slice (new x/y/z coordinate) build the graph and then start adding to a new one.
                diff = math.abs(currentCoord - firstCoord);

                if (diff > epsilonToUse)
                {
                    slice.BuildPointCloud(pointCloud.transform);
                    while (PointCloudGenerator.instance.creatingGraph)
                    {
                        yield return null;
                    }

                    yield return new WaitForSeconds(0.1f);
                    newPc = PointCloudGenerator.instance.CreateFromOld(pointCloud.transform);
                    slice = newPc.GetComponent<GraphSlice>();
                    slice.transform.position = pointCloud.transform.position;
                    slice.sliceCoords = pointCloud.transform.position;
                    slice.SliceNr = ++sliceNr;
                    slice.gameObject.name = pointCloud.gameObject.name + "_" + sliceNr;
                    slices.Add(slice);
                    childSlices.Add(slice);
                    firstCoord = currentCoord;
                    bc = newPc.GetComponent<BoxCollider>();
                    bcPos = bc.center;
                    bcPos[axis] = currentCoord + diff / 2;
                    bcSize = bc.size;
                    bcSize[axis] /= nrOfSlices;
                    newPc.SetCollider(bcPos, bcSize);
                    //yield return null;
                }

                else if (i == sortedPoints.Count - 1)
                {
                    slices.Add(slice);
                    slice.BuildPointCloud(pointCloud.transform);
                    childSlices.Add(slice);
                    while (PointCloudGenerator.instance.creatingGraph)
                    {
                        yield return null;
                    }
                }
                slice.points.Add(point);
                prevCoord = currentCoord;
            }
            for (int i = 0; i < slices.Count; i++)
            {
                Vector3 coords = Vector3.zero;
                coords[axis] = -0.5f + i * (1f / (slices.Count - 1));
                coords = transform.TransformPoint(coords);
                slices[i].sliceCoords = coords;
            }

            slicerBox.sliceAnimationActive = false;
            //ActivateSlices(true);
            //PointCloudGenerator.instance.BuildSlices(pointCloud.transform, slices.ToArray());

        }

        //private void OnTriggerEnter(Collider other)
        //{
        //    if (!parentSlice.controllerInsideSomeBox && other.CompareTag("GameController"))
        //    {
        //        parentSlice.controllerInsideSomeBox = true;
        //        controllerInside = true;
        //        slicerBox.box.SetActive(true);
        //    }

        //}

        //private void OnTriggerExit(Collider other)
        //{
        //    if (other.CompareTag("GameController"))
        //    {
        //        parentSlice.controllerInsideSomeBox = false;
        //        controllerInside = false;
        //        if (!slicerBox.Active)
        //        {
        //            slicerBox.box.SetActive(false);
        //        }
        //    }
        //}
    }

}