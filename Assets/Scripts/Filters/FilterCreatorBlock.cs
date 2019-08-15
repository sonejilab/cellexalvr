using CellexalVR.AnalysisLogic;
using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CellexalVR.Filters
{
    /// <summary>
    /// Abstract class that all filter creator blocks derive from. Filter creator blocks have children which are blocks whose outputs are connected to their parents' inputs.
    /// This creates a tree structure that ultimately defines a boolean expression.
    /// </summary>
    public abstract class FilterCreatorBlock : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public FilterManager filterManager;
        public bool isPrefab = true;

        private List<FilterCreatorBlockPort> ports;
        private GameObject filterBlockBoard;
        public virtual int HighlightedSection { get; set; }

        protected virtual void Start()
        {
            ports = new List<FilterCreatorBlockPort>();
            foreach (var port in GetComponentsInChildren<FilterCreatorBlockPort>())
            {
                ports.Add(port);
            }
            filterBlockBoard = referenceManager.filterBlockBoard;
            VRTK.VRTK_InteractableObject interactableObject = gameObject.GetComponent<VRTK.VRTK_InteractableObject>();
            interactableObject.InteractableObjectGrabbed += CreateDuplicateIfPrefab;
            interactableObject.InteractableObjectUngrabbed += MoveToBoard;
        }

        private void CreateDuplicateIfPrefab(object sender, VRTK.InteractableObjectEventArgs e)
        {
            if (isPrefab)
            {
                isPrefab = false;
                // create a new prefab to replace us
                GameObject newPrefab = Instantiate(gameObject, transform.position, transform.rotation, transform.parent);
                newPrefab.GetComponent<FilterCreatorBlock>().isPrefab = true;
                newPrefab.GetComponent<VRTK.VRTK_InteractableObject>().isGrabbable = true;
                transform.localScale = Vector3.one;
            }
        }

        private void MoveToBoard(object sender, VRTK.InteractableObjectEventArgs e)
        {
            StartCoroutine(MoveToBoardCoroutine());
        }

        private IEnumerator MoveToBoardCoroutine()
        {
            if (transform.parent != filterBlockBoard.transform)
            {
                transform.parent = filterBlockBoard.transform;
            }

            BoxCollider collider = filterBlockBoard.GetComponent<BoxCollider>();
            Vector3 startPos = filterBlockBoard.transform.InverseTransformPoint(transform.position);
            Vector3 targetPos = filterBlockBoard.transform.InverseTransformPoint(collider.ClosestPoint(transform.position));
            targetPos.z = 0.01f;

            Quaternion startRot = transform.localRotation;
            Quaternion targetRot = Quaternion.identity;

            float time = 1f;
            float t = 0f;
            while (t < time)
            {
                float progress = Mathf.SmoothStep(0, time, t);
                transform.localPosition = Vector3.Lerp(startPos, targetPos, progress);
                transform.localRotation = Quaternion.Lerp(startRot, targetRot, progress);
                t += (Time.deltaTime / time);
                yield return null;
            }
            transform.localPosition = targetPos;
            transform.localRotation = targetRot;

        }

        private void OnValidate()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            filterManager = referenceManager.filterManager;
        }

        public string ValidateValueString(string s)
        {
            //todo
            return "";
        }

        /// <summary>
        /// Returns a string representation of this block and all its children. If <see cref="IsValid"/> is true then this string is a valid filter.
        /// </summary>
        public abstract override string ToString();

        /// <summary>
        /// Checks if this block has only valid children 
        /// </summary>
        public abstract bool IsValid();

        /// <summary>
        /// Converts this block and all child blocks to a <see cref="BooleanExpression.Expr"/> that can be used in a <see cref="Filter"/>.
        /// </summary>
        public abstract BooleanExpression.Expr ToExpr();

        /// <summary>
        /// Unhighlights all ports on this block.
        /// </summary>
        public virtual void UnhighlightAllPorts()
        {
            foreach (var port in ports)
            {
                port.SetHighlighted(false);
            }
        }


    }
}