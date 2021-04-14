using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using AnalysisLogic;
using CellexalVR;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Interaction;
using Unity.Collections;
// using CellexalVR.AnalysisLogic;
// using CellexalVR.AnalysisObjects;
// using CellexalVR.General;
using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.VFX;
using Random = Unity.Mathematics.Random;
using CellexalVR.Spatial;
using System.Diagnostics;

namespace DefaultNamespace
{
    public struct Point : IComponentData
    {
        public bool selected;
        public int orgXIndex;
        public int orgYIndex;
        public int xindex;
        public int yindex;
        public float3 offset;
        public int parentID;
        public int label;
    }

    public struct PointCloudComponent : IComponentData
    {
        public int pointCloudId;
    }

    public class PointCloudGenerator : MonoBehaviour
    {
        public static PointCloudGenerator instance;
        [SerializeField] public Material material;
        [SerializeField] public Mesh mesh;
        [HideInInspector] public int nrOfGraphs = 0;
        public int mdsFileCount;
        public bool creatingGraph;
        public GameObject parentPrefab;
        public float3 minCoordValues;
        public float3 maxCoordValues;
        public float3 longestAxis;
        public Texture2D colorMap;
        public Texture2D alphaMap;
        public Texture2D clusterMap;
        public Dictionary<int, string> clusterDict = new Dictionary<int, string>();
        public Dictionary<string, Color> colorDict = new Dictionary<string, Color>();
        public Dictionary<string, List<Vector2>> clusters = new Dictionary<string, List<Vector2>>();
        public Dictionary<string, float3> scaledCoordinates = new Dictionary<string, float3>();
        public List<PointCloud> pointClouds = new List<PointCloud>();

        public int pointCount;

        public float3 scaledOffset;
        public const int textureWidth = 1000;
        // public List<string> cells = new List<string>();

        private float3 diffCoordValues;
        private Dictionary<string, float3> points = new Dictionary<string, float3>();
        private int graphNr;
        private Random random;
        private float spawnTimer;
        private EntityManager entityManager;
        private EntityArchetype entityArchetype;
        private QuadrantSystem quadrantSystem;
        private bool readingFile;
        private int maxPointCount;

        private void Awake()
        {
            instance = this;
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            quadrantSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<QuadrantSystem>();
            CreateParentArchetype();
        }

        public PointCloud CreateNewPointCloud()
        {
            points.Clear();
            scaledCoordinates.Clear();
            minCoordValues = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            maxCoordValues = new float3(float.MinValue, float.MinValue, float.MinValue);
            PointCloud pc = Instantiate(parentPrefab, new float3(graphNr, 1, graphNr), quaternion.identity).GetComponent<PointCloud>();
            quadrantSystem.graphParentTransforms.Add(pc.transform);
            quadrantSystem.graphParentTransforms[nrOfGraphs] = pc.transform;
            pc.Initialize(nrOfGraphs);
            nrOfGraphs++;
            return pc;
        }

        public PointCloud CreateFromOld(Transform oldPc)
        {
            points.Clear();
            scaledCoordinates.Clear();
            minCoordValues = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            maxCoordValues = new float3(float.MinValue, float.MinValue, float.MinValue);
            PointCloud pc = Instantiate(parentPrefab, oldPc.position, oldPc.rotation).GetComponent<PointCloud>();
            quadrantSystem.graphParentTransforms.Add(pc.transform);
            quadrantSystem.graphParentTransforms[nrOfGraphs] = pc.transform;
            pc.Initialize(nrOfGraphs);
            nrOfGraphs++;
            return pc;
        }

        public void BuildSlices(Transform oldPc, GraphSlice[] newSlices)
        {
            StartCoroutine(BuildSlicesCoroutine(oldPc, newSlices));
        }

        private IEnumerator BuildSlicesCoroutine(Transform oldPc, GraphSlice[] newSlices)
        {
            GraphSlice parentSlice = oldPc.GetComponent<GraphSlice>();
            GraphSlice[] oldSlices = parentSlice.childSlices.ToArray();
            maxPointCount = newSlices.ToList().Max(x => x.points.Count);
            for (int i = 0; i < newSlices.Length; i++)
            {
                instance.creatingGraph = true;
                GraphSlice slice = newSlices[i];
                slice.BuildPointCloud(oldPc);
                while (instance.creatingGraph)
                {
                    yield return null;
                }
                parentSlice.childSlices.Add(slice);
            }
            foreach (GraphSlice slice in oldSlices)
            {
                parentSlice.childSlices.Remove(slice);
                Destroy(slice.gameObject);
            }

        }

        public void AddGraphPoint(string cellName, float x, float y, float z)
        {
            points[cellName] = new float3(x, y, z);
            UpdateMinMaxCoords(x, y, z);
        }

        private void UpdateMinMaxCoords(float x, float y, float z)
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

        private void ScaleAllCoordinates()
        {
            diffCoordValues = maxCoordValues - minCoordValues;
            longestAxis = math.max(diffCoordValues.x, math.max(diffCoordValues.y, diffCoordValues.z));
            scaledOffset = (diffCoordValues / longestAxis) / 2;
        }

        /// <summary>
        /// Scales this graph point's coordinates from the graph's original scale to it's desired scale.
        /// </summary>
        private float3 ScaleCoordinate(string cellName)
        {
            // move one of the graph's corners to origo
            float3 scaledCoord = points[cellName] - minCoordValues;
            scaledCoord /= longestAxis;
            scaledCoord -= scaledOffset;
            if (!scaledCoordinates.ContainsKey(cellName))
            {
                scaledCoordinates[cellName] = scaledCoord + 0.5f;
            }

            return scaledCoord;
        }

        private void CreateParentArchetype()
        {
            entityArchetype = entityManager.CreateArchetype(
                typeof(LocalToWorld),
                typeof(LinkedEntityGroup),
                typeof(Translation),
                typeof(Rotation),
                typeof(Scale),
                typeof(PointCloudComponent)
            );
        }

        public void SpawnPoints(PointCloud pc, PointCloud parentPC, List<Point> points)
        {
            Entity parent = entityManager.CreateEntity(entityArchetype);
            entityManager.SetComponentData(parent, new Translation { Value = new float3(0, 0, 0) });
            entityManager.SetComponentData(parent, new Rotation { Value = new quaternion(0, 0, 0, 0) });
            entityManager.SetComponentData(parent, new Scale { Value = 1f });
            PointCloudComponent pointCloudComponent = new PointCloudComponent { pointCloudId = graphNr };
            entityManager.SetComponentData(parent, pointCloudComponent);
            ScaleAllCoordinates();
            graphNr++;
            pointClouds.Add(pc);
            pointCount = points.Count;
            pc.CreatePositionTextureMap(points, parentPC);
            StartCoroutine(CreateColorTextureMap(points, pc, parentPC));
        }

        public void SpawnPoints(PointCloud pc)
        {
            Entity parent = entityManager.CreateEntity(entityArchetype);
            entityManager.SetComponentData(parent, new Translation { Value = new float3(0, 0, 0) });
            entityManager.SetComponentData(parent, new Rotation { Value = new quaternion(0, 0, 0, 0) });
            entityManager.SetComponentData(parent, new Scale { Value = 1f });
            LocalToWorld localToWorld = entityManager.GetComponentData<LocalToWorld>(parent);
            PointCloudComponent pointCloudComponent = new PointCloudComponent { pointCloudId = graphNr };
            entityManager.SetComponentData(parent, pointCloudComponent);
            instance.creatingGraph = true;
            ScaleAllCoordinates();
            pc.minCoordValues = minCoordValues;
            pc.maxCoordValues = maxCoordValues;
            pc.scaledOffset = scaledOffset;
            pc.longestAxis = longestAxis;

            pc.SetCollider();
            graphNr++;
            pointClouds.Add(pc);
            foreach (KeyValuePair<string, float3> pointPair in points)
            {
                float3 pos = ScaleCoordinate(pointPair.Key);
            }

            pointCount = scaledCoordinates.Count;

            pc.CreatePositionTextureMap(scaledCoordinates.Values.ToList());


            // StartCoroutine(pc.CreatePositionTextureMap(scaledCoordinates.Values.ToList()));
            // if (nrOfGraphs == mdsFileCount)
            // {
            //     StartCoroutine(CreateColorTextureMap(scaledCoordinates.Values.ToList().Count));
            // }

            points.Clear();
            scaledCoordinates.Clear();
        }


        public IEnumerator CreateColorTextureMap(List<Point> points, PointCloud pc, PointCloud parentCloud)
        {
            while (readingFile) yield return null;
            int width = textureWidth;//(int)math.ceil(math.sqrt(pointCount));
            int height = (int)math.ceil(maxPointCount / (float)textureWidth);//width;
            //int height = parentCloud.positionTextureMap.height;
            colorMap = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            alphaMap = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            pc.colorTextureMap = colorMap;
            pc.alphaTextureMap = alphaMap;

            clusterMap = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            Color c;
            Color alpha = new Color(0.55f, 0f, 0);
            Color[] carray = new Color[width * height];
            Color[] aarray = new Color[width * height];
            alphaMap.SetPixels(0, 0, width, height, aarray);

            Texture2D parentTexture = (Texture2D)parentCloud.GetComponent<VisualEffect>().GetTexture("ColorMapTex");
            for (int i = 0; i < points.Count; i++)
            {
                Point p = points[i];
                c = parentTexture.GetPixel(p.xindex, p.yindex);
                carray[i] = c;
                aarray[i] = alpha;
            }

            colorMap.SetPixels(carray);
            alphaMap.SetPixels(aarray);
            //clusterMap.SetPixels(carrayClusters);
            CellexalLog.Log("Color Texture Map created");
            colorMap.Apply();
            alphaMap.Apply();
            clusterMap.Apply();
            VisualEffect vfx = pc.GetComponent<VisualEffect>();
            vfx.enabled = true;
            vfx.Play();
            vfx.SetTexture("ColorMapTex", colorMap);
            vfx.SetTexture("AlphaMapTex", alphaMap);
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<QuadrantSystem>().SetHashMap(pc.pcID);
            TextureHandler.instance.colorTextureMaps.Add(colorMap);
            TextureHandler.instance.alphaTextureMaps.Add(alphaMap);

            //EntityManager.DestroyEntity(GetEntityQuery(typeof(Point)));
        }


        public IEnumerator CreateColorTextureMap()
        {
            while (readingFile) yield return null;
            int width = textureWidth;//(int)math.ceil(math.sqrt(pointCount));
            int height = (int)math.ceil(pointCount / (float)textureWidth);//width;
            colorMap = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            alphaMap = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            clusterMap = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            Color c = new Color(0.32f, 0.32f, 0.32f);
            Color alpha = new Color(0.55f, 0f, 0);
            Color[] carray = new Color[width * height];
            Color[] aarray = new Color[width * height];
            Color[] carrayClusters = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int ind = x + (width * y);
                    if (ind >= pointCount)
                    {
                        aarray[ind] = Color.black;
                        continue;
                    }

                    carray[ind] = c;
                    aarray[ind] = alpha;
                    if (colorDict.Count > 0)
                    {
                        Color cluster = colorDict[clusterDict[ind]];
                        carrayClusters[ind] = cluster;
                        clusters[clusterDict[ind]].Add(new Vector2(x, y));
                    }
                }
            }

            colorMap.SetPixels(carray);
            alphaMap.SetPixels(aarray);
            clusterMap.SetPixels(carrayClusters);
            CellexalLog.Log("Color Texture Map created");
            colorMap.Apply();
            alphaMap.Apply();
            clusterMap.Apply();
            foreach (PointCloud pc in pointClouds)
            {
                VisualEffect vfx = pc.GetComponent<VisualEffect>();
                pc.colorTextureMap = colorMap;
                vfx.enabled = true;
                vfx.Play();
                vfx.SetTexture("ColorMapTex", colorMap);
                vfx.SetTexture("AlphaMapTex", alphaMap);
                World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<QuadrantSystem>().SetHashMap(pc.pcID);
                TextureHandler.instance.alphaTextureMaps.Add(alphaMap);
            }


            clusterDict.Clear();

            TextureHandler.instance.colorTextureMaps.Add(colorMap);
            TextureHandler.instance.mainColorTextureMaps.Add(colorMap);
            TextureHandler.instance.clusterTextureMaps.Add(clusterMap);

            instance.creatingGraph = false;
        }


        private void ReadColorMapFromFile()
        {
            string path = Directory.GetCurrentDirectory() + "\\Data\\" + CellexalUser.DataSourceFolder;
            string[] files = Directory.GetFiles(path, "*.csv");
            if (files.Length == 0) return;
            using (StreamReader sr = new StreamReader(files[0]))
            {
                string header = sr.ReadLine();

                while (!sr.EndOfStream)
                {
                    string[] words = sr.ReadLine().Split(',');
                    ColorUtility.TryParseHtmlString(words[1], out Color c);
                    colorDict[words[0]] = c;
                }
            }
            SelectionToolCollider.instance.Colors = colorDict.Values.ToArray();
            CellexalConfig.Config.SelectionToolColors = colorDict.Values.ToArray();

        }


        public IEnumerator ReadMetaData(string dir)
        {
            readingFile = true;
            ReadColorMapFromFile();
            //colorDict = new Dictionary<string, Color>();

            int width = (int)math.ceil(math.sqrt(pointCount));
            int height = width;
            int id = 0;
            string[] metafiles = Directory.GetFiles(dir, "*metadata.csv");
            //UnityEngine.Debug.Log(metafiles[0]);
            if (metafiles.Length != 0)
            {
                int clusterCount = 0;
                clusterDict = new Dictionary<int, string>();
                clusterMap = new Texture2D(width, height, TextureFormat.RGBAFloat, true, true);
                Color c;
                using (StreamReader sr = new StreamReader(metafiles[0]))
                {
                    string header = sr.ReadLine();
                    while (!sr.EndOfStream)
                    {
                        if (id % 10000 == 0) yield return null;
                        //string[] words = sr.ReadLine().Split(',');
                        // If no cell name column. Otherwise change below.
                        string cluster = sr.ReadLine(); // words[0];
                        clusterDict[id++] = cluster;
                        if (!clusters.ContainsKey(cluster))
                        {
                            if (colorDict.Count == 0)
                            {
                                if (clusterCount >= SelectionToolCollider.instance.Colors.Length - 1)
                                {
                                    c = UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);
                                    ReferenceManager.instance.settingsMenu.AddSelectionColor(c);
                                }
                                else
                                {
                                    c = SelectionToolCollider.instance.Colors[clusterCount];
                                }

                                c.a = 1;
                                colorDict[cluster] = c;
                            }
                            clusters[cluster] = new List<Vector2>();
                            clusterCount++;
                        }
                    }
                }

                ReferenceManager.instance.attributeSubMenu.CreateButtons(colorDict.Keys.ToArray());
                ReferenceManager.instance.attributeSubMenu.SwitchButtonStates(true);
                CellexalLog.Log("Meta data read");
            }

            readingFile = false;
        }
    }
}