using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DG.Tweening;
using UnityEngine.XR.Interaction.Toolkit;
using CellexalVR.General;

namespace CellexalVR.Spatial
{

    public abstract class GeoMXSlide : MonoBehaviour
    {
        public string displayName;
        [HideInInspector] public GeoMXImageHandler imageHandler;
        [HideInInspector] public Vector3 originalScale;
        [HideInInspector] public int type;
        [HideInInspector] public bool detached;
        public int index;

        [SerializeField] private GameObject reattachButton;
        [SerializeField] protected GameObject highlight;
        private XRGrabInteractable interactable;
        private float yPos;


        protected virtual void Start()
        {
            interactable = GetComponent<XRGrabInteractable>();
            interactable.selectEntered.AddListener(OnSelectEntered);
            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
        }

        private void OnTriggerClick()
        {
            if (interactable.isSelected)
            {
                Reattach();
            }
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            Detach();
        }

        public virtual void ShowName()
        {
            imageHandler.ShowName(displayName);
            imageHandler.SetScroller(type);
        }

        public abstract void Select();
        public abstract void SelectCells(int group);
        public virtual void OnRaycastHit() { }

        public virtual void Highlight()
        {
            highlight.SetActive(true);
        }

        public virtual void UnHighlight()
        {
            highlight.SetActive(false);
        }

        public virtual void Move(Vector3 toPos)
        {
            transform.DOLocalMove(toPos, .5f).SetEase(Ease.InOutSine);
            Vector3 wPos = transform.parent.TransformPoint(toPos);
            transform.DODynamicLookAt(2 * wPos - imageHandler.center, .5f).SetEase(Ease.InOutSine);
        }

        public virtual void Fade(bool toggle)
        {
            Vector3 targetScale;
            if (toggle)
            {
                targetScale = originalScale;
                transform.DOScale(targetScale, .5f).SetEase(Ease.InOutSine);
            }
            else
            {
                targetScale = Vector3.zero;
                transform.DOScale(targetScale, .5f).SetEase(Ease.InOutSine).OnComplete(() => gameObject.SetActive(false));
            }
        }

        public virtual void Detach()
        {
            detached = true;
            yPos = transform.localPosition.y;
            reattachButton.SetActive(true);
        }

        public virtual void Reattach()
        {
            detached = false;
            transform.parent = imageHandler.transform;
            reattachButton.SetActive(false);
            Vector3 targetPos;
            int length = 0;
            switch (type)
            {
                case 0:
                    length = imageHandler.slideScroller.currentScanIDs.Length;
                    break;
                case 1:
                    length = imageHandler.slideScroller.currentROIIDs.Length;
                    break;
                case 2:
                    length = imageHandler.slideScroller.currentAOIIDs.Length;
                    break;
            }
            int i = SlideScroller.mod(index - imageHandler.slideScroller.currentSlide[type], length);
            float yTargetPos = 1.1f;
            if (imageHandler.selectedScan != null && type == 0)
            {
                yTargetPos += 1.1f;
            }
            if (imageHandler.selectedROI != null && type != 2)
            {
                yTargetPos += 1.1f;
            }
            if (i >= imageHandler.nrOfPositions)
            {
                targetPos = new Vector3(imageHandler.inactivePosRight.x, yTargetPos, imageHandler.inactivePosRight.z);
                Fade(false);
            }
            else if (i < 0)
            {
                targetPos = new Vector3(imageHandler.inactivePosLeft.x, yTargetPos, imageHandler.inactivePosLeft.z);
                Fade(false);
            }
            else
            {
                targetPos = new Vector3(imageHandler.sliceCirclePositions[i].x, yTargetPos, imageHandler.sliceCirclePositions[i].z);
            }
            Move(targetPos);
        }


    }
}

