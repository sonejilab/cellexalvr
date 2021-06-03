using UnityEngine;
using System.Collections;
using Valve.VR.InteractionSystem;

namespace CellexalVR.Interaction
{

    public class InteractableObjectOneAxis : InteractableObjectBasic
    {
        public enum MovableAxis
        {
            X = 0,
            Y = 1,
            Z = 2,
        }

        public MovableAxis movableAxis;
        public bool enableMaxMinVal;
        public float maxAxisValue;
        public float minAxisValue;
        public Vector3 startPosition;

        private Quaternion startRotation;
        private int[] stationaryAxes;

        protected override void Awake()
        {
            base.Awake();

        }

        private void Start()
        {
            startPosition = transform.localPosition;
            startRotation = transform.localRotation;
            if (movableAxis == MovableAxis.X)
            {
                stationaryAxes = new int[] { 1, 2 };
            }
            else if (movableAxis == MovableAxis.Y)
            {
                stationaryAxes = new int[] { 0, 2 };
            }
            else
            {
                stationaryAxes = new int[] { 0, 1 };
            }
            SavePreviousState(transform, true);
            InteractableObjectUnGrabbed += OnUnGrabbed;
            InteractableObjectGrabbed += OnGrabbed;
        }

        private void OnGrabbed(object sender, Hand hand)
        {
        }

        private void OnUnGrabbed(object sender, Hand hand)
        {
            Vector3 pos = transform.localPosition;
            pos[stationaryAxes[0]] = startPosition[stationaryAxes[0]];
            pos[stationaryAxes[1]] = startPosition[stationaryAxes[1]];
            transform.position = GetPreviousParent().TransformPoint(pos);
            transform.rotation = GetPreviousParent().rotation * startRotation;
        }

        protected override void HandAttachedUpdate(Hand hand)
        {
            base.HandAttachedUpdate(hand);
            float value = GetPreviousParent().InverseTransformPoint(transform.position)[(int)movableAxis];
            if (enableMaxMinVal)
            {
                if (value >= maxAxisValue)
                {
                    value = maxAxisValue;
                }

                else if (value < minAxisValue)
                {
                    value = minAxisValue;
                }
            }
            float ax1 = startPosition[stationaryAxes[0]];
            float ax2 = startPosition[stationaryAxes[1]];
            Vector3 finalPos = new Vector3();
            finalPos[stationaryAxes[0]] = ax1;
            finalPos[stationaryAxes[1]] = ax2;
            finalPos[(int)movableAxis] = value;
            transform.position = GetPreviousParent().TransformPoint(finalPos);
            transform.rotation = GetPreviousParent().rotation * startRotation;
        }
    }
}
