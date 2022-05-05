using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DG.Tweening;
using System.Linq;

namespace CellexalVR.Spatial
{
    public class SpatialReferenceModelPart : MonoBehaviour
    {
        public int id;
        public string modelName;
        public string modelAcronym;
        public Color color;
        public GameObject plane;

        private bool spread;
        private GameObject meshParent;
        private Transform child;
        private Renderer[] rend;
        private MaterialPropertyBlock propertyBlock;


        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.U))
            //{
            //    SplitMesh();
            //}
            //if (Input.GetKeyDown(KeyCode.Y))
            //{
            //    StartCoroutine(Spread());
            //}
        }

        public void SetColor(Color col)
        {
            rend = GetComponentsInChildren<MeshRenderer>();
            color = col;
            //if (propertyBlock == null)
            //    propertyBlock = new MaterialPropertyBlock();
            //propertyBlock.SetColor("_Color", col);
            foreach (MeshRenderer r in rend)
            {
                //r.SetPropertyBlock(propertyBlock);
                r.material.SetColor("_Color", col);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Split part into separate meshes based on connectivity. This enables the right and left part to be moved separately. 
        /// Many of the allen reference brain meshes are mirrored right to left which makes this a usable feature.
        /// </summary>
        public void SplitMesh()
        {
            MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
            child = transform.GetChild(0);
            MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
            List<Mesh> parts = new List<Mesh>();
            //if (AllenReferenceBrain.instance.nonSplittableMeshes.Contains(id))
            //{
            //    parts.Add(meshFilter.mesh);
            //}
            //else
            //{
            //}
            //Mesh m = meshFilter.mesh;
            //Vector3[] vertices = new Vector3[meshFilters[0].mesh.vertexCount + meshFilters[1].mesh.vertexCount];
            //int i = 0;
            //foreach (MeshFilter mf in meshFilters)
            //{
            //    foreach (Vector3 v in mf.mesh.vertices)
            //    {
            //        vertices[i++] = v;
            //    }
            //}
            //m.vertices = vertices;
            //meshFilter.mesh = m;
            parts = CellexalVR.MeshUtils.MeshSplitter.SplitMesh(meshFilter.mesh);

            meshParent = new GameObject();
            meshParent.transform.parent = transform;
            meshParent.transform.localPosition = child.transform.localPosition;
            meshParent.transform.localRotation = child.transform.localRotation;
            meshParent.transform.localScale = child.transform.localScale;

            child.transform.parent = meshParent.transform;
            child.transform.localPosition = Vector3.zero;
            child.transform.localScale = Vector3.one;
            child.transform.localRotation = Quaternion.identity;

            for (int i = 0; i < parts.Count; i++)
            {
                Mesh m = parts[i];
                GameObject newObj = new GameObject();
                newObj.AddComponent<MeshRenderer>().material = meshRenderer.material;
                newObj.AddComponent<MeshFilter>().mesh = m;
                MeshCollider meshColldier = newObj.AddComponent<MeshCollider>();

                newObj.transform.parent = meshParent.transform;
                newObj.transform.localPosition = child.transform.localPosition;
                newObj.transform.localRotation = child.transform.localRotation;
                newObj.transform.localScale = Vector3.one;
                newObj.gameObject.name = $"part{i}";

                AssetDatabase.CreateAsset(m, $"Assets/Resources/meshparts/{id}_part{i}_test.asset");
            }

            Destroy(child.gameObject);

        }

        public void AddColliders()
        {
            // Move plane in z direction and add colliders....
            MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
            Transform t = meshFilter.transform;
            Vector3 forward = (plane.transform.forward);
            Vector3 center = plane.transform.localPosition;
            Mesh m = meshFilter.sharedMesh;
            List<Vector3> verticesLeft = new List<Vector3>();
            List<Vector3> verticesRight = new List<Vector3>();
            float minX = int.MaxValue;
            float maxX = int.MinValue;
            
            float minY = int.MaxValue;
            float maxY = int.MinValue;
            
            float minZ = int.MaxValue;
            float maxZ = int.MinValue;
            
            float minXR = int.MaxValue;
            float maxXR = int.MinValue;
            
            float minYR = int.MaxValue;
            float maxYR = int.MinValue;

            float minZR = int.MaxValue;
            float maxZR = int.MinValue;
            Vector3 centroidLeft = Vector3.zero;
            Vector3 centroidRight = Vector3.zero;
            foreach (Vector3 vertex in m.vertices)
            {
                if (Vector3.Dot(forward, vertex - center) > 0)
                {
                    verticesLeft.Add(vertex);
                    centroidLeft += vertex;
                    if (vertex.x > maxX)
                        maxX = vertex.x;
                    if (vertex.y > maxY)
                        maxY = vertex.y;
                    if (vertex.z > maxZ)
                        maxZ = vertex.z;
                    if (vertex.x < minX)
                        minX = vertex.x;
                    if (vertex.y < minY)
                        minY = vertex.y;
                    if (vertex.z < minZ)
                        minZ = vertex.z;
                }
                else
                {
                    verticesRight.Add(vertex);
                    centroidRight += vertex;
                    if (vertex.x > maxXR)
                        maxXR = vertex.x;
                    if (vertex.y > maxYR)
                        maxYR = vertex.y;
                    if (vertex.z > maxZR)
                        maxZR = vertex.z;
                    if (vertex.x < minXR)
                        minXR = vertex.x;
                    if (vertex.y < minYR)
                        minYR = vertex.y;
                    if (vertex.z < minZR)
                        minZR = vertex.z;
                }
            }
            centroidLeft /= verticesLeft.Count;
            centroidRight /= verticesRight.Count;
            print($"adding collider to {gameObject.name}, {minX}, {maxX}, {minY}, {maxY}, {centroidLeft}, {centroidRight}, {verticesLeft.Count}, {verticesRight.Count}");
            var colLeft = t.gameObject.AddComponent<BoxCollider>();
            colLeft.center = new Vector3((maxX - minX) / 2f, (maxY - minY) / 2f, centroidRight.z);
            colLeft.size = new Vector3((maxX - minX), (maxY - minY), (maxZ - minZ));
            var colRight = t.gameObject.AddComponent<BoxCollider>();
            colRight.center = centroidRight;
            colRight.size = new Vector3((maxXR - minXR), (maxYR - minYR), (maxZR - minZR));


        }
#endif

        public void Spread()
        {
            if (id == 997)
                return;

            spread = !spread;
            //float t = 0f;
            //float animationTime = 1f;
            var brain = GetComponentInParent<AllenReferenceBrain>();
            Vector3 center = brain.GetComponent<BoxCollider>().center;
            center = brain.transform.TransformPoint(center);
            MeshCollider[] mcs = GetComponentsInChildren<MeshCollider>();
            Vector3 targetPos;
            foreach (MeshCollider mc in mcs)
            {
                if (spread)
                {
                    targetPos = mc.transform.position + (mc.bounds.center - center).normalized * 0.75f;
                }
                else
                {
                    targetPos = transform.GetChild(0).transform.TransformPoint(Vector3.zero);
                }
                mc.transform.DOMove(targetPos, 0.6f).SetEase(Ease.InOutSine);
            }
            //for (int i = 0; i < mrs.Length; i++)
            //{
            //    start[i] = new Vector3(mrs[i].transform.position.x, mrs[i].transform.position.y, mrs[i].transform.position.z);
            //    dirs[i] = (mrs[i].bounds.center - center).normalized;
            //    if (spread)
            //    {
            //        end[i] = start[i] + dirs[i] * 0.5f;
            //    }
            //    else
            //    {
            //        end[i] = transform.GetChild(0).transform.TransformPoint(Vector3.zero);
            //    }
            //}

            //while (t < animationTime)
            //{
            //    float progress = Mathf.SmoothStep(0, animationTime, t);
            //    for (int i = 0; i < mrs.Length; i++)
            //    {
            //        mrs[i].transform.position = Vector3.Lerp(start[i], end[i], progress);
            //        //Debug.DrawLine(mcs[i].transform.position, center);
            //    }
            //    t += (Time.deltaTime / animationTime);
            //    yield return null;
            //}

        }

    }
#if UNITY_EDITOR
    [CustomEditor(typeof(SpatialReferenceModelPart))]
    public class SpatialReferenceModelPartEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SpatialReferenceModelPart myTarget = (SpatialReferenceModelPart)target;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Split Meshes"))
            {
                myTarget.SplitMesh();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Colliders"))
            {
                myTarget.AddColliders();
            }
            GUILayout.EndHorizontal();

            DrawDefaultInspector();
        }

    }
#endif
}
