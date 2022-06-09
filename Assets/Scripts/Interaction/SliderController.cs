using CellexalVR.General;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Interaction
{

    public class SliderController : MonoBehaviour
    {
        [SerializeField] private Transform sliderJoint;
        [SerializeField] private Transform track;
        [SerializeField] private Transform sliderProgressBar;
        [SerializeField] private TextMeshPro valueHeader;
        [SerializeField] private UnityEvent<float> onValueChanged;
        [SerializeField] private UnityEvent<float> onReleased;
        [SerializeField] private string valuePrefix;
        [SerializeField] private float minValue;
        [SerializeField] private float maxValue;
        [SerializeField] private float startFactor;
        [SerializeField] private float startValue;
        [SerializeField] private float minPos;
        [SerializeField] private float maxPos;
        [SerializeField] private bool useTriggerToMove;
        private enum updateType
        {
            ABSOLUTE, RELATIVE
        }
        [SerializeField] private updateType type;

        public enum sliderType
        {
            VELOCITY, PARTICLEALPHA, PARTICLESPREAD, PARTICLESIZE
        }
        [SerializeField] private sliderType slideType;

        private Transform interactor;
        private float startPosition;
        private bool shouldGetHandPosition;
        private XRSimpleInteractable interactable => GetComponent<XRSimpleInteractable>();
        private float currentFactor;
        public float currentValue;

        private void Awake()
        {
            currentFactor = startFactor;
            currentValue = startValue;
            Vector3 sliderJointPos = sliderJoint.transform.localPosition;
            sliderJointPos.x = (currentFactor - minValue - 0.5f) * track.transform.localScale.x;
            sliderJoint.transform.localPosition = sliderJointPos;

            float barScale = type == updateType.ABSOLUTE ? currentFactor : currentFactor - 0.5f;
            if (sliderProgressBar)
            {
                sliderProgressBar.transform.localScale = new Vector3(barScale, 1.1f, 1.1f);
                sliderProgressBar.transform.localPosition = new Vector3(-0.5f + (barScale / 2f), 0, 0);
            }
            if (valueHeader)
                valueHeader.text = $"{valuePrefix}: {Mathf.Round((currentFactor) * 100f)}%";


        }


        private void OnEnable()
        {
            if (useTriggerToMove)
            {
                CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
                CellexalEvents.RightTriggerUp.AddListener(Release);
            }
            else
            {
                interactable.selectEntered.AddListener(OnGrabbed);
                interactable.selectExited.AddListener(OnUngrabbed);
            }
        }

        private void OnGrabbed(SelectEnterEventArgs args)
        {
            interactor = ReferenceManager.instance.rightController.transform;
            shouldGetHandPosition = true;
        }

        private void OnUngrabbed(SelectExitEventArgs args)
        {
            Release();
        }

        private void OnTriggerClick()
        {
            interactor = ReferenceManager.instance.rightController.transform;
            Physics.Raycast(interactor.transform.position, interactor.transform.forward, out RaycastHit hit);
            if (hit.collider == null)
                return;
            if (hit.collider.transform == transform)
                shouldGetHandPosition = true;
        }

        private void Release()
        {
            shouldGetHandPosition = false;

            if (type == updateType.RELATIVE)
            {
                currentValue = currentFactor * currentValue;
                Vector3 sliderJointPos = sliderJoint.transform.localPosition;
                sliderJointPos.x = 0f;
                sliderJoint.transform.localPosition = sliderJointPos;

                valueHeader.text = $"{valuePrefix}: 100%";
                sliderProgressBar.transform.localScale = new Vector3(0.5f, 1.1f, 1.1f);
                sliderProgressBar.transform.localPosition = new Vector3(-0.25f, 0, 0);
            }
            else
            {
                currentValue = currentFactor * (maxValue - minValue);
            }

            onReleased?.Invoke(currentValue);
            ReferenceManager.instance.multiuserMessageSender.SendMessageUpdateSliderValue(slideType, currentValue);
        }

        private void Update()
        {
            if (shouldGetHandPosition)
            {
                Vector3 handPosInSliderBarSpace = track.InverseTransformPoint(interactor.position);
                Vector3 sliderJointPos = sliderJoint.transform.localPosition;
                float xVal = Mathf.Clamp(handPosInSliderBarSpace.x, minPos, maxPos);
                sliderJointPos.x = xVal * track.transform.localScale.x;
                sliderJoint.transform.localPosition = sliderJointPos;
                currentFactor = xVal + 0.5f;
                if (type == updateType.ABSOLUTE)
                {
                    currentValue = currentFactor * (maxValue - minValue);
                }
                else
                {
                    currentFactor += 0.5f;
                }

                UpdateTextAndBar();
                onValueChanged?.Invoke(currentValue);


            }
        }

        private void UpdateTextAndBar()
        {
            if (valueHeader != null)
            {
                valueHeader.text = $"{valuePrefix}: {Mathf.Round((currentFactor) * 100f)}%";
            }
            if (sliderProgressBar != null)
            {
                float barScale = type == updateType.ABSOLUTE ? currentFactor : currentFactor - 0.5f;
                sliderProgressBar.transform.localScale = new Vector3(barScale, 1.1f, 1.1f);
                sliderProgressBar.transform.localPosition = new Vector3(-0.5f + (barScale / 2f), 0, 0);
            }
        }

        public void UpdateSliderValue(float value)
        {
            currentValue = value;
            UpdateTextAndBar();
        }
    }

}