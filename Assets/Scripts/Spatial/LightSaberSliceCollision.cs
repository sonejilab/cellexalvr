using AnalysisLogic;
using CellexalVR.General;
using DefaultNamespace;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace CellexalVR.Spatial
{
    public class LightSaberSliceCollision : MonoBehaviour
    {
        private Vector3 enterPosition;
        private Vector3 exitPosition;
        private GameObject enterMarker;
        private GameObject enterMarker2;
        private GameObject exitMarker;
        private GameObject exitMarker2;
        private BoxCollider collider;
        private bool firstPoint;
        private bool lastPoint;
        private float3 previousPoint;
        private int graphToCut = -1;
        [SerializeField] private LineRenderer planeVisualizer;
        [SerializeField] private VisualEffect vfx;
        [SerializeField] private MeshFilter planeMesh;
        private PointCloud pcToSlice;

        public GameObject positionMarker;
        public GameObject planeMarker;
        [HideInInspector] public bool cuttingActive;

        [SerializeField] private GameObject[] spheres = new GameObject[4];


        private void Start()
        {
            collider = GetComponent<BoxCollider>();
            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
            CellexalEvents.RightTriggerPressed.AddListener(OnTriggerDown);
            CellexalEvents.RightTriggerUp.AddListener(OnTriggerUp);
        }

        private void OnTriggerClick()
        {
            vfx.enabled = true;
            cuttingActive = true;
            enterPosition = transform.position;
            //planeVisualizer.enabled = true;
            planeMesh.transform.parent = null;
            planeMesh.transform.rotation = Quaternion.identity;
            planeMesh.transform.position = Vector3.zero;
            planeMesh.transform.localScale = Vector3.one;
            spheres[0].transform.position = enterPosition - 0.3f * transform.up;
            spheres[1].transform.position = enterPosition + 0.3f * transform.up;
            planeMesh.gameObject.SetActive(true);
            lastPoint = false;
        }

        private void OnTriggerDown()
        {


        }

        private void OnTriggerUp()
        {
            cuttingActive = false;
            firstPoint = lastPoint = false;
            planeVisualizer.enabled = false;
            vfx.enabled = false;
            exitPosition = transform.position;
            CreateSlicePlane(enterPosition, exitPosition);
            planeVisualizer.enabled = false;
            graphToCut = -1;
            cuttingActive = false;
        }

        private void Update()
        {

            if (cuttingActive)
            {
                //int hashMapKey = QuadrantSystem.GetPositionHashMapKey(transform.position);
                //SaveClosestPointPosition(hashMapKey); // * QuadrantSystem.quadrantYMultiplier); // current quadrant
                //SaveClosestPointPosition();
                VisualizeCuttingPlane();
            }
        }

        [BurstCompile]
        private float3 SaveClosestPointPosition()
        {
            if (pcToSlice == null) return float3.zero;
            float3 saberInPCLocalPos = pcToSlice.transform.InverseTransformPoint(transform.position);
            int hashMapKey = QuadrantSystem.GetPositionHashMapKey(saberInPCLocalPos);
            if (QuadrantSystem.quadrantMultiHashMaps[pcToSlice.pcID].TryGetFirstValue(hashMapKey, out QuadrantData quadrantData,
                out NativeMultiHashMapIterator<int> nativeMultiHashMapIterator))
            {
                do
                {
                    Vector3 difference = saberInPCLocalPos - quadrantData.position;
                    if (!Physics.Raycast(quadrantData.position, difference, difference.magnitude, 1 << 14))
                    {
                        // Debug.DrawRay(pointPosition, difference, Color.green);
                        // AddGraphPointToSelection(quadrantData);
                        //if (!firstPoint)
                        //{
                        //    // enterPosition = quadrantData.position;
                        //    //enterPosition = transform.position;
                        //    graphToCut = quadrantData.parentID;
                        //    firstPoint = true;
                        //    //planeVisualizer.SetPosition(1, (enterPosition - 0.4f * transform.up));
                        //    //planeVisualizer.enabled = true;
                        //}

                        //else if (quadrantData.parentID != graphToCut)
                        //{
                        //    firstPoint = false;
                        //    exitPosition = transform.position;
                        //    //planeVisualizer.enabled = false;
                        //    graphToCut = -1;
                        //    cuttingActive = false;
                        //}
                    }
                } while (QuadrantSystem.quadrantMultiHashMaps[0].TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
            }

            //else if (firstPoint)
            //{
            //    firstPoint = false;
            //    exitPosition = transform.position;
            //    CreateSlicePlane(enterPosition, exitPosition, graphToCut);
            //    planeVisualizer.enabled = false;
            //    graphToCut = -1;
            //    cuttingActive = false;
            //}

            return quadrantData.position;
        }


        private void CreateSlicePlane(float3 enter, float3 exit)
        {
            if (pcToSlice == null) return;
            Plane plane = new Plane(spheres[0].transform.position, spheres[2].transform.position, spheres[3].transform.position);
            spheres[4].transform.position = (enterPosition + exitPosition) / 2f;
            World.DefaultGameObjectInjectionWorld.GetExistingSystem<SliceGraphSystem>().Slice(pcToSlice.pcID, plane.normal, spheres[4].transform.position);
            planeMesh.gameObject.SetActive(false);
        }


        private void VisualizeCuttingPlane()
        {
            Vector3 dir = math.normalize(enterPosition - transform.position);
            //Vector3 pos1 = exitPosition - 0.4f * transform.up;
            //Vector3 pos2 = exitPosition + 0.4f * transform.up;
            //Vector3 pos3 = pos1 + dir;
            //Vector3 pos4 = pos2 + dir;
            Mesh mesh = planeMesh.mesh;


            spheres[2].transform.position = transform.position - 0.3f * transform.up;
            spheres[3].transform.position = transform.position + 0.3f * transform.up;

            Vector3[] vertices = new Vector3[4]
            {
                 spheres[0].transform.position,
                 spheres[1].transform.position,
                 spheres[2].transform.position,
                 spheres[3].transform.position,
            };

            mesh.vertices = vertices;

            int[] tris = new int[6]
            {
                 0, 2, 1,
                 2, 3, 1,
            };

            mesh.triangles = tris;

            Vector3[] normals = new Vector3[4]
            {
                 -Vector3.forward,
                 -Vector3.forward,
                 -Vector3.forward,
                 -Vector3.forward,
            };

            mesh.normals = normals;
            planeMesh.sharedMesh = mesh;
        }

        private void OnTriggerEnter(Collider other)
        {
            PointCloud pc = other.GetComponent<PointCloud>();
            if (pc != null)
            {
                pcToSlice = pc;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<PointCloud>())
            {
                pcToSlice = null;
            }
        }
    }
}