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
using DG.Tweening;
using UnityEngine.InputSystem;

namespace CellexalVR.Spatial
{
    public class SlicerBox : MonoBehaviour
    {
        public GameObject blade;
        public GameObject plane;
        public GameObject box;
        public GameObject sliceInteractable;
        public Transform menuPosition;
        public Transform buttonPosition;
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
                //Color c = boxMaterial.color;
                //c.a = active ? 0.5f : 0.05f;
                //boxMaterial.color = c;
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
        private CullingWall cwToLock;

        public Vector3 cullPos1 = new Vector3(.5f, .5f, .5f);
        public Vector3 cullPos2 = new Vector3(.5f, .5f, .5f);


        private void Start()
        {
            sliceGraphSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SliceGraphSystem>();
            pointCloud = GetComponentInParent<PointCloud>();
            graphSlice = GetComponentInParent<GraphSlice>();
            boxMaterial = box.GetComponent<MeshRenderer>().material;
            boxMaterial.SetMatrix("_BoxMatrix", transform.worldToLocalMatrix);
            //Color c = boxMaterial.color;
            //c.a = 0.05f;
            //boxMaterial.color = c;
            toggleSlicingMenuButton = GetComponentInChildren<ToggleSlicingMenuButton>(true);
            vfx = GetComponentInParent<VisualEffect>();
        }

        //private void OnValidate()
        //{
        //    Material mat = box.GetComponent<Renderer>().sharedMaterial;
        //    print($"validate {box == null}, {mat == null}");
        //    mat.SetMatrix("_BoxMatrix", transform.worldToLocalMatrix);
        //}

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
            //vfx.SetVector3("CullingCubePos", cullPos1);
            //vfx.SetVector3("CullingCube2Pos", cullPos2);

            if (Keyboard.current.nKey.wasPressedThisFrame)
            {
                SliceGraphManual();
            }
            if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                SliceGraphAutomatic(2, 20);
            }

        }

        private void BoxAnimation(int axis)
        {
            Material mat = box.GetComponent<Renderer>().material;
            mat.SetInt("_WaveToggle", 1);
            mat.SetInt("_WaveAxis", axis);
            DOVirtual.Float(-1.5f, 1.5f, 1.2f, v =>
            {
                mat.SetVector("_WaveCoords", new Vector3(v, v, v));
            }).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Restart);
        }

        public void SetHandlePositions()
        {
            foreach (CullingWall cw in cullingWalls)
            {
                cw.SetStartPosition();
            }
        }

        public void SingleSliceViewMode(int axis = -1)
        {
            //reset handle positions back when deactivate....
            // if axis == -1 then toggle off
            if (axis != -1)
            {
                cwToLock = cullingWalls[axis + 3];
                cwToLock.GetComponentInParent<SliderController>().enabled = false;
                cwToLock.handle.SetActive(false);
            }
            else
            {
                cwToLock.handle.SetActive(true);
                cwToLock.Reset();
            }
            singleSliceViewMode = axis;
            foreach (CullingWall cullingWall in cullingWalls)
            {
                if (cullingWall == cwToLock) continue;
                cwToLock.GetComponentInParent<SliderController>(true).enabled = true;
                cullingWall.handle.SetActive(true);
                cullingWall.Reset();
            }
            foreach (ChangeCullingAxisButton b in GetComponentsInChildren<ChangeCullingAxisButton>())
            {
                b.SetButtonActivated(true);
            }
        }

        public void SliceBySliceAnimation()
        {
            CullingWall cw = cullingWalls[singleSliceViewMode];
            float targetX = 1f - (1f * cw.transform.localPosition.x);
            cw.transform.DOLocalMoveX(targetX, 2f).SetEase(Ease.Linear);
        }

        public void SliceGraphManual()
        {
            //StartCoroutine(SliceAnimation());
            Material mat = blade.GetComponent<Renderer>().material;
            DOVirtual.Float(0f, 0.49f, 0.5f, v =>
            {
                mat.SetFloat("_SliceOffset", v);
            }).SetEase(Ease.InOutCubic).SetLoops(2, LoopType.Yoyo);
            sliceGraphSystem.Slice(pointCloud.pcID, plane.transform.forward, plane.transform.position);
        }

        public void SliceGraphAutomatic(int axis, int nrOfSlices)
        {
            //StartCoroutine(SliceAnimation());
            BoxAnimation(axis);
            StartCoroutine(graphSlice.SliceAxis(axis, sliceGraphSystem.GetPoints(pointCloud.pcID), nrOfSlices));
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