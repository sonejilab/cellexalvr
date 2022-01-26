using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Interaction
{

    public class SliderController : MonoBehaviour
    {
        [SerializeField] private Transform sliderJoint;
        [SerializeField] private Transform sliderBar;
        [SerializeField] private Transform sliderProgressBar;
        [SerializeField] private TextMeshPro valueHeader;
        [SerializeField] private UnityEvent<float> onValueChanged;
        [SerializeField] private string valuePrefix;
        [SerializeField] private float minValue;
        [SerializeField] private float maxValue;

        private XRBaseInteractor interactor;
        private float startPosition;
        private bool shouldGetHandPosition;
        private XRSimpleInteractable interactable => GetComponent<XRSimpleInteractable>();
        private float currentValue;


        private void OnEnable()
        {
            interactable.selectEntered.AddListener(OnGrabbed);
            interactable.selectExited.AddListener(OnUnGrabbed);
        }

        private void OnGrabbed(SelectEnterEventArgs args)
        {
            interactor = args.interactor;
            shouldGetHandPosition = true;

        }

        private void OnUnGrabbed(SelectExitEventArgs args)
        {
            shouldGetHandPosition = false;
        }

        private void Update()
        {
            if (shouldGetHandPosition)
            {
                Vector3 handPosInSliderBarSpace = sliderBar.InverseTransformPoint(interactor.transform.position);
                Vector3 sliderJointPos = sliderJoint.transform.localPosition;
                float xVal = Mathf.Clamp(handPosInSliderBarSpace.x, -0.5f, 0.5f);
                sliderJointPos.x = xVal * sliderBar.transform.localScale.x;
                sliderJoint.transform.localPosition = sliderJointPos;
                valueHeader.text = $"{valuePrefix}: {Mathf.Round((xVal + 0.5f) * 100f)}%";
                currentValue = xVal + 0.5f;
                onValueChanged.Invoke(currentValue * (maxValue - minValue));

                sliderProgressBar.transform.localScale = new Vector3(currentValue, 1.1f, 1.1f);
                sliderProgressBar.transform.localPosition = new Vector3(-0.5f + (currentValue / 2f), 0, 0);
            }
        }

    }

}