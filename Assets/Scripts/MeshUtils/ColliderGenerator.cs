using CellexalVR.Spatial;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.MeshUtils
{
    public class ColliderGenerator : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
#if UNITY_EDITOR
        public void AddColliders()
        {
            // Move plane in z direction and add colliders....
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            Transform t = meshFilter.transform;
            //Vector3 forward = (plane.transform.forward);
            //Vector3 center = plane.transform.localPosition;
            Mesh m = meshFilter.sharedMesh;
            List<Vector3> verticesLeft = new List<Vector3>();
            List<Vector3> verticesRight = new List<Vector3>();
            List<Vector3> sortedVertices = new List<Vector3>(m.vertices);
            sortedVertices.Sort((a, b) => a.z.CompareTo(b.z));
            float minX = int.MaxValue;
            float maxX = int.MinValue;
            float minY = int.MaxValue;
            float maxY = int.MinValue;
            float maxZ = sortedVertices[^1].z;
            float minZ = sortedVertices[0].z;
            float incr = m.bounds.size.z / 10f;
            int i = 1;
            float zLim = minZ + (incr * i);
            List<Vector3> vertices = new List<Vector3>();
            Vector3 centroid = Vector3.zero;
            foreach (Vector3 v in sortedVertices)
            {
                centroid += v;
                if (v.x > maxX)
                    maxX = v.x;
                if (v.y > maxY)
                    maxY = v.y;
                if (v.z > maxZ)
                    maxZ = v.z;
                if (v.x < minX)
                    minX = v.x;
                if (v.y < minY)
                    minY = v.y;
                if (v.z < minZ)
                    minZ = v.z;
                if (v.z > zLim)
                {
                    // add collider
                    float temp = maxZ;
                    maxZ = zLim;
                    minZ += incr * (i - 1);
                    centroid /= vertices.Count;
                    AddCollider(vertices, new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ), centroid);

                    vertices.Clear();
                    centroid = Vector3.zero;
                    zLim += incr * (++i);
                    minX = int.MaxValue;
                    maxX = int.MinValue;
                    minY = int.MaxValue;
                    maxY = int.MinValue;
                    minZ = temp;

                }
                vertices.Add(v);
            }
            // add last collider


            float minXR = int.MaxValue;
            float maxXR = int.MinValue;

            float minYR = int.MaxValue;
            float maxYR = int.MinValue;

            float minZR = int.MaxValue;
            float maxZR = int.MinValue;



        }

        private void AddCollider(List<Vector3> positions, Vector3 min, Vector3 max, Vector3 centroid)
        {
            Vector3 center;
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3((max.x + min.x) / 2f, (max.y + min.y) / 2f, (max.z + min.z) / 2f);
            //col.center = (centroid);
            col.size = new Vector3((max.x - min.x), (max.y - min.y), 0.1f);
        }

        public void ClearColliders()
        {
            foreach (Collider col in GetComponents<Collider>())
            {
                DestroyImmediate(col);
            }
        }

        public void RecenterMesh()
        {
            EditorUtility.DisplayProgressBar("Recenter vertices", "", 0f);
            MeshFilter mf = GetComponent<MeshFilter>();
            Mesh m = mf.sharedMesh;
            Vector3[] centeredVertices = new Vector3[m.vertexCount];
            float longestAxis = Mathf.Max(Mathf.Max(m.bounds.size.x, m.bounds.size.y), m.bounds.size.z);
            Vector3 diffCoordValues = m.bounds.max - m.bounds.min;
            Vector3 scaledOffset = (diffCoordValues / longestAxis) / 2;
            Matrix4x4 rotMatrix = Matrix4x4.Rotate(Quaternion.Euler(new Vector3(0, 90, 180)));
            for (int i = 0; i < m.vertexCount; i++)
            {
                EditorUtility.DisplayProgressBar("Recenter vertices", "", (float)i / m.vertexCount);
                Vector3 v = m.vertices[i];
                v -= m.bounds.min;
                v /= longestAxis;
                v -= scaledOffset;
                v = rotMatrix.MultiplyPoint(v);
                centeredVertices[i] = v;
            }
            EditorUtility.ClearProgressBar();

            m.vertices = centeredVertices;
            m.RecalculateBounds();
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ColliderGenerator))]
    public class ColliderGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ColliderGenerator myTarget = (ColliderGenerator)target;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Colliders"))
            {
                myTarget.AddColliders();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Colliders"))
            {
                myTarget.ClearColliders();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Center Vertices"))
            {
                myTarget.RecenterMesh();
            }
            GUILayout.EndHorizontal();
            DrawDefaultInspector();
        }

    }
#endif
}
