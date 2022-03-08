using UnityEngine;
using System.Collections;
using CellexalVR.Spatial;
using CellexalVR.Interaction;
using CellexalVR.General;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class ReferenceOrganToggleButton : SliderButton
    {
        public GameObject movableContent;
        public GameObject menuToToggle;

        protected override string Description => "Show/Hide reference organ mesh";

        private GameObject referenceOrganPrefab;
        private GameObject referenceOrgan;
        private GraphSlice graphSlice;

        protected override void Awake()
        {
            base.Awake();
            graphSlice = GetComponentInParent<GraphSlice>();
            referenceOrganPrefab = ReferenceManager.instance.brainModel.gameObject;
            referenceOrgan = AllenReferenceBrain.instance.gameObject;
        }

        protected override void ActionsAfterSliding()
        {
            //referenceOrgan = MeshGenerator.instance.contourParent.gameObject;
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
            if (menuToToggle != null)
            {
                menuToToggle.SetActive(currentState);
            }
            //referenceOrgan.GetComponent<InteractableObjectBasic>().isGrabbable = !currentState;
            referenceOrgan.GetComponent<BoxCollider>().enabled = !currentState;
            referenceOrgan.GetComponent<AllenReferenceBrain>().ActivateKeyboard(true);


            StartCoroutine(MoveContent(currentState ? -1.8f : 0f));
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