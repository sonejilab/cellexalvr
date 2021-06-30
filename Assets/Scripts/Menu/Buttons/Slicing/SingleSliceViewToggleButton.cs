using UnityEngine;
using System.Collections;
using CellexalVR.Spatial;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class SingleSliceViewToggleButton : SliderButton
    {
        public GameObject menuToToggle;
        public GameObject movableContent;

        private SlicerBox slicerBox;

        protected override string Description => "Toggle single slice view mode";

        protected override void Awake()
        {
            base.Awake();
            slicerBox = GetComponentInParent<SlicerBox>();
        }

        protected override void ActionsAfterSliding()
        {
            slicerBox.SingleSliceViewMode(currentState ? 2 : -1);
            if (menuToToggle != null)
            {
                menuToToggle.SetActive(currentState);
            }

            StartCoroutine(MoveContent(currentState ? -3.2f : 0f));

        }

        private IEnumerator MoveContent(float zCoord)
        {
            float animationTime = 0.5f;
            float t = 0f;
            Vector3 startPos = movableContent.transform.localPosition;
            Vector3 targetPos = new Vector3(movableContent.transform.localPosition.x, movableContent.transform.localPosition.y, zCoord);
            while (t < animationTime)
            {
                //float progress = Mathf.SmoothStep(0, animationTime, t / animationTime);
                movableContent.transform.localPosition = Vector3.Lerp(startPos, targetPos, t / animationTime);
                t += (Time.deltaTime);
                yield return null;
            }
            yield return null;
        }

    }

}