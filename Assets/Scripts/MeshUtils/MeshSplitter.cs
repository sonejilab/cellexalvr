using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace CellexalVR.MeshUtils
{
    /// <summary>
    /// Class to split a mesh into separate meshed based on connectivity of the vertices.
    /// </summary>
    public class MeshSplitter : MonoBehaviour
    {
        /// <summary>
        /// Takes a mesh as arguments and splits it into a list of separated meshes then returns that list.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static List<Mesh> SplitMesh(Mesh mesh)
        {
            // first split
            List<Mesh> parts = SplitMeshByConnectivity(mesh);
            List<Mesh> allParts = new List<Mesh>();
            // while there are more than 1 part keep splitting.
            while (parts.Count > 1)
            {
                if (parts[0].triangles.Length > 20)
                {
                    allParts.Add(parts[0]);
                }
                parts = SplitMeshByConnectivity(parts[1]);
            }

            // In some cases meshes have small islands of vertices which in this case we consider not to be a seperate part. 
            // Remove this if you want every unconnected part no matter how small to be separated.
            if (parts[0].triangles.Length > 20)
            {
                allParts.Add(parts[0]);
            }

            return allParts;
        }

        /// <summary>
        /// Function to do the actual check for connectivity and dividing of a mesh into two parts.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        private static List<Mesh> SplitMeshByConnectivity(Mesh mesh)
        {
            List<Mesh> meshParts = new List<Mesh>();
            List<int> part1Triangles = new List<int>();
            part1Triangles.AddRange(mesh.triangles);
            List<Vector3> part1Vertices = new List<Vector3>();
            part1Vertices.AddRange(mesh.vertices);

            List<Vector3> part2Vertices = new List<Vector3>();
            List<int> part2Triangles = new List<int>();


            // Start with 1 arbitrary triangle
            for (int i = 10; i < 13; i++)
            {
                part2Triangles.Add(part1Triangles[i]);
            }

            for (int i = 0; i < part1Triangles.Count - 2; i += 3)
            {
                if (part2Triangles.Contains(part1Triangles[i]) || part2Triangles.Contains(part1Triangles[i + 1]) || part2Triangles.Contains(part1Triangles[i + 2]))
                {
                    part2Triangles.Add(part1Triangles[i]);
                    part2Triangles.Add(part1Triangles[i + 1]);
                    part2Triangles.Add(part1Triangles[i + 2]);


                    if (!part2Vertices.Contains(part1Vertices[part1Triangles[i]]))
                    {
                        part2Vertices.Add(part1Vertices[part1Triangles[i]]);
                    }
                    if (!part2Vertices.Contains(part1Vertices[part1Triangles[i + 1]]))
                    {
                        part2Vertices.Add(part1Vertices[part1Triangles[i + 1]]);
                    }
                    if (!part2Vertices.Contains(part1Vertices[part1Triangles[i + 2]]))
                    {
                        part2Vertices.Add(part1Vertices[part1Triangles[i + 2]]);
                    }

                    //part1Triangles.RemoveAt(i);
                    //part1Triangles.RemoveAt(i + 1);
                    //part1Triangles.RemoveAt(i + 2);
                }
            }


            // TODO: Remove vertices not used and reindex triangles.
            for (int i = 0; i < part2Triangles.Count; i++)
            {
                part1Triangles.Remove(part2Triangles[i]);
            }

            //if (id == 8 || id == 997)
            //{
            //    meshParts.Add(mesh);
            //}

            if (part1Triangles.Count > 10 && part2Triangles.Count > 10)
            {
                Mesh newMesh = new Mesh();

                for (int i = 0; i < part2Triangles.Count; i++)
                {
                    part1Triangles.Remove(part2Triangles[i]);
                }

                for (int i = 0; i < part2Vertices.Count; i++)
                {
                    print($"remove vertex {i}");
                    part1Vertices.Remove(part2Vertices[i]);
                }

                newMesh.vertices = part2Vertices.ToArray();
                newMesh.triangles = part2Triangles.ToArray();

                newMesh.RecalculateBounds();
                newMesh.RecalculateNormals();

                meshParts.Add(newMesh);

                Mesh newMesh2 = new Mesh();
                newMesh2.vertices = part1Vertices.ToArray();
                newMesh2.triangles = part1Triangles.ToArray();

                newMesh2.RecalculateBounds();
                newMesh2.RecalculateNormals();

                meshParts.Add(newMesh2);
            }

            else if (part1Triangles.Count > 10)
            {
                Mesh newMesh = new Mesh();
                newMesh.vertices = part1Vertices.ToArray();
                newMesh.triangles = part1Triangles.ToArray();

                newMesh.RecalculateBounds();
                newMesh.RecalculateNormals();
                meshParts.Add(newMesh);
            }

            else if (part2Triangles.Count > 10)
            {
                Mesh newMesh2 = new Mesh();
                newMesh2.vertices = part1Vertices.ToArray();
                newMesh2.triangles = part2Triangles.ToArray();

                newMesh2.RecalculateBounds();
                newMesh2.RecalculateNormals();

                meshParts.Add(newMesh2);
            }

            else
            {
                meshParts.Add(mesh);
            }


            return meshParts;

        }
    }

}