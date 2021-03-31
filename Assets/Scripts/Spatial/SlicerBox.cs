using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.Menu.Buttons;
using CellexalVR.Menu.Buttons.Slicing;
using DefaultNamespace;
using Unity.Entities;
using UnityEngine;
using UnityEngine.VFX;
using Valve.VR;

namespace CellexalVR.Spatial
{
    public class SlicerBox : MonoBehaviour
    {
        // public Transform t1, t2, t3, t4, t5, t6;
        // public Transform[] currentTransforms = new Transform[2];
        // public int axis;
        public GameObject blade;
        public GameObject plane;
        public GameObject box;
        public Transform menuPosition;
        public Transform buttonPosition;
        //public GraphSlicer graphSlicer;
        public SlicingMenu slicingMenu;
        public VisualEffect slicingAnimation;
        [HideInInspector] public bool sliceAnimationActive;

        private bool active;
        public bool Active
        {
            get
            {
                return active;
            }

            set
            {
                active = value;
                Color c = boxMaterial.color;
                c.a = active ? 0.5f : 0.05f;
                boxMaterial.color = c;
                slicingMenu.gameObject.SetActive(active);
                slicingMenu.transform.localPosition = transform.InverseTransformPoint(menuPosition.position);
                toggleSlicingMenuButton.transform.localPosition = transform.InverseTransformPoint(buttonPosition.position);
                toggleSlicingMenuButton.gameObject.SetActive(active);
            }
        }

        public int Axis { get; set; }

        public bool Automatic { get; set; }

        private int axis;
        private LineRenderer lr;
        private Graph graph;
        private CellexalButton sliceGraphButton;
        private SliceGraphSystem sliceGraphSystem;
        private GraphSlice graphSlice;
        private PointCloud pointCloud;
        private Material boxMaterial;
        private ToggleSlicingMenuButton toggleSlicingMenuButton;

        private void Start()
        {
            sliceGraphSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SliceGraphSystem>();
            pointCloud = GetComponentInParent<PointCloud>();
            graphSlice = GetComponentInParent<GraphSlice>();
            boxMaterial = box.GetComponent<MeshRenderer>().material;
            Color c = boxMaterial.color;
            c.a = 0.05f;
            boxMaterial.color = c;
            toggleSlicingMenuButton = GetComponentInChildren<ToggleSlicingMenuButton>(true);
            // graph = GetComponentInParent<Graph>();
            // lr = GetComponentInChildren<LineRenderer>();
            // lr.useWorldSpace = false;
            // lr.SetPositions(new Vector3[] {t1.localPosition, t2.localPosition});
            //
            // switch (axis)
            // {
            //     case 0:
            //         currentTransforms[0] = t1;
            //         currentTransforms[1] = t2;
            //         lr.transform.localRotation = Quaternion.identity;
            //         break;
            //     case 1:
            //         currentTransforms[0] = t3;
            //         currentTransforms[1] = t4;
            //         lr.transform.localRotation = Quaternion.Euler(90, 0, 0);
            //         break;
            //     case 2:
            //         currentTransforms[0] = t5;
            //         currentTransforms[1] = t6;
            //         // lr.transform.localRotation = Quaternion.Euler(0, 0, 180);
            //         break;
            // }
        }

        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.K))
        //    {
        //        SliceGraphAutomatic(1, 10);
        //    }
        //}


        public void SliceGraphManual()
        {
            StartCoroutine(SliceAnimation());
            sliceGraphSystem.Slice(pointCloud.pcID, plane.transform.forward, plane.transform.position);
        }

        public void SliceGraphAutomatic(int axis, int nrOfSlices)
        {
            StartCoroutine(SliceAnimation());
            StartCoroutine(graphSlice.SliceAxis(axis, sliceGraphSystem.GetPoints(pointCloud.pcID), nrOfSlices));
        }

        public void MorphGraph()
        {
            StartCoroutine(pointCloud.Morph());
        }

        public void ToggleManualSlicer(bool toggle)
        {
            Automatic = !toggle;
            plane.SetActive(toggle);
            Axis = 2;
        }

        public void ChangeAxis(int axis)
        {
            Axis = axis;
            if (axis == -1)
            {
                sliceGraphButton.SetButtonActivated(false);
            }
            else
            {
                foreach (SliceAxisToggleButton sb in GetComponentsInChildren<SliceAxisToggleButton>())
                {
                    if (sb.axis == axis) return;
                    sb.CurrentState = false;
                }

                sliceGraphButton.SetButtonActivated(true);
            }
        }

        public IEnumerator SliceAnimation()
        {
            plane.SetActive(false);
            var pc = GetComponentInParent<PointCloud>();
            slicingAnimation.enabled = true;
            sliceAnimationActive = true;
            float t = 0f;
            float animationTime = 3.0f;

            while (t < animationTime || PointCloudGenerator.instance.creatingGraph)
            {
                t += Time.deltaTime;
                yield return null;
            }

            slicingAnimation.enabled = false;
            gameObject.SetActive(false);
            pc.GetComponent<VisualEffect>().enabled = false;
            if (gameObject.activeSelf)
            {
                pc.gameObject.SetActive(false);
            }
            sliceAnimationActive = false;
            graphSlice.ActivateSlices(true);
        }

        public Plane GetPlane()
        {
            return new Plane(plane.transform.forward, plane.transform.position);
        }

    }
}