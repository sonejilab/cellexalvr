using CellexalVR.AnalysisLogic;
using CellexalVR.Spatial;
using UnityEngine;
using DG.Tweening;
using AnalysisLogic;
using CellexalVR.Interaction;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class ToggleAttachGlassOrganButton : CellexalButton
    {
        protected override string Description => "Attach/Detach Glass Organ";

        private Transform pointCloudParent => GetComponentInParent<PointCloud>().transform;
        private bool detached;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void Click()
        {
            Transform t = AllenReferenceBrain.instance.transform;
            if (detached)
            {
                t.parent = pointCloudParent;
                t.transform.DOLocalMove(Vector3.one * -0.5f, 0.5f).SetEase(Ease.InOutSine);
                t.transform.DOLocalRotate(Vector3.zero, 0.5f).SetEase(Ease.InOutSine);
                
            }
            else
            {
                t.parent = null;
                t.transform.DOLocalMove(t.transform.localPosition + Vector3.one * 0.1f, 0.5f).SetEase(Ease.InOutSine);
            }
            detached = !detached;
            t.GetComponent<OffsetGrab>().enabled = detached;
        }
    }
}

