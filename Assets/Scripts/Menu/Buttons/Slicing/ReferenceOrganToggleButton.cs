using UnityEngine;
using System.Collections;
using CellexalVR.Spatial;

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
            if (!referenceOrgan)
            {
                referenceOrgan = Instantiate(referenceOrganPrefab, graphSlice.transform);
                referenceOrgan.gameObject.name = "BrainParent";
            }
            referenceOrgan.transform.localPosition = Vector3.zero;
            referenceOrgan.transform.localScale = Vector3.one;
            referenceOrgan.transform.localRotation = Quaternion.identity;
            referenceOrgan.SetActive(currentState);

        }

    }

}