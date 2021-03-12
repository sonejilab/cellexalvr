using System.Collections;
using System.Collections.Generic;
using CellexalVR;
using CellexalVR.Interaction;
using DefaultNamespace;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.VFX;

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
        private bool morphed;

        public Texture2D positionTextureMap;
        public Texture2D targetPositionTextureMap;
        public float3 minCoordValues;
        public float3 maxCoordValues;
        public float3 longestAxis;
        public float3 scaledOffset;
        public float3 diffCoordValues;
        public Dictionary<string, float3> points = new Dictionary<string, float3>();
        public int pcID;
        public Transform selectionSphere;
        public Entity parent;
        public List<float> zPositions = new List<float>();

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

            if (Input.GetKeyDown(KeyCode.Y))
            {
                StartCoroutine(Morph(!morphed));
                morphed = !morphed;
            }

            vfx.SetVector3("SelectionPosition", selectionSphere.position);
            vfx.SetFloat("SelectionRadius", selectionSphere.localScale.x / 2f);
            vfx.SetVector3("CullingCubePos", PointCloudCulling.instance.transform.position);
            vfx.SetVector3("CullingCubeScale", PointCloudCulling.instance.transform.localScale);
        }

        public IEnumerator Morph(bool toggle, float time = 1f)
        {
            float t = 0.0f;
            float min = toggle ? 1f : 0f;
            float max = toggle ? 0f : 1f;
            while (t <= 1f)
            {
                float val = math.lerp(min, max, t);
                vfx.SetFloat("morphStep", val);
                t += 0.8f * Time.deltaTime;
                yield return null;
            }
        }

        public void CreatePositionTextureMap(List<Point> points, PointCloud parentPC)
        {
            pointCount = points.Count;
            vfx.SetInt("SpawnRate", pointCount);
            int width = (int)math.ceil(math.sqrt(points.Count));
            int height = width;
            positionTextureMap = new Texture2D(width, height, TextureFormat.RGBAFloat, true, true);
            targetPositionTextureMap = new Texture2D(width, height, TextureFormat.RGBAFloat, true, true);
            Texture2D parentTextureMap = parentPC.targetPositionTextureMap;
            Color[] positions = new Color[width * height];
            Color[] targetPositions = new Color[positions.Length];
            for (int i = 0; i < points.Count; i++)
            {
                Point p = points[i];
                Color c = parentTextureMap.GetPixel(p.xindex, p.yindex);
                targetPositions[i] = c;
            }

            Color col;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int ind = x + (height * y);
                    if (ind >= points.Count) continue;
                    float3 pos = points[ind].offset;
                    col = new Color(pos.x, pos.y, pos.z, 1);
                    Entity e = entityManager.Instantiate(PrefabEntities.prefabEntity);
                    float3 wPos = math.transform(transform.localToWorldMatrix, pos);
                    entityManager.SetComponentData(e, new Translation { Value = wPos });
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
                }
            }
            targetPositionTextureMap.SetPixels(targetPositions);
            positionTextureMap.SetPixels(positions);
            positionTextureMap.Apply();
            targetPositionTextureMap.Apply();
            vfx.enabled = true;
            vfx.SetTexture("PositionMapTex", positionTextureMap);
            vfx.SetTexture("TargetPosMapTex", targetPositionTextureMap);
            vfx.pause = false;
        }


        public void CreatePositionTextureMap(List<float3> pointPositions, bool createEntities = true)
        {
            pointCount = pointPositions.Count;
            vfx.SetInt("SpawnRate", pointCount);
            int width = (int)math.ceil(math.sqrt(pointPositions.Count));
            int height = width;
            positionTextureMap = new Texture2D(width, height, TextureFormat.RGBAFloat, true, true);
            Color[] positions = new Color[width * height];
            Color col;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int ind = x + (height * y);
                    if (ind >= pointCount) continue;
                    float3 pos = pointPositions[ind];
                    col = new Color(pos.x, pos.y, pos.z, 1);
                    if (createEntities)
                    {
                        Entity e = entityManager.Instantiate(PrefabEntities.prefabEntity);
                        float3 wPos = math.transform(transform.localToWorldMatrix, pos);
                        entityManager.SetComponentData(e, new Translation { Value = wPos });
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
                    }
                    positions[ind] = col;
                }
            }

            positionTextureMap.SetPixels(positions);
            positionTextureMap.Apply();
            vfx.enabled = true;
            vfx.SetTexture("PositionMapTex", positionTextureMap);
            vfx.pause = false;
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