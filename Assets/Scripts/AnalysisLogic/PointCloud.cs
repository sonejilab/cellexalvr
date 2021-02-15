using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.SceneObjects;
using CellexalVR;
using CellexalVR.General;
using CellexalVR.Interaction;
using DefaultNamespace;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.VFX;
using Valve.VR.InteractionSystem;

namespace AnalysisLogic
{
    public class PointCloud : MonoBehaviour
    {
        private VisualEffect vfx;
        private int pointCount;
        private bool spawn = true;
        private EntityManager entityManager;
        private int frameCount;
        private InteractableObjectBasic interactableObjectBasic;

        public Texture2D positionTextureMap;
        public float3 minCoordValues;
        public float3 maxCoordValues;
        public float3 longestAxis;
        public float3 scaledOffset;
        public float3 diffCoordValues;
        public Dictionary<string, float3> points = new Dictionary<string, float3>();
        public int pcID;
        public Transform selectionSphere;
        public Entity parent;

        private EntityArchetype entityArchetype;
        
        public void Initialize(int id)
        {
            minCoordValues = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            maxCoordValues = new float3(float.MinValue, float.MinValue, float.MinValue);
            pcID = id;
            gameObject.name = "PointCloud" + pcID;
            vfx = GetComponent<VisualEffect>();
            selectionSphere = SelectionToolCollider.instance.transform;
            vfx.pause = true;
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            interactableObjectBasic = GetComponent<InteractableObjectBasic>();
            entityArchetype = entityManager.CreateArchetype(typeof(Translation), typeof(Point));
        }

        private void Update()
        {

            // if (frameCount >= 50)
            // {
            //     print(vfx.aliveParticleCount);
            //     frameCount = 0;
            //     // vfx.SetTexture("ColorMapTex", colorMap);
            // }
            // frameCount++;


            vfx.SetVector3("SelectionPosition", selectionSphere.position);
            vfx.SetFloat("SelectionRadius", selectionSphere.localScale.x / 2f);
            vfx.SetVector3("CullingCubePos", PointCloudCulling.instance.transform.position);
            vfx.SetVector3("CullingCubeScale", PointCloudCulling.instance.transform.localScale);
        }

        public IEnumerator CreatePositionTextureMap(List<float3> pointPositions)
        {
            yield return null;
            pointCount = pointPositions.Count;
            // NativeArray<Entity> entities = new NativeArray<Entity>(pointCount, Allocator.Temp);
            // entityManager.CreateEntity(entityArchetype, entities);
            vfx.SetInt("SpawnRate", pointCount);
            int width = (int) math.ceil(math.sqrt(pointPositions.Count));
            int height = width;
            positionTextureMap = new Texture2D(width, height, TextureFormat.RGBAFloat, true, true);
            Color[] positions = new Color[width * height];
            Color col;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int ind = x + (height * y);
                    if (ind >= pointPositions.Count) continue;
                    float3 pos = pointPositions[ind] + 0.5f;
                    col = new Color(pos.x, pos.y, pos.z, 1);
                    // Entity e = entities[ind];
                    Entity e = entityManager.Instantiate(PrefabEntities.prefabEntity);
                    float3 wPos = math.transform(transform.localToWorldMatrix, pos);
                    entityManager.SetComponentData(e, new Translation {Value = wPos});
                    entityManager.AddComponent(e, typeof(Point));
                    entityManager.SetComponentData(e, new Point
                    {
                        selected = false,
                        xindex = x,
                        yindex = y,
                        label = ind,
                        offset = pos,
                        parentID = pcID
                    });
                    positions[ind] = col;
                    // positionTextureMap.SetPixel(x, y, col);
                }
                
                //if (y % 10 == 0) yield return null;

                // print(y);
                // yield return null;
            }

            positionTextureMap.SetPixels(positions);
            // World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<QuadrantSystem>().SetHashMap(pcID);
            // entities.Dispose();
            positionTextureMap.Apply();
            vfx.enabled = true;
            vfx.SetTexture("PositionMapTex", positionTextureMap);
            vfx.pause = false;
            // yield return new WaitForSeconds(1.5f);
            vfx.Stop();
            vfx.Play();
            // vfx.SetInt("SpawnRate", 0);
            GC.Collect();
        }

        //
        // public void UpdateColorTexture(int x, int y)
        // {
        //     colorMap.SetPixel(x, y, Color.red);
        //     colorMap.Apply();
        //     vfx.SetTexture("ColorMapTex", colorMap);
        // }

        public void AddGraphPoint(string cellName, float x, float y, float z)
        {
            points[cellName] = new float3(x, y, z);
            UpdateMinMaxCoords(x, y, z);
            // if (cells.Contains(cellName)) return;
            // PointSpawner.instance.cells.Add(cellName);
        }

        public void UpdateMinMaxCoords(float x, float y, float z)
        {
            if (x < minCoordValues.x)
                minCoordValues.x = x;
            if (y < minCoordValues.y)
                minCoordValues.y = y;
            if (z < minCoordValues.z)
                minCoordValues.z = z;
            if (x > maxCoordValues.x)
                maxCoordValues.x = x;
            if (y > maxCoordValues.y)
                maxCoordValues.y = y;
            if (z > maxCoordValues.z)
                maxCoordValues.z = z;
        }
    }
}