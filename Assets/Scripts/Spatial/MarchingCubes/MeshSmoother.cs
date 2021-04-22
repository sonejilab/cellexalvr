using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MarchingCubes
{
    public class MeshSmoother
    {
        public static Mesh SmoothMesh(Mesh originalMesh, int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                originalMesh.vertices = LaplacianFilter(originalMesh.vertices, originalMesh.triangles);
                originalMesh.RecalculateBounds();
                originalMesh.RecalculateNormals();
            }
            return originalMesh;
        }

        public static Mesh[] SmoothMeshes(Mesh[] originalMeshes, int iterations)
        {
            Dictionary<int, Vector3[]> allVertices = new Dictionary<int, Vector3[]>();
            List<int> allTriangles = new List<int>();
            for (int i = 0; i < originalMeshes.Length; i++)
            {
                Mesh m = originalMeshes[i];
                allVertices[i] = m.vertices;
                allTriangles.AddRange(m.triangles);
            }

            var newVertices = LaplacianFilter(allVertices, allTriangles.ToArray(), iterations);
            for (int i = 0; i < originalMeshes.Length; i++)
            {
                Mesh m = originalMeshes[i];
                m.vertices = newVertices[i];
                m.RecalculateBounds();
                m.RecalculateNormals();
            }

            //originalMeshes.RecalculateBounds();
            //originalMeshes.RecalculateNormals();
            return originalMeshes;
        }

        public static Dictionary<int, Vector3[]> LaplacianFilter(Dictionary<int, Vector3[]> meshDict, int[] triangles, int iterations)
        {
            Dictionary<int, Vector3[]> smoothedMeshDict = new Dictionary<int, Vector3[]>();
            Dictionary<int, VertexConnection> adjacencyDict = BuildVertexAdjacencyDict(triangles);
            Vector3 dV;
            foreach (KeyValuePair<int, Vector3[]> kvp in meshDict)
            {
                Vector3[] originalVertices = kvp.Value;
                Vector3[] smoothedVertices = new Vector3[originalVertices.Length];
                for (int i = 0; i < originalVertices.Length; i++)
                {
                    dV = Vector3.zero;
                    if (!adjacencyDict.ContainsKey(i)) continue;
                    HashSet<int> adjacentVertices = adjacencyDict[i].connections;
                    foreach (int adjVertInd in adjacentVertices)
                    {
                        Vector3 vert = originalVertices[adjVertInd];
                        dV += vert;
                    }

                    dV /= adjacentVertices.Count;
                    smoothedVertices[i] = dV;
                }
                smoothedMeshDict[kvp.Key] = smoothedVertices;
            }

            return smoothedMeshDict;
        }

        public static Vector3[] LaplacianFilter(Vector3[] originalVertices, int[] triangles)
        {
            Vector3[] smoothedVertices = new Vector3[originalVertices.Length];
            Dictionary<int, VertexConnection> adjacencyDict = BuildVertexAdjacencyDict(triangles);
            //Dictionary<Vector3, VertexConnection> adjacencyDict = BuildVertexAdjacencyDict(originalVertices);
            Vector3 dV;
            for (int i = 0; i < originalVertices.Length; i++)
            {
                dV = Vector3.zero;
                if (!adjacencyDict.ContainsKey(i)) continue;
                HashSet<int> adjacentVertices = adjacencyDict[i].connections;
                foreach (int adjVertInd in adjacentVertices)
                {
                    Vector3 vert = originalVertices[adjVertInd];
                    dV += vert;
                }

                dV /= adjacentVertices.Count;
                smoothedVertices[i] = dV;
            }

            return smoothedVertices;
        }

        private static Dictionary<Vector3, VertexConnection> BuildVertexAdjacencyDict(Vector3[] vertices)
        {
            Dictionary<Vector3, VertexConnection> adjacencyDict = new Dictionary<Vector3, VertexConnection>();
            Dictionary<Vector3, int> duplicateHashTable = new Dictionary<Vector3, int>();

            //List<int> newVerts = new List<int>();
            //int[] map = new int[vertices.Length];

            ////create mapping and find duplicates, dictionaries are like hashtables, mean fast
            //for (int i = 0; i < vertices.Length; i++)
            //{
            //    if (!adjacencyDict.ContainsKey(vertices[i]))
            //    {
            //        adjacencyDict.Add(vertices[i], newVerts.Count);
            //        map[i] = newVerts.Count;
            //        newVerts.Add(i);
            //    }
            //    else
            //    {
            //        map[i] = duplicateHashTable[vertices[i]];
            //    }
            //}

            //// create new vertices
            //var verts2 = new Vector3[newVerts.Count];
            //var normals2 = new Vector3[newVerts.Count];
            //var uvs2 = new Vector2[newVerts.Count];
            //for (int i = 0; i < newVerts.Count; i++)
            //{
            //    int a = newVerts[i];
            //    verts2[i] = vertices[a];
            //    //normals2[i] = normals[a];
            //    //uvs2[i] = uvs[a];
            //}
            //// map the triangle to the new vertices
            //var tris = aMesh.triangles;
            //for (int i = 0; i < tris.Length; i++)
            //{
            //    tris[i] = map[tris[i]];
            //}
            //aMesh.triangles = tris;
            //aMesh.vertices = v;


            return adjacencyDict;
        }

        private static Dictionary<int, VertexConnection> BuildVertexAdjacencyDict(int[] triangles)
        {
            Dictionary<int, VertexConnection> adjacencyDict = new Dictionary<int, VertexConnection>();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int vA = triangles[i];
                int vB = triangles[i + 1];
                int vC = triangles[i + 2];

                if (!adjacencyDict.ContainsKey(vA))
                {
                    adjacencyDict.Add(vA, new VertexConnection());
                }

                if (!adjacencyDict.ContainsKey(vB))
                {
                    adjacencyDict.Add(vB, new VertexConnection());
                }

                if (!adjacencyDict.ContainsKey(vC))
                {
                    adjacencyDict.Add(vC, new VertexConnection());
                }

                adjacencyDict[vA].Connect(vB);
                adjacencyDict[vA].Connect(vC);
                adjacencyDict[vB].Connect(vA);
                adjacencyDict[vB].Connect(vC);
                adjacencyDict[vC].Connect(vA);
                adjacencyDict[vC].Connect(vB);
            }

            return adjacencyDict;
        }


        private static List<Vector3> FindAdjacentVertices(Vector3[] vertices, int index, int[] triangle)
        {
            Vector3 vertex = vertices[index];
            List<Vector3> adjacentVertices = new List<Vector3>();
            foreach (Vector3 v in vertices)
            {
                adjacentVertices.Add(v);
            }

            return adjacentVertices;
        }
    }

    public class VertexConnection
    {
        public VertexConnection()
        {
            connections = new HashSet<int>();
        }

        public void Connect(int vi)
        {
            connections.Add(vi);
        }

        public HashSet<int> connections;
    }
}