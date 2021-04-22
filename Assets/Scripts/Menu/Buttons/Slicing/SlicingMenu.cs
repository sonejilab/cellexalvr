using System;
using System.Collections;
using CellexalVR.Spatial;
using Unity.Entities;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class SlicingMenu : MonoBehaviour
    {
        public GameObject automaticModeMenu;
        public GameObject manualModeMenu;
        public GameObject freeHandModeMenu;
        public ChangeSlicingModeToggleButton manualModeButton;
        public ChangeSlicingModeToggleButton autoModeButton;
        public ChangeNrOfSlicesButton addSliceButton;
        public ChangeNrOfSlicesButton subtractSliceButton;
        public int nrOfSlices;

        public enum SliceMode
        {
            None,
            Automatic,
            Manual,
            Freehand,
        }

        public enum SliceAxis
        {
            X = 0,
            Y = 1,
            Z = 2,
        }

        private SliceMode currentMode;
        public SliceAxis currentAxis;
        private SlicerBox slicerBox;
        private SliceGraphButton sliceGraphButton;

        private void Start()
        {
            slicerBox = GetComponentInParent<SlicerBox>();
            //ActivateMode(SliceMode.Automatic);
            sliceGraphButton = GetComponentInChildren<SliceGraphButton>();
            //sliceGraphSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SliceGraphSystem>();//GetComponentInParent<GraphSlicer>();
        }


        public void ToggleMode(SliceMode modeToActivate, bool toggle)
        {
            currentMode = toggle ? modeToActivate : currentMode;
            switch (modeToActivate)
            {
                case SliceMode.None:
                    automaticModeMenu.SetActive(false);
                    //sliceGraphButton.SetButtonActivated(false);
                    //manualModeMenu.SetActive(false);
                    break;
                case SliceMode.Automatic:
                    slicerBox.plane.SetActive(!toggle);
                    automaticModeMenu.SetActive(toggle);
                    //sliceGraphButton.SetButtonActivated(toggle);
                    manualModeButton.CurrentState = !toggle;
                    //manualModeMenu.SetActive(false);
                    break;
                case SliceMode.Manual:
                    slicerBox.plane.SetActive(toggle);
                    //manualModeMenu.SetActive(true);
                    automaticModeMenu.SetActive(!toggle);
                    //sliceGraphButton.SetButtonActivated(toggle);
                    autoModeButton.CurrentState = !toggle;
                    break;
                case SliceMode.Freehand:
                    //freeHandModeMenu.SetActive(true);
                    //automaticModeMenu.SetActive(false);
                    //manualModeMenu.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(modeToActivate), modeToActivate, null);
            }
        }

        public void ChangeAxis(SliceAxis axis)
        {
            currentAxis = axis;
            foreach (ChangeSliceAxisButton b in GetComponentsInChildren<ChangeSliceAxisButton>())
            {
                b.SetButtonActivated(true);
            }
        }

        public void ChangeNrOfSlices(int dir)
        {
            nrOfSlices += dir;
            if (nrOfSlices + dir < 2)
            {
                subtractSliceButton.SetButtonActivated(false);
            }

            else
            {
                subtractSliceButton.SetButtonActivated(true);
            }
        }

        public void SliceGraph()
        {
            switch (currentMode)
            {
                case SliceMode.Automatic:
                    //StartCoroutine(graphSlicer.SliceGraph(automatic: true, axis: graphSlicer.slicer.Axis, true));
                    slicerBox.SliceGraphAutomatic((int)currentAxis, nrOfSlices);
                    break;
                case SliceMode.Manual:
                    //StartCoroutine(graphSlicer.SliceGraph(false, 2, true));
                    slicerBox.SliceGraphManual();
                    break;
                case SliceMode.None:
                    break;
                case SliceMode.Freehand:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ChangeMeshTransparancy(float val)
        {
            MeshGenerator.instance.ChangeMeshTransparency(val);
        }

        public IEnumerator MinimizeMenu()
        {
            float time = 1f;
            float t = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = Vector3.zero;
            while (t < time)
            {
                float progress = Mathf.SmoothStep(0, time, t);
                //transform.localPosition = Vector3.Lerp(startPos, originalPos, progress);
                //transform.localRotation = Quaternion.Lerp(startRot, targetRot, progress);
                transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
                t += (Time.deltaTime / time);
                yield return null;
            }
        }

        public IEnumerator MaximizeMenu()
        {
            float time = 1f;
            float t = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = Vector3.one * 0.05f;
            while (t < time)
            {
                float progress = Mathf.SmoothStep(0, time, t);
                //transform.localPosition = Vector3.Lerp(startPos, originalPos, progress);
                //transform.localRotation = Quaternion.Lerp(startRot, targetRot, progress);
                transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
                t += (Time.deltaTime / time);
                yield return null;
            }
        }
    }

}
