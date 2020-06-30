using System;
using System.Net;
using CellexalVR.General;
using UnityEngine;
using UnityEngine.XR;

namespace CellexalVR
{
    [RequireComponent(typeof(MeshFilter))]
    public class MeshDeformer : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public float force = 20;
        public float forceOffset = 0.1f;
        public float springForce = 20f;
        public float damping = 5f;
        [HideInInspector] public Vector3[] displacedVertices;
        [HideInInspector] public Vector3[] originalVertices;

        public Transform settingsHandlerTransform;
        
        private Mesh deformingMesh;
        private Vector3[] vertexVelocities;
        private SteamVR_Controller.Device device;
        private SteamVR_TrackedObject rightController;

        private float uniformScale = 1f;

        // private float alpha = 1f;
        private float timeSinceClick;
        private Ray firstRay;
        private Ray secondRay;


        private void Start()
        {
            // rightController = referenceManager.rightController;
            deformingMesh = GetComponent<MeshFilter>().mesh;
            originalVertices = deformingMesh.vertices;
            displacedVertices = new Vector3[originalVertices.Length];
            vertexVelocities = new Vector3[originalVertices.Length];
            for (int i = 0; i < displacedVertices.Length; i++)
            {
                displacedVertices[i] = originalVertices[i];
            }
            
        }

        private void Update()
        {
            // settingsHandlerTransform.localPosition = new Vector3(0.5f, -0.1f, 0);
            // settingsHandlerTransform.localRotation = transform.localRotation;
            if (timeSinceClick < 2)
            {
                uniformScale = transform.localScale.x;
                for (int i = 0; i < displacedVertices.Length; i++)
                {
                    UpdateVertex(i);
                }
            }
            // else if (timeSinceClick < 5)
            // {
            //     springForce = 20;
            //     uniformScale = transform.localScale.x;
            //     for (int i = 0; i < displacedVertices.Length; i++)
            //     {
            //         UpdateVertex(i);
            //     }
            // }

            deformingMesh.vertices = displacedVertices;
            deformingMesh.RecalculateNormals();
            deformingMesh.RecalculateBounds();

            int index = (int) referenceManager.rightController.index;
            device = SteamVR_Controller.Input(index);
            if (Input.GetMouseButton(0))
            {
                // print("Down");
                firstRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                HandleInputOneClick(firstRay);
            }

            // if (Input.GetMouseButtonUp(0))
            // {
            //     print("up");
            //     secondRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            //     // HandleInputTwoClick(firstRay, secondRay);
            // }

            // if (device.GetPress(SteamVR_Controller.ButtonMask.Trigger))
            // {
            //     HandleInput(new Ray(referenceManager.rightController.transform.position,
            //         referenceManager.rightController.transform.forward));
            // }
            
            if (device.GetPress(SteamVR_Controller.ButtonMask.Trigger))
            {
                firstRay = new Ray(referenceManager.rightController.transform.position,
                    referenceManager.rightController.transform.forward);
                HandleInputOneClick(firstRay);
            }

            // if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
            // {
            //     secondRay = new Ray(referenceManager.rightController.transform.position,
            //         referenceManager.rightController.transform.forward);
            //     HandleInputTwoClick(firstRay, secondRay);
            // }

            timeSinceClick += Time.deltaTime;
        }

        public void UpdateVertices(Vector3[] newVertices)
        {
            originalVertices = newVertices;
            if (newVertices.Length != displacedVertices.Length)
            {
                displacedVertices = new Vector3[newVertices.Length];
            }
        }

        private void UpdateVertex(int vi)
        {
            Vector3 velocity = vertexVelocities[vi];
            Vector3 displacement = displacedVertices[vi] - originalVertices[vi];
            displacement *= uniformScale;
            velocity -= displacement * springForce * Time.deltaTime;
            velocity *= 1f - damping * Time.deltaTime;
            vertexVelocities[vi] = velocity;
            displacedVertices[vi] += velocity * Time.deltaTime;
        }

        private void HandleInputOneClick(Ray inputRay)
        {
             timeSinceClick = 0;
             if (!Physics.Raycast(inputRay, out RaycastHit hit)) return;
             Vector3 point = hit.point;
             point += hit.normal * forceOffset;
             MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();
             if (!deformer) return;
             deformer.AddDeformingForce(point, force);           
        }
        
        private void HandleInputTwoClick(Ray inputRay1, Ray inputRay2)
        {
            timeSinceClick = 0;
            if (!Physics.Raycast(inputRay1, out RaycastHit hit)) return;
            if (!Physics.Raycast(inputRay2, out RaycastHit secondHit)) return;
            Vector3 point = hit.point;
            point += hit.normal * forceOffset;
            MeshDeformer deformer = secondHit.collider.GetComponent<MeshDeformer>();
            if (!deformer) return;
            Vector3 point2 = secondHit.point;
            point2 += secondHit.normal * forceOffset;
            // print($"add force to p: {point}");
            deformer.AddDeformingForce(point, point2, force);
        }

        private void AddDeformingForce(Vector3 point, float force)
        {
            point = transform.InverseTransformPoint(point);
            for (int i = 0; i < displacedVertices.Length; i++)
            {
                AddForceToVertex(i, point, force);
            }

            // Debug.DrawLine(Camera.main.transform.position, point);
        }


        private void AddDeformingForce(Vector3 point, Vector3 secondPoint, float force)
        {
            point = transform.InverseTransformPoint(point);
            secondPoint = transform.InverseTransformPoint(secondPoint);
            for (int i = 0; i < displacedVertices.Length; i++)
            {
                AddForceToVertex(i, point, secondPoint, force);
            }

            // Debug.DrawLine(Camera.main.transform.position, point);
        }

        private void AddForceToVertex(int i, Vector3 point, float force)
        {
            Vector3 pointToVertex = displacedVertices[i] - point;
            pointToVertex *= uniformScale;
            float attenuatedForce = force / (1 + pointToVertex.sqrMagnitude);
            float velocity = attenuatedForce * Time.deltaTime;
            vertexVelocities[i] += pointToVertex.normalized * velocity;
        }

        private void AddForceToVertex(int i, Vector3 point, Vector3 point2, float force)
        {
            Vector3 vertex = displacedVertices[i];
            Vector3 midPoint = new Vector3((point.x + point2.x) / 2, (point.y + point2.y) / 2, point.z);
            Vector3 pointToVertex = vertex - midPoint;
            // Vector3 pointToVertex = displacedVertices[i] - point;
            // pointToVertex *= uniformScale;
            // if (Math.Abs(pointToVertex.x) < 0.10 &&
            //     Math.Abs(pointToVertex.y) < 0.10)
            // {
            //     displacedVertices[i].z -= 0.1f;
            //     return;
            // }
            // print(vertex.x > point.x && vertex.y < point.y && vertex.x < point2.x && vertex.y > point2.y);
            float attenuatedForce = force / (1 + pointToVertex.sqrMagnitude);
            if (vertex.x > point.x && vertex.y < point.y && vertex.x < point2.x && vertex.y > point2.y)
            {
                vertex.z -= 0.2f;
            }
            // else
            // {
            // }

            vertex += pointToVertex.normalized * attenuatedForce * Time.deltaTime;
            springForce = 0;
            displacedVertices[i] = vertex;
            // float velocity = attenuatedForce * Time.deltaTime;
            // vertexVelocities[i] += forcePointToVertex.normalized * velocity;
        }


        // private void OnTriggerEnter(Collider other)
        // {
        //     Ray inputRay = new Ray(other.transform.position, other.transform.forward);
        //     GetComponent<Renderer>().material.SetFloat("_Alpha", 1);
        //     Vector3 point = other.transform.position;
        //     GetComponent<Renderer>().material.SetFloat("_OffsetX", point.x); // / deformingMesh.bounds.size.x);
        //     GetComponent<Renderer>().material
        //         .SetFloat("_OffsetY", point.y); // / deformingMesh.bounds.size.y);           
        //     // GetComponent<Renderer>().material.SetFloat("_OffsetZ", point.z); // deformingMesh.bounds.size.y);           
        //     // HandleInput(inputRay);
        // }
        //
        //
        // private void OnTriggerStay(Collider other)
        // {
        //     GetComponent<Renderer>().material.SetFloat("_Alpha", 1);
        //     Vector3 point = other.transform.position;
        //     GetComponent<Renderer>().material.SetFloat("_OffsetX", point.x); // / deformingMesh.bounds.size.x);
        //     GetComponent<Renderer>().material
        //         .SetFloat("_OffsetY", point.y); // / deformingMesh.bounds.size.y);           
        //     GetComponent<Renderer>().material.SetFloat("_OffsetZ", point.z); // deformingMesh.bounds.size.y);           
        //     // Ray inputRay = new Ray(other.transform.position, other.transform.forward);
        //     // HandleInput(inputRay);
        // }
        //
        // private void OnTriggerExit(Collider other)
        // {
        //     GetComponent<Renderer>().material.SetFloat("_Alpha", 0);
        // }
    }
}