﻿using CellexalVR.AnalysisLogic;
using CellexalVR.Extensions;
using CellexalVR.General;
using CellexalVR.SceneObjects;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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
        public GameObject detachArea;
        public GeneExpressionHistogram geneExpressionHistogram;
        [HideInInspector] public bool legendActive;
        [HideInInspector] public Legend desiredLegend;
        public Legend currentLegend;

        public enum Legend
        {
            None,
            AttributeLegend,
            GeneExpressionLegend,
            SelectionLegend
        }

        private Vector3 minPos = new Vector3(-0.58539f, -0.28538f, 0f);
        private Vector3 maxPos = new Vector3(-0.0146f, 0.2852f, 0f);
        private bool legendInsideCube;
        // Open XR
        //private VRTK.VRTK_InteractableObject interactableObject;
        private XRGrabInteractable interactableObject;
        private Transform cullingCubeTransform;
        private GameObject attachArea;
        private Transform originalParent;
        private Vector3 originalbackgroundScale;
        private bool attached;
        private ReferenceManager referenceManager;
        private Transform legendTransform;

        private void Start()
        {
            legendTransform = transform;
            //CellexalEvents.GraphsReset.AddListener(DeactivateLegend);
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            geneExpressionHistogram.referenceManager = referenceManager;
            interactableObject = GetComponent<XRGrabInteractable>();
            interactableObject.selectExited.AddListener(OnUngrabbed);
            originalParent = legendTransform.parent;
            originalbackgroundScale = backgroundPlane.transform.localScale;
            CellexalEvents.GraphsReset.AddListener(ClearLegends);
            CellexalEvents.GraphsUnloaded.AddListener(ClearLegends);
            Dataset.instance.legend = this;
        }

        private void Update()
        {
            if (interactableObject == null || !interactableObject.isSelected) return;
            referenceManager.multiuserMessageSender.SendMessageMoveLegend(legendTransform.position,
                legendTransform.rotation,
                legendTransform.localScale);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name != "AttachOnSide") return;
            legendInsideCube = true;
            cullingCubeTransform = other.transform;
            attachArea = cullingCubeTransform.parent.GetComponent<CullingCube>().attachOnSideArea;
            attachArea.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name != "AttachOnSide") return;
            legendInsideCube = false;
            attachArea = cullingCubeTransform.parent.GetComponent<CullingCube>().attachOnSideArea;
            attachArea.SetActive(false);
        }

        private void OnUngrabbed(SelectExitEventArgs e)
        {
            Rigidbody rigidBody = this.GetComponent<Rigidbody>();
            referenceManager.multiuserMessageSender.SendMessageLegendUngrabbed(transform.position, transform.rotation, rigidBody.velocity, rigidBody.angularVelocity);
            if (legendInsideCube)
            {
                StartCoroutine(AttachLegendToCube(cullingCubeTransform));
            }
        }

        /// <summary>
        /// When the legend is hold by the side of the culling cube it attaches the legend to the cube and adds the extra columns for filtering.
        /// </summary>
        /// <param name="cubeTransform"></param>
        /// <returns></returns>
        private IEnumerator AttachLegendToCube(Transform cubeTransform)
        {
            attachArea.SetActive(false);
            print(message: attachArea.activeSelf);
            transform.parent = cubeTransform.parent;
            Vector3 startPos = transform.localPosition;
            Vector3 targetPos = new Vector3(-1f, 0.012f, 0.497f);
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = Vector3.one * 1.7f;
            Quaternion startRot = transform.localRotation;
            Quaternion targetRot = Quaternion.Euler(0, 180, 0);
            float time = 1f;
            float t = 0f;
            while (t < time)
            {
                float progress = Mathf.SmoothStep(0, time, t);
                this.transform.localPosition = Vector3.Lerp(startPos, targetPos, progress);
                this.transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
                this.transform.localRotation = Quaternion.Lerp(startRot, targetRot, progress);
                t += (Time.deltaTime / time);
                yield return null;
            }

            interactableObject.enabled = false;
            Destroy(GetComponent<Rigidbody>());
            detachArea.SetActive(true);
            Vector3 pos = backgroundPlane.transform.localPosition;
            pos.x = 0.03f;
            backgroundPlane.transform.localPosition = pos;
            Vector3 scale = backgroundPlane.transform.localScale;
            scale.x = 0.064f;
            backgroundPlane.transform.localScale = scale;
            attached = true;
            CellexalEvents.LegendAttached.Invoke();
        }

        /// <summary>
        /// Detach by clicking the detach button. Goes back to normal legend state.
        /// </summary>
        public void DetachLegendFromCube()
        {
            transform.parent = originalParent;
            transform.Translate(Vector3.forward * -0.05f);
            transform.Translate(Vector3.right * 0.05f);
            //transform.localScale = originalbackgroundScale;
            var rigidbody = GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = gameObject.AddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.isKinematic = false;
                rigidbody.drag = 10;
                rigidbody.angularDrag = 15;
                interactableObject.enabled = true;
            }

            detachArea.SetActive(false);
            legendInsideCube = false;
            Vector3 pos = backgroundPlane.transform.localPosition;
            pos.x = 0;
            backgroundPlane.transform.localPosition = pos;
            backgroundPlane.transform.localScale = originalbackgroundScale;
            CellexalEvents.LegendDetached.Invoke();
            attached = false;
        }

        /// <summary>
        /// Deactivates all legends. Sets the gameobject to inactive.
        /// </summary>
        public void DeactivateLegend()
        {
            legendInsideCube = false;
            backgroundPlane.SetActive(false);
            gameObject.GetComponent<Collider>().enabled = false;
            legendActive = false;
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Activates the desired legend, as determined by <see cref="desiredLegend"/>.
        /// </summary>
        public void ActivateLegend()
        {
            ActivateLegend(desiredLegend);
            var cameraTransform = referenceManager.headset.transform;
            transform.position = cameraTransform.position + cameraTransform.forward * 0.7f;
            transform.LookAt(cameraTransform);
            transform.Rotate(0f, 180f, 0f);
        }

        /// <summary>
        /// Activates a legend. Does not set the gameobject active.
        /// </summary>
        /// <param name="legendToActivate">The legend to activate</param>
        public void ActivateLegend(Legend legendToActivate)
        {
            DeactivateLegend();
            backgroundPlane.SetActive(true);
            gameObject.GetComponent<Collider>().enabled = true;
            legendActive = true;
            if (attached)
            {
                detachArea.SetActive(true);
                CellexalEvents.LegendAttached.Invoke();
            }

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

            desiredLegend = legendToActivate;
            currentLegend = desiredLegend;
        }

        public void ClearLegends()
        {
            geneExpressionHistogram.ClearLegend();
            attributeLegend.ClearLegend();
            selectionLegend.ClearLegend();
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
            localPos.z = 0f;
            return (localPos - geneExpressionHistogram.HistogramMinPos).InverseScale(
                geneExpressionHistogram.HistogramMaxPos - geneExpressionHistogram.HistogramMinPos);
        }
    }
}