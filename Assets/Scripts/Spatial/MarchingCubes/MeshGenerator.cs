using UnityEngine;
using System.Collections;
using System.IO;
using CellexalVR.MarchingCubes;
using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Valve.VR;
using Valve.VR.InteractionSystem;
using CellexalVR.Interaction;
using System.Threading;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// Represents a spatial graph that in turn consists of many slices. The spatial graph is the parent of the graph objects.
    /// </summary>
    public class MeshGenerator : MonoBehaviour
    {
        private GameObject contour;
        private Vector3 startPosition;
        private Rigidbody _rigidBody;
        private bool dispersing;
        private Vector3 positionBeforeDispersing;
        private Quaternion rotationBeforeDispersing;
        private bool meshCreated;
        private Dictionary<int, ChunkManager> meshDict = new Dictionary<int, ChunkManager>();
        private int frameCount;

        // public bool slicesActive;

        public bool generateMeshes;
        public GameObject chunkManagerPrefab;
        public GameObject contourParent;
        public static MeshGenerator instance;
        public Hand hand;
        public SteamVR_Action_Boolean grabPinch;
        public int size;
        public int res;
        public bool creatingMesh;
        public float neighbourDistance = 0.1f;
        public int nrOfNeighbours = 5;


        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.M))// && !meshCreated)
            //{
            //    CreateMesh();
            //}

            if (Input.GetKeyDown(KeyCode.K))// && !meshCreated)
            {
                GenerateMeshes();
            }
        }

        public void GenerateMeshes()
        {
            UpdateMesh(true);
        }

        private void UpdateMesh(bool removeOutliers = false)
        {
            creatingMesh = true;
            Color[] colors = TextureHandler.instance.colorTextureMaps[0].GetPixels();
            Color[] alphas = TextureHandler.instance.alphaTextureMaps[0].GetPixels();
            Color[] positions = PointCloudGenerator.instance.pointClouds[0].positionTextureMap.GetPixels();
            Dictionary<int, List<float3>> meshPositions = new Dictionary<int, List<float3>>();

            for (int i = 0; i < positions.Length; i++)
            {
                Color a = alphas[i];
                if (a.maxColorComponent > 0.8f)
                {
                    Color pos = positions[i];
                    Color c = colors[i];
                    int cInd = SelectionToolCollider.instance.GetColorIndex(c);
                    if (!meshPositions.ContainsKey(cInd))
                    {
                        meshPositions[cInd] = new List<float3>();
                    }
                    meshPositions[cInd].Add(new float3(pos.r - 0.5f, pos.g - 0.5f, pos.b - 0.5f));
                }
            }
            //Color currentColor = SelectionToolCollider.instance.GetCurrentColor();
            //int ind = SelectionToolCollider.instance.GetColorIndex(currentColor);
            //ChunkManager meshToUpdate = meshDict[ind];
            //meshToUpdate.UpdateMesh();
            //StartCoroutine(CreateMesh(chunkManager, meshPositions[0], SelectionToolCollider.instance.Colors[0]));
            //CreateMesh(meshPositions);
            //StartCoroutine(UpdateMeshCoroutine(meshPositions));
            //Thread t = new Thread(() =>
            //{
            //    UpdateMeshCoroutine(meshPositions, removeOutliers);
            //});
            //t.Start();
            //while (t.IsAlive)
            //{
            //    yield return null;
            //}
            StartCoroutine(UpdateMeshCoroutine(meshPositions, removeOutliers));
        }

        public void SmoothenMeshes()
        {
            foreach (ChunkManager chunkManager in meshDict.Values)
            {
                chunkManager.SmoothMesh();
            }
        }

        public void RemoveMesh(int id)
        {
            meshDict.TryGetValue(id, out ChunkManager chunkManager);
            if (chunkManager != null)
            {
                Destroy(chunkManager.gameObject);
                meshDict.Remove(id);
            }
        }

        public void ChangeMeshTransparency(float val)
        {
            foreach (KeyValuePair<int, ChunkManager> kvp in meshDict)
            {
                MeshRenderer mr = kvp.Value.GetComponentInChildren<MeshRenderer>();
                Color c = mr.material.color;
                c.a = val;
                mr.material.color = c;
            }
        }

        private IEnumerator UpdateMeshCoroutine(Dictionary<int, List<float3>> positions, bool removeOutliers = false)
        {
            foreach (int key in positions.Keys)
            {
                if (!meshDict.ContainsKey(key))
                {
                    ChunkManager chunkManager = Instantiate(chunkManagerPrefab).GetComponent<ChunkManager>();
                    chunkManager.transform.parent = GameObject.Find("BrainParent").transform;
                    chunkManager.transform.localScale = Vector3.one * 1.55f;
                    chunkManager.transform.localPosition = Vector3.zero;
                    chunkManager.transform.localRotation = Quaternion.identity;
                    //Color c = key == -1 ? Color.white : SelectionToolCollider.instance.Colors[key];
                    meshDict[key] = chunkManager;
                }

                if (removeOutliers)
                {
                    ChunkManager chunkManager = meshDict[key];
                    chunkManager.positions = positions[key];
                    yield return StartCoroutine(RemoveOutliers(chunkManager));
                }

                else
                {
                    meshDict[key].positions = positions[key];
                }
            }

            foreach (KeyValuePair<int, ChunkManager> kvp in meshDict)
            {
                Color c = kvp.Key == -1 ? Color.white : SelectionToolCollider.instance.Colors[kvp.Key];
                yield return StartCoroutine(CreateMesh(kvp.Value, c));
            }
        }

        public void CreateMesh(bool removeOutliers = false)
        {
            Color[] colors = TextureHandler.instance.colorTextureMaps[0].GetPixels();
            Color[] alphas = TextureHandler.instance.alphaTextureMaps[0].GetPixels();
            Color[] positions = PointCloudGenerator.instance.pointClouds[0].positionTextureMap.GetPixels();
            Dictionary<int, List<float3>> meshPositions = new Dictionary<int, List<float3>>();

            for (int i = 0; i < positions.Length; i++)
            {
                Color a = alphas[i];
                if (a.maxColorComponent > 0.8f)
                {
                    Color pos = positions[i];
                    Color c = colors[i];
                    int cInd = SelectionToolCollider.instance.GetColorIndex(c);
                    if (!meshPositions.ContainsKey(cInd))
                    {
                        meshPositions[cInd] = new List<float3>();
                    }
                    meshPositions[cInd].Add(new float3(pos.r - 0.5f, pos.g - 0.5f, pos.b - 0.5f));
                }
            }
            CreateMesh(meshPositions, removeOutliers);
        }

        private IEnumerator RemoveOutliers(ChunkManager chunkManager)
        {
            List<float3> newPositions = new List<float3>();
            Dictionary<int, int> neighbours = new Dictionary<int, int>();
            //List<Graph.GraphPoint> neighbours = points.FindAll(x => Vector3.Distance(centroid, x.Position) < distance);
            for (int i = 0; i < chunkManager.positions.Count; i++)
            {
                neighbours[i] = 0;
                float3 position = chunkManager.positions[i];
                for (int j = i + 1; j < chunkManager.positions.Count - 1; j++)
                {
                    neighbours[j] = 0;
                    if (Vector3.Distance(position, chunkManager.positions[j]) < neighbourDistance)
                    {
                        neighbours[i]++;
                        neighbours[j]++;
                    }
                }
                if (neighbours[i] > nrOfNeighbours)
                {
                    newPositions.Add(position);
                }
                else
                {
                    int x = (int)(position.x * 20f) + 10;
                    int y = (int)(position.y * 20f) + 10;
                    int z = (int)(position.z * 20f) + 10;
                    // int d = z > 5 && z < 7 ? 0 : 1; 
                    chunkManager.SetDensity(x, y, z, 0);
                }
                if (i % 50 == 0) yield return null;
            }

            chunkManager.positions = newPositions;

        }


        private void CreateMesh(Dictionary<int, List<float3>> positions, bool removeOutliers = false)
        {
            foreach (ChunkManager chunk in contourParent.GetComponentsInChildren<ChunkManager>())
            {
                Destroy(chunk.gameObject);
            }
            foreach (KeyValuePair<int, List<float3>> kvp in positions)
            {
                ChunkManager chunkManager = Instantiate(chunkManagerPrefab).GetComponent<ChunkManager>();
                chunkManager.transform.parent = GameObject.Find("BrainParent").transform;
                chunkManager.transform.localScale = Vector3.one * 1.55f;// 0.2f;
                chunkManager.transform.localPosition = Vector3.zero;
                chunkManager.transform.localRotation = Quaternion.identity;
                Color c = kvp.Key == -1 ? Color.white : SelectionToolCollider.instance.Colors[kvp.Key];
                meshDict[kvp.Key] = chunkManager;
                //chunkManager.positions = removeOutliers ? RemoveOutliers(chunkManager) : kvp.Value;
                //chunkManager.positions = kvp.Value;
                StartCoroutine(CreateMesh(chunkManager, c));
            }
            return /*chunkManager*/;
        }

        /// <summary>
        /// Create a mesh using the marching cubes algorithm. Read the coordinates and add a density value of one to each point.
        /// </summary>
        /// <returns></returns>
        private IEnumerator CreateMesh(ChunkManager chunkManager, Color color)
        {
            int i = 0;
            foreach (float3 position in chunkManager.positions)
            {
                int x = (int)(position.x * 20f) + 10;
                int y = (int)(position.y * 20f) + 10;
                int z = (int)(position.z * 20f) + 10;
                // int d = z > 5 && z < 7 ? 0 : 1; 
                chunkManager.SetDensity(x, y, z, 1);
                i++;
                if (i % 1000 == 0) yield return null;
            }

            StartCoroutine(chunkManager.ToggleSurfaceLevelandUpdateCubes(0, chunkManager.chunks, color));
            //creatingMesh = true;
            while (creatingMesh) yield return null;
            chunkManager.SmoothMesh();
            meshCreated = true;
            // ChunkManager.toggleSurfaceLevelandUpdateCubes(0, chunkManager.chunks);

            // foreach (MeshFilter mf in chunkManager.GetComponentsInChildren<MeshFilter>())
            // {
            //     // mf.mesh.Optimize();
            //     mf.mesh.RecalculateBounds();
            //     mf.mesh.RecalculateNormals();
            // }

            // GameObject obj  = Instantiate(contourParent);
            // chunkManager.transform.parent = obj.transform;
            // obj.transform.localScale = Vector3.one * 0.25f;
            // obj.transform.localPosition = new Vector3(0.5f, 1, 0.5f);
            // BoxCollider bc = obj.AddComponent<BoxCollider>();
            // bc.center = Vector3.one * 4;
            // bc.size = Vector3.one * 6;
        }
    }
}