using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CellexalVR.Spatial;
using CellexalVR.MarchingCubes;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System.Threading.Tasks;

namespace CellexalVR.MarchingCubes
{
    public class ChunkManager : MonoBehaviour
    {
        public Transform sphereHolder;
        public static int chunkResolution = 32;
        public static int size = 1;
        public static int n = (chunkResolution - 1) * size + 1;
        public static bool addVertexSpheres = false;
        public static bool addDensitySpheres = false;
        public static float surfaceLevel = 1f;

        // public float SurfaceLevel
        // {
        //     get => SurfaceLevel;
        //     set
        //     {
        //         SurfaceLevel = value;
        //         toggleSurfaceLevelandUpdateCubes(0);
        //     }
        // }

        public BoxCollider boxCollider;
        public ChunkScript chunkPrefab;
        public GameObject visualiseChunkPrefab;
        public ChunkScript[,,] chunks;
        public float[,,] density = new float[n, n, n];
        public GameObject[,,] densitySpheres = new GameObject[n, n, n];
        public bool visualiseChunks;
        public List<float3> positions;
        public bool smoothened;


        [Range(0, 1)] public float brushSize;

        private void Awake()
        {
            chunks = new ChunkScript[size, size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    for (int k = 0; k < size; k++)
                    {
                        ChunkScript chunk = Instantiate(chunkPrefab);
                        chunk.transform.parent = transform;
                        // chunk.transform.localPosition = new Vector3((i - size/2), (j - size/2), (k - size/2));
                        chunk.transform.localPosition = new Vector3(i, j, k);
                        chunk.transform.localRotation = Quaternion.identity;
                        chunk.chunkManager = this;
                        chunk.index = new int[] { i, j, k };
                        chunks[i, j, k] = chunk;

                        if (!visualiseChunks) continue;
                        GameObject obj = Instantiate(visualiseChunkPrefab, transform);
                        obj.transform.localPosition = chunk.transform.localPosition;
                    }
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                SmoothMesh();
            }
        }


        public void SmoothMesh(int iterations = 1)
        {
            foreach (MeshFilter mf in GetComponentsInChildren<MeshFilter>())
            {
                Mesh smoothedMesh = MeshSmoother.SmoothMesh(mf.mesh, iterations);
                mf.mesh = smoothedMesh;
            }
            //MeshGenerator.instance.creatingMesh = false;
            smoothened = true;
        }

        public void SmoothMeshes()
        {
            var meshFilters = GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            int i = 0;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                meshFilters[i].gameObject.SetActive(false);
                i++;
            }
            transform.gameObject.AddComponent<MeshFilter>();
            transform.GetComponent<MeshFilter>().mesh = new Mesh();
            transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
            //Mesh[] meshes = new Mesh[mfs.Length];
            //int i = 0;
            //foreach (MeshFilter mf in mfs)
            //{
            //    meshes[i++] = mf.mesh;
            //}

            //Mesh[] newMeshes = MeshSmoother.SmoothMeshes(meshes, 5);
            //i = 0;
            //foreach (MeshFilter mf in mfs)
            //{
            //    mf.mesh = newMeshes[i++];
            //}


        }

        private void ReadCoords()
        {
            string path = "C:\\Users\\vrproject\\Documents\\CellexalVR\\cellexalvr\\Data\\BrainSlidesTest\\tsne.mds";

            using (StreamReader streamReader = new StreamReader(path))
            {
                string header = streamReader.ReadLine();
                while (!streamReader.EndOfStream)
                {
                    string[] coords = streamReader.ReadLine().Split(null);
                    AddDensity(int.Parse(coords[1]), int.Parse(coords[2]), int.Parse(coords[3]), 1);
                }
            }

            foreach (ChunkScript c in chunks)
            {
                c.updateVertices();
                c.updateTriangles();
            }
        }

        public void AddSphericalDensity(Vector3 pos)
        {
            pos = transform.InverseTransformPoint(pos);
            HashSet<ChunkScript> chunksToUpdate = new HashSet<ChunkScript>();
            for (float i = -0.5f; i < 1; i++)
            {
                for (float j = -0.5f; j < 1; j++)
                {
                    for (float k = -0.5f; k < 1; k++)
                    {
                        Vector3 temp = pos + new Vector3(i, j, k);
                        if (temp.x < 0 || temp.x > size || temp.y < 0 || temp.y > size || temp.z < 0 || temp.z > size)
                            continue;
                        chunksToUpdate.Add(GetChunkFromPos(temp));
                    }
                }
            }

            pos = pos * (chunkResolution - 1);
            int rad = (int)(((chunkResolution - 1) / 2) * brushSize);
            for (int i = 0; i < chunkResolution - 1; i++)
            {
                for (int j = 0; j < rad * 2; j++)
                {
                    for (int k = 0; k < rad * 2; k++)
                    {
                        int x = Mathf.RoundToInt(pos.x + i - rad);
                        int y = Mathf.RoundToInt(pos.y + j - rad);
                        int z = Mathf.RoundToInt(pos.z + k - rad);
                        float dist = Vector3.Distance(pos, new Vector3(x, y, z)) / rad;
                        AddDensity(x, y, z, Mathf.Clamp01(1.0f - dist));
                    }
                }
            }

            foreach (ChunkScript c in chunksToUpdate)
            {
                c.updateVertices();
                c.updateTriangles();
            }
        }

        private ChunkScript GetChunkFromPos(Vector3 pos)
        {
            return chunks[(int)pos.x, (int)pos.y, (int)pos.z];
        }

        public void UpdateMesh()
        {

        }

        public void ToggleSurfaceLevelandUpdateCubes(float i, ChunkScript[,,] chunks, Color color)
        {
            //surfaceLevel += i;
            //surfaceLevel = Mathf.Clamp01(surfaceLevel);
            //int chunksThisFrame = 0;
            foreach (ChunkScript c in chunks)
            {
                //StartCoroutine(c.UpdateVertices());
                //c.AddVerticesToMesh();
                //c.updateVertices();
                //c.updateTriangles();
                //MeshFilter mf = c.GetComponent<MeshFilter>();
                //color.a = 1;
                //c.GetComponent<Renderer>().material.color = color;
                //mf.mesh.RecalculateBounds();
                //mf.mesh.RecalculateNormals();
                //if (++chunksThisFrame % 2 == 0) yield return null;
                c.AddVerticesToMesh();
                c.updateVertices();
                c.updateTriangles();
                MeshFilter mf = c.GetComponent<MeshFilter>();
                //color.a = 1;
                //c.GetComponent<Renderer>().material.color = color;
                mf.mesh.RecalculateBounds();
                mf.mesh.RecalculateNormals();
                float scale = 1f / transform.localScale.x;
                c.transform.localPosition = Vector3.one * -0.5f * scale;
            }

            if (!smoothened)
            {
            }
            //SmoothMesh();

            MeshGenerator.instance.creatingMesh = false;
        }

        public void CreateSphereDensity()
        {
            Vector3 centre = Vector3.one * (n - 1) * 0.5f;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    for (int k = 0; k < n; k++)
                    {
                        float dist = Vector3.Distance(centre, new Vector3(i, j, k)) / ((n - 1) / 2);
                        AddDensity(i, j, k, Mathf.Clamp01(1.0f - dist));
                    }
                }
            }
        }

        public void AddDensity(int x, int y, int z, float d)
        {
            if (x < 0 || x >= n || y < 0 || y >= n || z < 0 || z >= n)
            {
                return;
            }
            density[x, y, z] += d;
            if (!addDensitySpheres) return;
            if (densitySpheres[x, y, z] == null)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                DensitySphere ds = sphere.AddComponent<DensitySphere>();
                ds.chunkManager = this;
                ds.transform.SetParent(sphereHolder);
                ds.transform.localPosition = new Vector3(x, y, z) / (chunkResolution - 1);
                ds.transform.localScale = Vector3.one / ((ChunkManager.chunkResolution - 1) * 3) * density[x, y, z];
                ds.index[0] = x;
                ds.index[1] = y;
                ds.index[2] = z;
                densitySpheres[x, y, z] = sphere;
            }
            densitySpheres[x, y, z].GetComponent<Renderer>().material.color = new Color(1 - density[x, y, z], 1 - density[x, y, z], 1 - density[x, y, z], density[x, y, z]);
        }

        public void SetDensity(int x, int y, int z, float d, bool addSphere = false)
        {
            if (x < 0 || x >= n || y < 0 || y >= n || z < 0 || z >= n)
                return;
            density[x, y, z] = d;
            if (!addSphere) return;
            if (densitySpheres[x, y, z] == null)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(sphereHolder);
                sphere.transform.localPosition = new Vector3(x, y, z) / (chunkResolution - 1);
                sphere.transform.localScale = Vector3.one / ((ChunkManager.chunkResolution - 1) * 3) * d;
                densitySpheres[x, y, z] = sphere;
            }

            densitySpheres[x, y, z].GetComponent<Renderer>().material.color = new Color(1 - d, 1 - d, 1 - d, d);
        }

        public void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position + transform.localScale * size * 0.5f, transform.localScale * size);
        }
    }
}