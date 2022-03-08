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

namespace CellexalVR.AnalysisLogic
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
        public Entity entity;
    }

    public struct PointCloudComponent : IComponentData
    {
        public int pointCloudId;
    }

    public class PointCloudGenerator : MonoBehaviour
    {
        public static PointCloudGenerator instance;
        [SerializeField] private PointCloud pointCloudPrefab;
        [SerializeField] private PointCloud spatialPointCloudPrefab;
        [HideInInspector] public int nrOfGraphs = 0;
        [HideInInspector] public int mdsFileCount;
        [HideInInspector] public bool creatingGraph;
        [HideInInspector] public float3 minCoordValues;
        [HideInInspector] public float3 maxCoordValues;
        [HideInInspector] public float3 longestAxis;
        [HideInInspector] public float3 scaledOffset;
        [HideInInspector] public Texture2D colorMap;
        [HideInInspector] public Texture2D alphaMap;
        [HideInInspector] public Texture2D clusterMap;
        [HideInInspector] public Dictionary<int, string> indToLabelDict = new Dictionary<int, string>();
        [HideInInspector] public Dictionary<int, string> clusterDict = new Dictionary<int, string>();
        [HideInInspector] public Dictionary<string, Color> colorDict = new Dictionary<string, Color>();
        [HideInInspector] public Dictionary<string, List<Vector2Int>> clusters = new Dictionary<string, List<Vector2Int>>();
        [HideInInspector] public Dictionary<string, float3> scaledCoordinates = new Dictionary<string, float3>();
        [HideInInspector] public List<PointCloud> pointClouds = new List<PointCloud>();

        public int pointCount;

        public const int textureWidth = 1000;
        public bool readingFile;
        // public List<string> cells = new List<string>();

        private float3 diffCoordValues;
        private Dictionary<string, float3> points = new Dictionary<string, float3>();
        private int graphNr;
        private Random random;
        private float spawnTimer;
        private EntityManager entityManager;
        private EntityArchetype entityArchetype;
        private QuadrantSystem quadrantSystem;
        private int maxPointCount;
        private int slicePrefabsUsed;
        //[SerializeField] private PointCloud[] slicePrefabs;

        private void Awake()
        {
            instance = this;
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            quadrantSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<QuadrantSystem>();
            CreateParentArchetype();
        }

        public PointCloud CreateNewPointCloud(bool spatial = false)
        {
            points.Clear();
            scaledCoordinates.Clear();
            minCoordValues = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            maxCoordValues = new float3(float.MinValue, float.MinValue, float.MinValue);
            PointCloud pc;
            if (spatial)
            {
                pc = Instantiate(spatialPointCloudPrefab, new float3(graphNr, 1, graphNr), quaternion.identity);
            }
            else
            {
                pc = Instantiate(pointCloudPrefab, new float3(graphNr, 1, graphNr), quaternion.identity);
            }
            quadrantSystem.graphParentTransforms.Add(pc.transform);
            quadrantSystem.graphParentTransforms[nrOfGraphs] = pc.transform;
            pc.Initialize(nrOfGraphs);
            nrOfGraphs++;
            return pc;
        }

        public HistoImage CreateNewHistoImage()
        {
            points.Clear();
            scaledCoordinates.Clear();
            minCoordValues = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            maxCoordValues = new float3(float.MinValue, float.MinValue, float.MinValue);
            //HistoImage hi = Instantiate(HistoImageHandler.instance.slicePrefab);
            PointCloud pc = Instantiate(spatialPointCloudPrefab);
            HistoImage hi = pc.gameObject.AddComponent<HistoImage>();
            quadrantSystem.graphParentTransforms.Add(pc.transform);
            quadrantSystem.graphParentTransforms[nrOfGraphs] = pc.transform;
            pc.Initialize(nrOfGraphs);
            nrOfGraphs++;
            return hi;
        }

        public PointCloud CreateFromOld(Transform oldPc, bool spatial = true)
        {
            points.Clear();
            scaledCoordinates.Clear();
            minCoordValues = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            maxCoordValues = new float3(float.MinValue, float.MinValue, float.MinValue);
            PointCloud pc;
            if (spatial)
            {
                pc = Instantiate(spatialPointCloudPrefab, oldPc.transform.position, quaternion.identity);
            }
            else
            {
                pc = Instantiate(pointCloudPrefab, oldPc.transform.position, quaternion.identity);
            }
            quadrantSystem.graphParentTransforms.Add(pc.transform);
            quadrantSystem.graphParentTransforms[nrOfGraphs] = pc.transform;
            pc.Initialize(nrOfGraphs);
            nrOfGraphs++;
            return pc;
        }

        public void BuildSlices(Transform oldPc, GraphSlice[] newSlices)
        {
            //StartCoroutine(BuildSlicesCoroutine(oldPc, new GraphSlice[] { newSlices[0], newSlices[1], newSlices[2] }));
            StartCoroutine(BuildSlicesCoroutine(oldPc, newSlices));
        }

        private IEnumerator BuildSlicesCoroutine(Transform oldPc, GraphSlice[] newSlices)
        {
            GraphSlice parentSlice = oldPc.GetComponent<GraphSlice>();
            GraphSlice[] oldSlices = parentSlice.childSlices.ToArray();
            foreach (GraphSlice oldSlice in oldSlices)
            {
                parentSlice.childSlices.Remove(oldSlice);
                Destroy(oldSlice.gameObject);
            }
            maxPointCount = newSlices.ToList().Max(x => x.points.Count);

            yield return new WaitForSeconds(0.2f);
            GraphSlice slice;
            for (int i = 0; i < newSlices.Length; i++)
            {
                instance.creatingGraph = true;
                slice = newSlices[i];
                yield return SpawnPoints(slice.pointCloud, parentSlice.pointCloud, slice.points);
                slice.gameObject.SetActive(true);
                slice.UpdateColorTexture();
                parentSlice.childSlices.Add(slice);
            }
            parentSlice.slicerBox.sliceAnimationActive = false;

            yield return new WaitForSeconds(0.6f);
            parentSlice.ActivateSlices(true);

        }


        public void AddGraphPoint(string cellName, float x, float y, float z)
        {
            points[cellName] = new float3(x, y, z);
            UpdateMinMaxCoords(x, y, z);
        }

        public void AddGraphPoint(string cellName, float x, float y)
        {
            points[cellName] = new float3(x, y, 0);
            UpdateMinMaxCoords(x, y, 0);
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
            maxCoordValues += new float3(0.01f, 0.01f, 0.01f);
            minCoordValues -= new float3(0.01f, 0.01f, 0.01f);
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

        public IEnumerator SpawnPoints(PointCloud pc, PointCloud parentPC, List<Point> points)
        {
            creatingGraph = false;
            pc.GetComponent<GraphSlice>().parentSlice = parentPC.GetComponent<GraphSlice>();
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
            yield return pc.CreatePositionTextureMap(points, parentPC);
            yield return CreateColorTextureMap(points, pc, parentPC);
        }

        public void SpawnPoints(PointCloud pc)
        {
            Entity parent = entityManager.CreateEntity(entityArchetype);
            entityManager.SetComponentData(parent, new Translation { Value = new float3(0, 0, 0) });
            entityManager.SetComponentData(parent, new Rotation { Value = new quaternion(0, 0, 0, 0) });
            entityManager.SetComponentData(parent, new Scale { Value = 1f });
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
                float3 scaledPos = ScaleCoordinate(pointPair.Key);
                if (!scaledCoordinates.ContainsKey(pointPair.Key))
                {
                    scaledCoordinates[pointPair.Key] = scaledPos;
                }

            }

            pointCount = scaledCoordinates.Count;
            //WriteToFile(pc.GraphName + ".txt");
            pc.CreatePositionTextureMap(scaledCoordinates.Values.ToList(), scaledCoordinates.Keys.ToList());
            points.Clear();
            scaledCoordinates.Clear();
        }


        private void WriteToFile(string fPath)
        {
            using (StreamWriter sr = new StreamWriter(fPath))
            {
                foreach (KeyValuePair<string, float3> kvp in scaledCoordinates)
                {
                    string line = kvp.Key + ", " + kvp.Value.x + ", " + kvp.Value.y + ", " + kvp.Value.z;
                    sr.WriteLine(line);
                }
            }
        }

        public void SpawnPoints(HistoImage hi, PointCloud parentPC)
        {
            PointCloud pc = hi.GetComponent<PointCloud>();
            Entity parent = entityManager.CreateEntity(entityArchetype);
            entityManager.SetComponentData(parent, new Translation { Value = new float3(0, 0, 0) });
            entityManager.SetComponentData(parent, new Rotation { Value = new quaternion(0, 0, 0, 0) });
            entityManager.SetComponentData(parent, new Scale { Value = 1f });
            LocalToWorld localToWorld = entityManager.GetComponentData<LocalToWorld>(parent);
            PointCloudComponent pointCloudComponent = new PointCloudComponent { pointCloudId = graphNr };
            entityManager.SetComponentData(parent, pointCloudComponent);
            instance.creatingGraph = true;
            hi.maxValues = maxCoordValues;
            hi.minValues = minCoordValues;
            hi.ScaleCoordinates();
            pc.minCoordValues = minCoordValues;
            pc.maxCoordValues = maxCoordValues;
            pc.scaledOffset = scaledOffset;
            pc.longestAxis = longestAxis;
            float maxX = 0f;
            float minX = 0f;
            float maxY = 0f;
            float minY = 0f;
            pc.SetCollider();
            graphNr++;
            pointClouds.Add(pc);
            foreach (KeyValuePair<string, float3> pointPair in points)
            {
                float3 pos = pointPair.Value;
                float3 scaledPos = hi.ScaleCoordinate(pos);
                scaledCoordinates[pointPair.Key] = scaledPos;
                if (scaledPos.x > maxX)
                {
                    maxX = scaledPos.x;
                    //maxInds[0] = ind;
                }

                if (scaledPos.x < minX)
                {
                    minX = scaledPos.x;
                    //maxInds[1] = ind;
                }
                if (scaledPos.y > maxY)
                {
                    maxY = scaledPos.y;
                    //maxInds[2] = ind;
                }
                if (scaledPos.y < minY)
                {
                    minY = scaledPos.y;
                    //maxInds[3] = ind;
                }
                pc.points[pointPair.Key] = pointPair.Value;
            }
            hi.scaledMaxValues = new Vector2(maxX, maxY);
            hi.scaledMinValues = new Vector2(minX, minY);
            pc.CreatePositionTextureMap(scaledCoordinates.Values.ToList(), scaledCoordinates.Keys.ToList());
            StartCoroutine(CreateColorTextureMap(points, hi, parentPC));
            points.Clear();
            scaledCoordinates.Clear();
        }


        public IEnumerator CreateColorTextureMap(Dictionary<string, float3> points, HistoImage hi, PointCloud parentCloud)
        {
            while (readingFile) yield return null;
            int width = textureWidth;//(int)math.ceil(math.sqrt(pointCount));
            int height = (int)math.ceil(3500 / (float)textureWidth);//width;
            //int height = parentCloud.positionTextureMap.height;
            PointCloud pc = hi.GetComponent<PointCloud>();
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
            foreach (KeyValuePair<string, float3> kvp in points)
            {
                Vector2Int textureCoord = TextureHandler.instance.textureCoordDict[kvp.Key];
                c = parentTexture.GetPixel(textureCoord.x, textureCoord.y);
                Vector2Int hiTexCoord = hi.textureCoords[kvp.Key];
                colorMap.SetPixel(hiTexCoord.x, hiTexCoord.y, c);
                alphaMap.SetPixel(hiTexCoord.x, hiTexCoord.y, alpha);
            }

            CellexalLog.Log("Color Texture Map created");
            colorMap.Apply();
            alphaMap.Apply();
            clusterMap.Apply();
            VisualEffect vfx = pc.GetComponentInChildren<VisualEffect>();
            vfx.SetTexture("ColorMapTex", colorMap);
            vfx.SetTexture("AlphaMapTex", alphaMap);
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<QuadrantSystem>().SetHashMap(pc.pcID);
            TextureHandler.instance.colorTextureMaps.Add(colorMap);
            TextureHandler.instance.alphaTextureMaps.Add(alphaMap);
            vfx.enabled = true;
            vfx.Stop();
            vfx.Play();
            instance.creatingGraph = false;
            //EntityManager.DestroyEntity(GetEntityQuery(typeof(Point)));
        }


        public IEnumerator CreateColorTextureMap(List<Point> points, PointCloud pc, PointCloud parentCloud)
        {
            while (readingFile) yield return null;
            int width = textureWidth;//(int)math.ceil(math.sqrt(pointCount));
            int height = (int)math.ceil(points.Count / (float)textureWidth);//width;
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
                if (i % 2000 == 0) yield return null;
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
            vfx.SetTexture("ColorMapTex", colorMap);
            vfx.SetTexture("AlphaMapTex", alphaMap);
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<QuadrantSystem>().SetHashMap(pc.pcID);
            TextureHandler.instance.colorTextureMaps.Add(colorMap);
            TextureHandler.instance.alphaTextureMaps.Add(alphaMap);
            vfx.enabled = true;
            vfx.Stop();
            vfx.Play();
            instance.creatingGraph = false;
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
            Color c = new Color(0.32f, 0.32f, 0.32f, 0f);
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
                    if (ind % 5000 == 0) yield return null;

                    carray[ind] = c;
                    aarray[ind] = alpha;
                    if (colorDict.Count > 0)
                    {
                        Color cluster = colorDict[clusterDict[ind]];
                        carrayClusters[ind] = cluster;
                        clusters[clusterDict[ind]].Add(new Vector2Int(x, y));
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
                if (vfx == null) vfx = pc.GetComponentInChildren<VisualEffect>();
                pc.colorTextureMap = colorMap;
                pc.alphaTextureMap = alphaMap;
                vfx.SetTexture("ColorMapTex", colorMap);
                vfx.SetTexture("AlphaMapTex", alphaMap);
                vfx.enabled = true;
                vfx.Stop();
                vfx.Play();
                World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<QuadrantSystem>().SetHashMap(pc.pcID);
            }


            //clusterDict.Clear();

            //var p = clusterMap.EncodeToPNG();
            //File.WriteAllBytes("clusterTest.png", p);

            TextureHandler.instance.colorTextureMaps.Add(colorMap);
            TextureHandler.instance.mainColorTextureMaps.Add(colorMap);
            TextureHandler.instance.clusterTextureMaps.Add(clusterMap);
            TextureHandler.instance.alphaTextureMaps.Add(alphaMap);

            instance.creatingGraph = false;
        }


        private void ReadColorMapFromFile()
        {
            string path = Directory.GetCurrentDirectory() + "\\Data\\" + CellexalUser.DataSourceFolder;
            string[] files = Directory.GetFiles(path, "color_codes.csv");
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
            //ReadColorMapFromFile();
            int width = textureWidth;//(int)math.ceil(math.sqrt(pointCount));
            int height = (int)math.ceil(pointCount / (float)textureWidth);//width;
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
                        if (id % 5000 == 0) yield return null;
                        // If no cell name column. Otherwise change below.
                        string[] words = sr.ReadLine().Split(',');
                        string cluster = words[words.Length - 1];
                        //string cluster = sr.ReadLine();
                        clusterDict[id++] = cluster;
                        if (!clusters.ContainsKey(cluster))
                        {
                            if (clusterCount >= CellexalConfig.Config.SelectionToolColors.Length - 1)
                            {
                                c = UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);
                                ReferenceManager.instance.settingsMenu.AddSelectionColor(c);
                            }
                            else
                            {
                                c = CellexalConfig.Config.SelectionToolColors[clusterCount];
                            }

                            c.a = 1;
                            colorDict[cluster] = c;
                            clusters[cluster] = new List<Vector2Int>();
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