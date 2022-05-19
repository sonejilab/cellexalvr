using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.Interaction;
using CellexalVR.Menu.Buttons;
using CellexalVR.Menu.Buttons.Slicing;
using CellexalVR.AnalysisLogic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.VFX;
using DG.Tweening;
using UnityEngine.InputSystem;
using CellexalVR.General;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// This class handles the box used for spatial graphs.
    /// Handles culling, slicing and reference mesh and mesh generation.
    /// As of now only used on spatial graphs but could be applicable to other data sets as well.
    /// </summary>
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
        private Tweener boxTween;
        public bool Active
        {
            get
            {
                return active;
            }

            set
            {
                active = value;
                slicingMenu.gameObject.SetActive(active);
                slicingMenu.transform.localPosition = transform.InverseTransformPoint(menuPosition.position);
                toggleSlicingMenuButton.transform.localPosition = transform.InverseTransformPoint(buttonPosition.position);
                vfx.SetVector3("CullingCubeSize", Vector3.one * (active ? 1.02f : 50f));
                toggleSlicingMenuButton.gameObject.SetActive(active);
                cullingWallsParent.SetActive(active);
                gameObject.SetActive(active);
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
            toggleSlicingMenuButton = GetComponentInChildren<ToggleSlicingMenuButton>(true);
            vfx = GetComponentInParent<VisualEffect>();
            Active = false;
            cullingWalls.All(x => x.transform.hasChanged = false);
        }

        private void Update()
        {
            if (cullingWalls.Any(x => x.transform.hasChanged))
            {
                UpdateCullingBox();
                cullingWalls.All(x => x.transform.hasChanged = false);
            }
        }

        /// <summary>
        /// Using the position of the culling sliders it sets the values of the culling box in the visual effects graph.
        /// And in that way decides which points are visible.
        /// </summary>
        public void UpdateCullingBox()
        {
            cullPos1.x = 1f - cullingWalls[0].handle.transform.localPosition.x;
            cullPos1.y = 1f - cullingWalls[1].handle.transform.localPosition.x;
            cullPos1.z = 1f - cullingWalls[2].handle.transform.localPosition.x;
            if (singleSliceViewMode != -1)
            {
                cullPos2[singleSliceViewMode] = cullPos1[singleSliceViewMode] - 0.9f;
            }
            else
            {
                cullPos2.x = cullingWalls[3].handle.transform.localPosition.x - 1f;
                cullPos2.y = cullingWalls[4].handle.transform.localPosition.x - 1f;
                cullPos2.z = cullingWalls[5].handle.transform.localPosition.x - 1f;
            }

            vfx.SetVector3("CullingCubePos", cullPos1);
            vfx.SetVector3("CullingCube2Pos", cullPos2);
            ReferenceManager.instance.multiuserMessageSender.SendMessageUpdateCullingBox(pointCloud.pcID, cullPos1, cullPos2);
        }

        /// <summary>
        /// Update the values in the visual effects graph that decides which points are visible.
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        public void UpdateCullingBox(Vector3 pos1, Vector3 pos2)
        {
            vfx.SetVector3("CullingCubePos", pos1);
            vfx.SetVector3("CullingCube2Pos", pos2);
        }

        /// <summary>
        /// Plays an animation to indicate a slicing of the graph was initiated and is running.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="toggle"></param>
        public void BoxAnimation(int axis, int toggle)
        {
            Material mat = box.GetComponent<Renderer>().material;
            mat.SetInt("_WaveToggle", toggle);
            mat.SetInt("_WaveAxis", axis);
            if (toggle > 0)
            {
                boxTween = DOVirtual.Float(-1.5f, 1.5f, 1.2f, v =>
                {
                    mat.SetVector("_WaveCoords", new Vector3(v, v, v));
                }).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Restart);
            }
            else
            {
                boxTween.Kill();
            }
        }

        /// <summary>
        /// Set the position of the handles for the culling walls.
        /// </summary>
        public void SetHandlePositions()
        {
            foreach (CullingWall cw in cullingWalls)
            {
                cw.SetStartPosition();
            }
        }
        
        /// <summary>
        /// Sets the graph to only view one slice of points at a time. 
        /// </summary>
        /// <param name="axis">Decides in which direction to divide up the graph when viewing.</param>
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

        /// <summary>
        /// Animates the culling to get a slice by slice view of the graph.
        /// </summary>
        public void SliceBySliceAnimation()
        {
            CullingWall cw = cullingWalls[singleSliceViewMode];
            float targetX = 1f - (1f * cw.transform.localPosition.x);
            cw.transform.DOLocalMoveX(targetX, 2f).SetEase(Ease.Linear);
        }

        /// <summary>
        /// Slice the graph with the plane using the <see cref="SliceGraphSystem"/>.
        /// The normal and position of the plane is used to divide the graph.
        /// </summary>
        public void SliceGraphManual()
        {
            Material mat = blade.GetComponent<Renderer>().material;
            DOVirtual.Float(0f, 0.49f, 0.5f, v =>
            {
                mat.SetFloat("_SliceOffset", v);
            }).SetEase(Ease.InOutCubic).SetLoops(2, LoopType.Yoyo);
            sliceGraphSystem.Slice(pointCloud.pcID, plane.transform.forward, plane.transform.position);
            ReferenceManager.instance.multiuserMessageSender.SendMessageSliceGraphManual(pointCloud.pcID, plane.transform.forward, plane.transform.position);
        }

        /// <summary>
        /// Slice the graph based on which points are selected.
        /// </summary>
        public void SliceGraphFromSelection()
        {
            Material mat = blade.GetComponent<Renderer>().material;
            DOVirtual.Float(0f, 0.49f, 0.5f, v =>
            {
                mat.SetFloat("_SliceOffset", v);
            }).SetEase(Ease.InOutCubic).SetLoops(2, LoopType.Yoyo);
            sliceGraphSystem.SliceFromSelection(pointCloud.pcID);
            BoxAnimation(2, 1);
            ReferenceManager.instance.multiuserMessageSender.SendMessageSliceGraphFromSelection(pointCloud.pcID);
        }

        /// <summary>
        /// Slicing the graph based on an axis.
        /// Dividing upp the graph into equally big.
        /// </summary>
        /// <param name="axis">The axis used to divide up the graph.</param>
        /// <param name="nrOfSlices">The nr of slices to make.</param>
        public void SliceGraphAutomatic(int axis, int nrOfSlices)
        {
            BoxAnimation(axis, 1);
            StartCoroutine(graphSlice.SliceAxis(axis, sliceGraphSystem.GetPoints(pointCloud.pcID), nrOfSlices));
            ReferenceManager.instance.multiuserMessageSender.SendMessageSliceGraphAutomatic(pointCloud.pcID, axis, nrOfSlices);
        }

        /// <summary>
        /// Morph to other graph to highlight differences between the two representations.
        /// Animates the position of each point to its corresponding position in the other.
        /// </summary>
        public void MorphToOtherGraph()
        {
            pointCloud.GetComponent<VisualEffect>().SetTexture("TargetPosMapTex", pointCloud.targetPositionTextureMap);
            pointCloud.Morph();
        }

        /// <summary>
        /// Activates the reference glass organ and places it on top of the graph points.
        /// </summary>
        /// <param name="toggle"></param>
        public void ToggleReferenceOrgan(bool toggle)
        {
            GetComponentInChildren<ReferenceOrganToggleButton>().CurrentState = toggle;
        }

        /// <summary>
        /// Toggle the manual slicing plane used to divide up the graph.
        /// </summary>
        /// <param name="toggle"></param>
        public void ToggleManualSlicer(bool toggle)
        {
            Automatic = !toggle;
            plane.SetActive(toggle);
            Axis = 2;
        }

        /// <summary>
        /// Change the axis used to slice the graph.
        /// </summary>
        /// <param name="axis"></param>
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

        /// <summary>
        /// Retrieve the plane used to slice the graph.
        /// </summary>
        /// <returns></returns>
        public Plane GetPlane()
        {
            return new Plane(plane.transform.forward, plane.transform.position);
        }

    }
}