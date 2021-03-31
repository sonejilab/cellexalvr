using System.Collections;
using System.Collections.Generic;
using CellexalVR;
using CellexalVR.Interaction;
using CellexalVR.Spatial;
using DefaultNamespace;
using Unity.Collections;
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
        private SlicerBox slicerBox;

        public Texture2D positionTextureMap;
        public Texture2D colorTextureMap;
        public Texture2D alphaTextureMap;
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
        public GraphSlice graphSlice;

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
            slicerBox = GetComponentInChildren<SlicerBox>(true);
            graphSlice = GetComponent<GraphSlice>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                StartCoroutine(Morph());
            }

            if (SelectionToolCollider.instance.selActive)
            {
                UpdateColorTexture();
            }

            //vfx.SetVector3("SelectionPosition", selectionSphere.position);
            //vfx.SetFloat("SelectionRadius", selectionSphere.localScale.x / 2f);
            //vfx.SetVector3("CullingCubePos", PointCloudCulling.instance.transform.position);
            //vfx.SetVector3("CullingCubeScale", PointCloudCulling.instance.transform.localScale);
        }

        public void SetCollider(bool offset = false)
        {
            diffCoordValues = maxCoordValues - minCoordValues;
            float3 mid = (minCoordValues + maxCoordValues) / 2;
            longestAxis = math.max(diffCoordValues.x, math.max(diffCoordValues.y, diffCoordValues.z));
            scaledOffset = (diffCoordValues / longestAxis) / 2;
            var bc = GetComponent<BoxCollider>();
            if (offset)
            {
                bc.center = mid;
            }
            bc.size = scaledOffset * 2;
            var slicerBox = GetComponentInChildren<SlicerBox>(true);
            if (slicerBox != null)
            {
                slicerBox.box.transform.localScale = bc.size;
                slicerBox.box.transform.localPosition = bc.center - Vector3.one * 0.5f;
            }
            //var sliceCube = GetComponentInChildren<GraphSliceCube>();
            //if (sliceCube != null)
            //{
            //    sliceCube.SetCube(scaledOffset, bc.center);
            //}
        }

        public void SetCollider(Vector3 mid, Vector3 size)
        {
            diffCoordValues = maxCoordValues - minCoordValues;
            longestAxis = math.max(diffCoordValues.x, math.max(diffCoordValues.y, diffCoordValues.z));
            scaledOffset = (diffCoordValues / longestAxis) / 2;
            var bc = GetComponent<BoxCollider>();
            bc.center = mid;
            bc.size = size;
            var slicerBox = GetComponentInChildren<SlicerBox>(true);
            if (slicerBox != null)
            {
                slicerBox.box.transform.localScale = bc.size;
                slicerBox.box.transform.localPosition = bc.center - Vector3.one * 0.5f;
            }
        }

        public IEnumerator Morph(float time = 1f)
        {
            print($"{slicerBox}, {morphed}");
            if (slicerBox != null)
            {
                slicerBox.box.SetActive(morphed);
            }
            morphed = !morphed;
            float t = 0.0f;
            float min = morphed ? 0f : 1f;
            float max = morphed ? 1f : 0f;
            while (t <= time)
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
            int width = 100;//(int)math.ceil(math.sqrt(pointCount));
            int height = (int)math.ceil(pointCount / 100f);//width;
            positionTextureMap = new Texture2D(width, height, TextureFormat.RGBAFloat, true, true);
            targetPositionTextureMap = new Texture2D(width, height, TextureFormat.RGBAFloat, true, true);
            Texture2D parentTargetTextureMap = parentPC.targetPositionTextureMap;
            Color[] positions = new Color[width * height];
            Color[] targetPositions = new Color[positions.Length];
            //for (int i = 0; i < points.Count; i++)
            //{
            //    Point p = points[i];
            //    Color c = parentTextureMap.GetPixel(p.xindex, p.yindex);
            //    targetPositions[i] = c;
            //}

            Color col;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int ind = x + (width * y);
                    if (ind >= pointCount) continue;
                    Point p = points[ind];
                    Color c = parentTargetTextureMap.GetPixel(p.xindex, p.yindex);
                    targetPositions[ind] = c;

                    float3 pos = p.offset;
                    col = new Color(pos.x, pos.y, pos.z, 1);
                    Entity e = entityManager.Instantiate(PrefabEntities.prefabEntity);
                    float3 wPos = math.transform(transform.localToWorldMatrix, pos);
                    entityManager.SetComponentData(e, new Translation { Value = wPos });
                    entityManager.AddComponent(e, typeof(Point));
                    Point newP = new Point();
                    newP.selected = false;
                    newP.orgXIndex = p.orgXIndex;
                    newP.orgYIndex = p.orgYIndex;
                    newP.xindex = x;
                    newP.yindex = y;
                    newP.label = p.label;
                    newP.offset = pos;
                    newP.parentID = pcID;
                    entityManager.SetComponentData(e, newP);
                    //{
                    //    selected = false,
                    //    orgXIndex = p.orgXIndex,
                    //    orgYIndex = p.orgYIndex,
                    //    xindex = x,
                    //    yindex = y,
                    //    label = p.label,
                    //    offset = pos,
                    //    parentID = pcID
                    //});
                    points[ind] = newP;
                    positions[ind] = col;
                }
                //if (y % 10 == 0) yield return null;
            }
            targetPositionTextureMap.SetPixels(targetPositions);
            positionTextureMap.SetPixels(positions);
            positionTextureMap.Apply();
            targetPositionTextureMap.Apply();
            vfx.enabled = true;
            vfx.SetTexture("PositionMapTex", positionTextureMap);
            vfx.SetTexture("TargetPosMapTex", targetPositionTextureMap);
            vfx.pause = false;
            PointCloudGenerator.instance.creatingGraph = false;
        }


        public void CreatePositionTextureMap(List<float3> pointPositions)
        {
            pointCount = pointPositions.Count;
            vfx.SetInt("SpawnRate", pointCount);
            int width = 100;//(int)math.ceil(math.sqrt(pointCount));
            int height = (int)math.ceil(pointCount / 100f);//width;
            positionTextureMap = new Texture2D(width, height, TextureFormat.RGBAFloat, true, true);
            Color[] positions = new Color[width * height];
            Color col;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int ind = x + (width * y);
                    if (ind >= pointCount) continue;
                    float3 pos = pointPositions[ind];
                    col = new Color(pos.x, pos.y, pos.z, 1);

                    Entity e = entityManager.Instantiate(PrefabEntities.prefabEntity);
                    float3 wPos = math.transform(transform.localToWorldMatrix, pos);
                    entityManager.SetComponentData(e, new Translation { Value = wPos });
                    entityManager.AddComponent(e, typeof(Point));
                    entityManager.SetComponentData(e, new Point
                    {
                        selected = false,
                        orgXIndex = x,
                        orgYIndex = y,
                        xindex = x,
                        yindex = y,
                        label = ind,
                        offset = pos,
                        parentID = pcID
                    });
                    positions[ind] = col;
                }
            }

            positionTextureMap.SetPixels(positions);
            positionTextureMap.Apply();
            vfx.enabled = true;
            vfx.SetTexture("PositionMapTex", positionTextureMap);
            vfx.pause = false;
            PointCloudGenerator.instance.creatingGraph = false;
        }

        //
        // public void UpdateColorTexture(int x, int y)
        // {
        //     colorMap.SetPixel(x, y, Color.red);
        //     colorMap.Apply();
        //     vfx.SetTexture("ColorMapTex", colorMap);
        // }
        public void UpdateColorTexture()
        {
            // test speed and maybe make into couroutine..
            if (graphSlice.points.Count > 0)
            {
                Color[] carray = colorTextureMap.GetPixels();
                Color[] aarray = new Color[carray.Length];
                Texture2D parentTexture = graphSlice.parentSlice.pointCloud.colorTextureMap;
                Texture2D parentATexture = graphSlice.parentSlice.pointCloud.alphaTextureMap;
                for (int i = 0; i < graphSlice.points.Count; i++)
                {
                    Point p = graphSlice.points[i];
                    int ind = (p.yindex) * 100 + (p.xindex);
                    carray[ind] = parentTexture.GetPixel(p.orgXIndex, p.orgYIndex);
                    aarray[ind] = parentATexture.GetPixel(p.orgXIndex, p.orgYIndex);
                }

                colorTextureMap.SetPixels(carray);
                colorTextureMap.Apply();
            }
        }


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