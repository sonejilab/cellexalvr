using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.Interaction;
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
        public GameObject cullingCube;
        public GameObject cullingCube2;
        public CullingWall[] cullingWalls = new CullingWall[6];
        public GameObject cullingWallsParent;

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
                cullingWallsParent.SetActive(active);
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
        private VisualEffect vfx;
        private int singleSliceViewMode = -1;

        public Vector3 cullPos1 = new Vector3(.5f, .5f, .5f);
        public Vector3 cullPos2 = new Vector3(.5f, .5f, .5f);


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
            vfx = GetComponentInParent<VisualEffect>();
        }

        private void Update()
        {
            cullPos1.x = Math.Min(0.6f, transform.InverseTransformPoint(cullingWalls[0].transform.position).x);
            cullPos1.y = Math.Min(0.6f, transform.InverseTransformPoint(cullingWalls[1].transform.position).y);
            cullPos1.z = Math.Min(0.6f, transform.InverseTransformPoint(cullingWalls[2].transform.position).z);

            if (singleSliceViewMode == 0)
            {
                cullingWalls[3].transform.position = cullingWalls[0].transform.position;
                cullingWalls[3].transform.position += new Vector3(-0.1f, 0, 0);
            }
            else if (singleSliceViewMode == 1)
            {
                cullingWalls[4].transform.position = cullingWalls[1].transform.position;
                cullingWalls[4].transform.position += new Vector3(0, -0.1f, 0);
            }
            else if (singleSliceViewMode == 2)
            {
                cullingWalls[5].transform.position = cullingWalls[2].transform.position;
                cullingWalls[5].transform.position += new Vector3(0, 0, -0.1f);
            }
            cullPos2.x = (Math.Min(0.6f, transform.InverseTransformPoint(cullingWalls[3].transform.position).x)) + 1f;
            cullPos2.y = (Math.Min(0.6f, transform.InverseTransformPoint(cullingWalls[4].transform.position).y)) + 1f;
            cullPos2.z = (Math.Min(0.6f, transform.InverseTransformPoint(cullingWalls[5].transform.position).z)) + 1f;
            vfx.SetVector3("CullingCubePos", cullPos1);
            vfx.SetVector3("CullingCube2Pos", cullPos2);
        }

        public void SetHandlePositions()
        {
            foreach (CullingWall cw in cullingWalls)
            {
                cw.SetStartPosition();
            }
        }

        public void SingleSliceViewMode(bool toggle, int axis)
        {
            //reset handle positions back when deactivate....
            singleSliceViewMode = toggle ? axis : -1;
            CullingWall cwToLock = cullingWalls[axis + 3];
            cwToLock.GetComponent<InteractableObjectBasic>().enabled = !toggle;
            cwToLock.handle.SetActive(false);
            foreach (CullingWall cullingWall in cullingWalls)
            {
                if (cullingWall == cwToLock) continue;
                InteractableObjectOneAxis interactable = cullingWall.GetComponent<InteractableObjectOneAxis>();
                cullingWall.transform.localPosition = interactable.startPosition;
                interactable.enabled = true;
                cullingWall.handle.SetActive(true);
            }
            foreach (ChangeCullingAxisButton b in GetComponentsInChildren<ChangeCullingAxisButton>())
            {
                b.SetButtonActivated(true);
            }
        }

        public void SliceBySliceAnimation()
        {
            StartCoroutine(SliceBySliceAnimationCoroutine());
        }

        private IEnumerator SliceBySliceAnimationCoroutine()
        {
            CullingWall cw = cullingWalls[singleSliceViewMode];
            float t = 0f;
            float animationTime = 2f;
            Vector3 startPos = cw.transform.localPosition;
            Vector3 targetPos = cw.transform.localPosition;
            targetPos[singleSliceViewMode] -= 1f * Math.Sign(targetPos[singleSliceViewMode]);

            while (t < animationTime)
            {
                //float progress = Mathf.SmoothStep(0, animationTime, t / animationTime);
                cw.transform.localPosition = Vector3.Lerp(startPos, targetPos, t / animationTime);
                t += (Time.deltaTime);
                yield return null;
            }

        }

        public void SliceGraphManual()
        {
            StartCoroutine(SliceAnimation());
            sliceGraphSystem.Slice(pointCloud.pcID, plane.transform.forward, plane.transform.position);
        }

        public void SliceGraphAutomatic(int axis, int nrOfSlices)
        {
            StartCoroutine(SliceAnimation());
            StartCoroutine(graphSlice.SliceAxis(axis, sliceGraphSystem.GetPoints(pointCloud.pcID), nrOfSlices));
            //graphSlice.SliceAxis(axis, sliceGraphSystem.GetPoints(pointCloud.pcID), nrOfSlices);
        }

        public void MorphGraph()
        {
            pointCloud.Morph();
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
            float animationTime = 2.0f;

            while (t < animationTime || sliceAnimationActive)
            {
                t += Time.deltaTime;
                yield return null;
            }

            slicingAnimation.enabled = false;
            Active = false;
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