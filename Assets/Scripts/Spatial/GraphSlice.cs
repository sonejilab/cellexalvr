using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.SceneObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// Represents one graph with the same z - coordinate (one slice of the spatial graph).
    /// Each slice can be moved independently if in slice mode otherwise they should be moved together as one object.
    /// </summary>
    public class GraphSlice : MonoBehaviour
    {

        public ReferenceManager referenceManager;
        public bool sliceMode;
        public GameObject replacement;
        public GameObject wire;
        public int sliceNr;
        public float zCoord;


        protected Graph graph;

        private Vector3 originalPos;
        private Vector3 originalSc;
        private Quaternion originalRot;
        private SpatialGraph spatGraph;
        private VRTK.VRTK_InteractableObject interactableObject;
        private GameObject wirePrefab;
        private GameObject replacementPrefab;
        private Color replacementCol;
        private Color replacementHighlightCol;
        private bool grabbing;


        private void Start()
        {
            graph = gameObject.GetComponent<Graph>();
            spatGraph = transform.parent.gameObject.GetComponent<SpatialGraph>();
            interactableObject = gameObject.GetComponent<VRTK.VRTK_InteractableObject>();
            interactableObject.InteractableObjectGrabbed += OnGrabbed;
            interactableObject.InteractableObjectUngrabbed += OnUngrabbed;
            originalPos = transform.localPosition;
            originalRot = transform.localRotation;
            GetComponent<Rigidbody>().drag = Mathf.Infinity;
            GetComponent<Rigidbody>().angularDrag = Mathf.Infinity;
        }

        private void Update()
        {
            // When slicemode is false the slices are to be kept in their original position.
            // Since each slice has its own rigidbody and I dont want the parent to have a rigidbody it is synched here below instead.
            if (!sliceMode && grabbing)
            {
                Vector3 tempPos = this.transform.position;
                Quaternion tempRot = this.transform.rotation;
                this.transform.localPosition = originalPos;
                this.transform.localRotation = originalRot;
                this.transform.parent.position = tempPos;
                this.transform.parent.rotation = tempRot;
            }
        }

        private void OnGrabbed(object sender, VRTK.InteractableObjectEventArgs e)
        {
            //spatGraph.transform.SetParent(this.transform);
            //spatGraph.SynchMovement(transform);
            if (sliceMode)
            {
                // Show wire and replacement
                //replacement.GetComponent<Renderer>().material.color = replacementHighlightCol;
                //replacement.SetActive(true);
                //wire.SetActive(true);
                //var follow = wire.GetComponent<LineRendererFollowTransforms>();
                //follow.transform1 = e.interactingObject.transform;
                //follow.transform2 = replacement.transform;
            }
            else
            {
                grabbing = true;
            }
        }

        private void OnUngrabbed(object sender, VRTK.InteractableObjectEventArgs e)
        {
            if (!sliceMode)
            {
                transform.localPosition = originalPos;
                transform.localRotation = originalRot;
                grabbing = false;
            }
            else
            {
                // Hide wire and replacement
                //wire.SetActive(false);
                //replacement.SetActive(false);
                //replacement.GetComponent<Renderer>().material.color = replacementCol;
            }
            //if (!sliceMode)
            //{
            //    StartCoroutine(SynchSlices());
            //}
            //transform.SetParent(spatGraph.transform);
            //StartCoroutine(MoveToBoardCoroutine());
        }

        /// <summary>
        /// Animation to move the slice back to its original position within the parent object.
        /// </summary>
        /// <returns></returns>
        public IEnumerator MoveToGraphCoroutine()
        {
            this.transform.parent = spatGraph.transform;
            BoxCollider collider = graph.GetComponent<BoxCollider>();
            Vector3 startPos = this.transform.localPosition;
            Quaternion startRot = this.transform.localRotation;
            Quaternion targetRot = Quaternion.identity;

            float time = 1f;
            float t = 0f;
            while (t < time)
            {
                float progress = Mathf.SmoothStep(0, time, t);
                this.transform.localPosition = Vector3.Lerp(startPos, originalPos, progress);
                this.transform.localRotation = Quaternion.Lerp(startRot, targetRot, progress);
                t += (Time.deltaTime / time);
                yield return null;
            }
            this.transform.localPosition = originalPos;
            this.transform.localRotation = originalRot;
            //wire.SetActive(false);
            //replacement.GetComponent<Renderer>().material.color = replacementCol;
            //replacement.SetActive(false);

        }



        /// <summary>
        /// Add replacement prefab instance. A replacement is spawned when slices is removed from parent to show where it came from.
        /// </summary>
        public void AddReplacement()
        {
            wirePrefab = spatGraph.wirePrefab;
            replacementPrefab = spatGraph.replacementPrefab;
            replacement = Instantiate(replacementPrefab, transform.parent);
            Vector3 maxCoords = graph.ScaleCoordinates(graph.maxCoordValues);
            replacement.transform.localPosition = new Vector3(0, maxCoords.y + 0.2f, zCoord);
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
        /// Activate/Deactivate a slice. Activating means the slice can be moved individually away from the parent object.
        /// When activating the slices are pulled apart slighly to make it easier to grab them.
        /// </summary>
        /// <param name="activate"></param>
        /// <returns></returns>
        public IEnumerator ActivateSlice(bool activate)
        {
            if (activate)
            {
                sliceMode = true;
                Vector3 startPos = this.transform.localPosition;
                Vector3 targetPos = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, zCoord);
                float time = 1f;
                float t = 0f;
                while (t < time)
                {
                    float progress = Mathf.SmoothStep(0, time, t);
                    this.transform.localPosition = Vector3.Lerp(startPos, targetPos, progress);
                    t += (Time.deltaTime / time);
                    yield return null;
                }
            }
            else
            {
                sliceMode = false;
                //graph.transform.localPosition = Vector3.zero;
            }
        }

    }
}
