﻿using CellexalVR.AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Interaction;
using CellexalVR.Menu.Buttons;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// Represents one graph with the same z - coordinate (one slice of the spatial graph).
    /// Each slice can be moved independently if in slice mode otherwise they should be moved together as one object.
    /// </summary>
    public class GraphSlice : MonoBehaviour
    {
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
        public List<Point> points = new List<Point>();
        public SpatialGraph spatialGraph;
        public PointCloud parentPC;
        public GraphSlice parentSlice;
        public List<GraphSlice> childSlices = new List<GraphSlice>();
        public PointCloud pointCloud;
        public SlicerBox slicerBox;
        public GameObject image;


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
        private HistoImage histoImage;

        private void Start()
        {
            originalPos = Vector3.zero; //transform.localPosition;
            originalRot = transform.localRotation;
            originalSc = transform.localScale;
            slicerBox = GetComponentInChildren<SlicerBox>(true);
            pointCloud = GetComponent<PointCloud>();
            if (parentPC == null)
            {
                parentPC = pointCloud;
            }
            if (parentSlice == null)
            {
                parentSlice = this;
            }
            boxCollider = GetComponent<BoxCollider>();
            TryGetComponent(out HistoImage histoImage);
            CellexalEvents.GraphsColoredByGene.AddListener(UpdateColorTexture);
            CellexalEvents.GraphsReset.AddListener(UpdateColorTexture);
            CellexalEvents.RightTriggerClick.AddListener(ActivateBox);
            CellexalEvents.ColorTextureUpdated.AddListener(UpdateColorTexture);
        }


        private void ActivateBox()
        {
            if (SelectionToolCollider.instance.selActive) return;
            bool controllerInsideBox = CheckForController();
            if (slicerBox.gameObject.activeSelf && controllerInsideBox)
            {
                slicerBox.Active = false;
            }
            else if (controllerInsideBox)
            {
                slicerBox.Active = true;
            }
        }

        private bool CheckForController()
        {
            if (!boxCollider.enabled) return false;
            Collider[] colliders = Physics.OverlapBox(transform.TransformPoint(boxCollider.center), boxCollider.size / 2, transform.rotation, LayerMask.GetMask("Ignore Raycast")) ;
            if (colliders.Any(x => x.CompareTag("GameController")))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update the color texture map of the point cloud to correspond to the parents color map.
        /// The parent point cloud contains all the original points whereas this slice is a subset.
        /// </summary>
        public void UpdateColorTexture()
        {
            if (points.Count > 0 && histoImage == null)
            {
                Color[] carray = pointCloud.colorTextureMap.GetPixels();
                Color[] aarray = new Color[carray.Length];
                Texture2D parentTexture = parentPC.colorTextureMap;
                Texture2D parentATexture = parentPC.alphaTextureMap;
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

        /// <summary>
        /// Animation to move the slice back to its original position within the parent object.
        /// </summary>
        /// <returns></returns>
        public void MoveToGraph()
        {
            transform.parent = parentPC.transform;
            transform.DOLocalMove(Vector3.zero, 0.8f).SetEase(Ease.InOutSine);
            transform.DOLocalRotate(Vector3.zero, 0.8f).SetEase(Ease.InOutSine).OnComplete(() => gameObject.SetActive(false));
        }

        /// <summary>
        /// Animation to move the slice back to its original position within the parent object.
        /// </summary>
        /// <returns></returns>
        public IEnumerator MoveToGraphCoroutine()
        {
            transform.parent = parentPC.transform;
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
            GetComponent<BoxCollider>().enabled = false;
        }


        /// <summary>
        /// Places the slices in a grid pattern to be able to look at them all individually.
        /// </summary>
        /// <returns></returns>
        private void DisperseSlices()
        {
            float angle = (Mathf.PI * 1.1f);
            Vector3 center = Vector3.zero; // referenceManager.headset.transform.position;
            int slicesPerRow = childSlices.Count / 4;
            float yDiff = transform.position.y;
            float xPos;
            float yPos = (yDiff > 0f) ? -0.5f : -yDiff;
            float zPos;
            float radius = 4.0f;
            List<Vector3> slicePositions = new List<Vector3>();
            GraphSlice gs;
            Vector3 lookAtPos = new Vector3(0, 1.7f, 0);
            for (int i = 0; i < childSlices.Count; i++)
            {
                if (i % slicesPerRow == 0 && i > 0)
                {
                    angle = (Mathf.PI * 1.1f);
                    radius += 0.1f;
                    yPos += 1.0f;
                }

                xPos = center.x + (float)Mathf.Cos(angle) * radius;
                zPos = center.z + (float)Mathf.Sin(angle) * radius / 2f;
                Vector3 pos = new Vector3(xPos, yPos, zPos);
                slicePositions.Add(pos);
                angle += (Mathf.PI * 0.9f) / (float)slicesPerRow;
                gs = childSlices[i].GetComponent<GraphSlice>();
                gs.transform.DOLocalMove(pos, 0.8f).SetEase(Ease.InOutQuad);
                Vector3 wPos = transform.TransformPoint(pos);
                gs.transform.DODynamicLookAt(2 * wPos - lookAtPos, 0.8f);
            }
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

        /// <summary>
        /// Separates the slice from its parent point cloud so it can be individually interacted with.
        /// </summary>
        /// <param name="toggle">To separate or move back to parent.</param>
        public void ActivateSlices(bool toggle)
        {
            foreach (CellexalButton b in GetComponentsInChildren<CellexalButton>(true))
            {
                b.controllerInside = false;
            }
            gameObject.SetActive(!toggle);
            GetComponent<BoxCollider>().enabled = !toggle;
            foreach (GraphSlice gs in childSlices)
            {
                if (toggle)
                {
                    gs.gameObject.SetActive(true);
                    gs.ActivateSlice(toggle, true);
                }

                else
                {
                    if (gs.childSlices.Count > 0)
                    {
                        gs.ActivateSlices(false);
                    }
                    gs.slicerBox.Active = false;
                    gs.MoveToGraph();
                }
            }
            slicerBox.BoxAnimation(0, -1);

        }

        /// <summary>
        /// Activate/Deactivate a slice. Activating means the slice can be moved individually away from the parent object.
        /// When activating the slices are pulled apart slighly to make it easier to grab them.
        /// </summary>
        /// <param name="activate"></param>
        /// <returns></returns>
        public void ActivateSlice(bool activate, bool move = true)
        {
            foreach (BoxCollider bc in GetComponents<BoxCollider>())
            {
                bc.enabled = activate;
            }

            if (activate)
            {
                if (move)
                {
                    transform.DOLocalMove(sliceCoords, 0.8f).SetEase(Ease.InOutQuad);
                }
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
            parentPC = oldPc.GetComponent<PointCloud>();
            if (pointCloud == null)
            {
                pointCloud = GetComponent<PointCloud>();
            }
        }

        /// <summary>
        /// Creates a set of new slices from this based on the position in a given axis. 
        /// Also based on the nr of slices you want.
        /// E.g. if you want 10 new slices in the x it divides up the slice in 10 equally big (in x) slices.
        /// </summary>
        /// <param name="axis">Which axis to subdivide on. 0 = x, 1 = y, 2 = z.</param>
        /// <param name="points">Which points to slice.</param>
        /// <param name="nrOfSlices">Nr of slices to make. </param>
        /// <returns></returns>
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

            List<GraphSlice> slices = new List<GraphSlice>();
            int sliceNr = 0;
            PointCloud newPc = PointCloudGenerator.instance.CreateFromOld(pointCloud.transform);
            yield return null;
            GraphSlice slice = newPc.GetComponent<GraphSlice>();
            slice.transform.position = pointCloud.transform.position;
            slice.sliceCoords = pointCloud.transform.position;
            slice.SliceNr = ++sliceNr;
            slice.gameObject.name = "Slice" + sliceNr;
            slice.SliceNr = SliceNr;
            slices.Add(slice);
            childSlices.Add(slice);
            slice.gameObject.name = pointCloud.gameObject.name + "_" + SliceNr;
            float currentCoord, diff, prevCoord;
            Point point = sortedPoints[0];
            float firstCoord = prevCoord = point.offset[axis];
            float lastCoord = sortedPoints[sortedPoints.Count - 1].offset[axis];
            float epsilonToUse = math.abs(firstCoord - lastCoord) / (float)nrOfSlices;
            BoxCollider bc = newPc.GetComponent<BoxCollider>();
            Vector3 bcPos = bc.center;
            bcPos[axis] = firstCoord + epsilonToUse / 2;
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
                    yield return PointCloudGenerator.instance.SpawnPoints(newPc, pointCloud, slice.points);
                    newPc = PointCloudGenerator.instance.CreateFromOld(pointCloud.transform);

                    yield return new WaitForSeconds(0.1f);
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
                    yield return null;
                }

                else if (i == sortedPoints.Count - 1)
                {
                    slices.Add(slice);
                    yield return PointCloudGenerator.instance.SpawnPoints(newPc, pointCloud, slice.points);
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
            parentSlice.ActivateSlices(true);

        }
    }

}