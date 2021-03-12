using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Valve.VR;

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

        public GameObject positionMarker;
        public GameObject planeMarker;
        public SteamVR_Action_Boolean grabPinch;
        [HideInInspector] public bool cuttingActive;


        private void Start()
        {
            collider = GetComponent<BoxCollider>();
        }

        private void Update()
        {
            if (grabPinch.stateDown)
            {
                cuttingActive = true;
                lastPoint = false;
            }

            if (grabPinch.stateUp)
            {
                cuttingActive = false;
                firstPoint = lastPoint = false;
                planeVisualizer.enabled = false;
            }

            // if (cuttingActive)
            // {
            //     int hashMapKey = QuadrantSystem.GetPositionHashMapKey(transform.position);
            //     SaveClosestPointPosition(hashMapKey); // * QuadrantSystem.quadrantYMultiplier); // current quadrant
            //     VisualizeCuttingPlane();
            // }
        }

        // [BurstCompile]
        // private float3 SaveClosestPointPosition(int hashMapKey)
        // {
        //     float3 position = transform.position;
        //     if (QuadrantSystem.quadrantMultiHashMaps[0].TryGetFirstValue(hashMapKey, out QuadrantData quadrantData,
        //         out NativeMultiHashMapIterator<int> nativeMultiHashMapIterator))
        //     {
        //         do
        //         {
        //             Vector3 difference = position - quadrantData.position;
        //             if (!Physics.Raycast(quadrantData.position, difference, difference.magnitude,
        //                 1 << 6))
        //             {
        //                 // Debug.DrawRay(pointPosition, difference, Color.green);
        //                 // AddGraphPointToSelection(quadrantData);
        //                 if (!firstPoint)
        //                 {
        //                     // enterPosition = quadrantData.position;
        //                     enterPosition = transform.position;
        //                     graphToCut = quadrantData.point.parentId;
        //                     firstPoint = true;
        //                     planeVisualizer.SetPosition(0, enterPosition);
        //                     planeVisualizer.enabled = true;
        //                 }
        //     
        //                 else if (quadrantData.point.parentId != graphToCut)
        //                 {
        //                     firstPoint = false;
        //                     exitPosition = transform.position;
        //                     planeVisualizer.enabled = false;
        //                     CreateSlicePlane(enterPosition, exitPosition, graphToCut);
        //                     graphToCut = -1;
        //                 }
        //             }
        //         } while (QuadrantSystem.quadrantMultiHashMaps[0].TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
        //     }
        //     
        //     else if (firstPoint)
        //     {
        //         firstPoint = false;
        //         exitPosition = transform.position;
        //         CreateSlicePlane(enterPosition, exitPosition, graphToCut);
        //         planeVisualizer.enabled = false;
        //     }
        //     
        //     return quadrantData.position;
        // }

        // private void CreateSlicePlane(float3 enter, float3 exit, int graphId)
        // {
        //     Vector3 dir = math.normalize(enter - exit);
        //     Vector3 pos1 = exitPosition - 0.5f * transform.up;
        //     Vector3 pos2 = exitPosition + 0.5f * transform.up;
        //     Vector3 pos3 = pos1 + dir;
        //     Vector3 pos4 = pos2 + dir;
        //     Mesh mesh = new Mesh();
        //
        //     Vector3[] vertices = new Vector3[4]
        //     {
        //         pos1,
        //         pos3,
        //         pos2,
        //         pos4,
        //     };
        //
        //     mesh.vertices = vertices;
        //
        //     int[] tris = new int[6]
        //     {
        //         0, 2, 1,
        //         2, 3, 1,
        //     };
        //
        //     mesh.triangles = tris;
        //
        //     Vector3[] normals = new Vector3[4]
        //     {
        //         -Vector3.forward,
        //         -Vector3.forward,
        //         -Vector3.forward,
        //         -Vector3.forward,
        //     };
        //
        //     mesh.normals = normals;
        //     Plane plane = new Plane(pos1, pos2, pos3);
        //     World.DefaultGameObjectInjectionWorld.GetExistingSystem<SliceGraphSystem>().Slice(graphId, plane.normal, mesh.bounds.center);
        //
        //     var diff = math.abs(enterPosition - exitPosition);
        //     // GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad); // for debugging
        //     // quad.GetComponent<MeshFilter>().mesh = mesh; //for debugging
        // }
        //
        //
        // private void VisualizeCuttingPlane()
        // {
        //     planeVisualizer.SetPosition(1, transform.position);
        // }
    }
}