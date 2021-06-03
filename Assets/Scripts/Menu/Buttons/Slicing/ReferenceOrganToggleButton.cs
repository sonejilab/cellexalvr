using UnityEngine;
using System.Collections;
using CellexalVR.Spatial;
using CellexalVR.Interaction;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class ReferenceOrganToggleButton : SliderButton
    {
        protected override string Description => "Show/Hide reference organ mesh";

        private GameObject referenceOrganPrefab;
        private GameObject referenceOrgan;
        private GraphSlice graphSlice;

        protected override void Awake()
        {
            base.Awake();
            graphSlice = GetComponentInParent<GraphSlice>();
            referenceOrganPrefab = graphSlice.referenceOrgan;
        }

        protected override void ActionsAfterSliding()
        {
            referenceOrgan = MeshGenerator.instance.contourParent.gameObject;
            if (!referenceOrgan)
            {
                referenceOrgan = Instantiate(referenceOrganPrefab, graphSlice.transform);
                referenceOrgan.gameObject.name = "BrainParent";
            }
            referenceOrgan.transform.parent = currentState ? graphSlice.transform : null;
            referenceOrgan.transform.localPosition = Vector3.zero;
            referenceOrgan.transform.localScale = Vector3.one;
            referenceOrgan.transform.localRotation = Quaternion.identity;
            referenceOrgan.SetActive(currentState);
            referenceOrgan.GetComponent<InteractableObjectBasic>().isGrabbable = !currentState;
            referenceOrgan.GetComponent<BoxCollider>().enabled = !currentState;
        }

    }

}