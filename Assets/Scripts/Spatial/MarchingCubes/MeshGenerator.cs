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
        private Dictionary<int, Vector3> centroids = new Dictionary<int, Vector3>();
        private bool spreadOut;

        public SteamVR_Action_Boolean controllerAction = SteamVR_Input.GetBooleanAction("Teleport");
        public bool generateMeshes;
        public GameObject chunkManagerPrefab;
        public AllenReferenceBrain contourParent;
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

            //if (Input.GetKeyDown(KeyCode.K))// && !meshCreated)
            //{
            //    GenerateMeshes();
            //}

            //if (Input.GetKeyDown(KeyCode.J))
            //{
            //    StartCoroutine(SpreadOutParts());
            //}
            if (controllerAction.GetStateDown(Player.instance.leftHand.handType))
            {
                StartCoroutine(SpreadOutParts());
                //SpreadOutPoints();
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
                yield return StartCoroutine(CreateMesh(kvp.Key, c));
            }
        }

        public void CreateMesh()
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
            CreateMesh(meshPositions);
        }

        private IEnumerator RemoveOutliers(ChunkManager chunkManager)
        {
            List<float3> newPositions = new List<float3>();
            Dictionary<int, int> neighbours = new Dictionary<int, int>();
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
                    chunkManager.SetDensity(x, y, z, 0);
                }
                if (i % 50 == 0) yield return null;
            }

            chunkManager.positions = newPositions;

        }


        private void CreateMesh(Dictionary<int, List<float3>> positions)
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
                StartCoroutine(CreateMesh(kvp.Key, c));
            }
            return /*chunkManager*/;
        }

        /// <summary>
        /// Create a mesh using the marching cubes algorithm. Read the coordinates and add a density value of one to each point.
        /// </summary>
        /// <returns></returns>
        private IEnumerator CreateMesh(int key, Color color)
        {
            ChunkManager chunkManager = meshDict[key];
            int i = 0;
            float3 centroid = float3.zero;
            foreach (float3 position in chunkManager.positions)
            {
                centroid += position;
                int x = (int)(position.x * 20f) + 10;
                int y = (int)(position.y * 20f) + 10;
                int z = (int)(position.z * 20f) + 10;
                // int d = z > 5 && z < 7 ? 0 : 1; 
                chunkManager.SetDensity(x, y, z, 1);
                i++;
                if (i % 1000 == 0) yield return null;
            }

            centroid /= chunkManager.positions.Count;
            centroids[key] = centroid;

            StartCoroutine(chunkManager.ToggleSurfaceLevelandUpdateCubes(0, chunkManager.chunks, color));
            //creatingMesh = true;
            while (creatingMesh) yield return null;
            chunkManager.SmoothMesh();
            meshCreated = true;
        }

        private IEnumerator SpreadOutParts()
        {
            spreadOut = !spreadOut;
            float t = 0f;
            float animationTime = 1f;
            while (t < animationTime)
            {
                foreach (KeyValuePair<int, ChunkManager> meshPair in meshDict)
                {
                    Vector3 startPos = meshPair.Value.transform.localPosition;
                    Vector3 centroid = centroids[meshPair.Key];
                    if (spreadOut)
                    {
                        Vector3 targetPosition = (centroid - Vector3.zero).normalized * 0.5f;
                        float progress = Mathf.SmoothStep(0, animationTime, t);
                        meshPair.Value.transform.localPosition = Vector3.Lerp(startPos, targetPosition, progress);
                    }
                    else
                    {
                        Vector3 targetPosition = Vector3.zero;
                        float progress = Mathf.SmoothStep(0, animationTime, t);
                        meshPair.Value.transform.localPosition = Vector3.Lerp(startPos, targetPosition, progress);
                    }
                }
                yield return null;
                t += (Time.deltaTime / animationTime);
            }

        }
    }
}
