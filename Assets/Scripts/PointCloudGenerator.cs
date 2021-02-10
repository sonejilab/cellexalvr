using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using AnalysisLogic;
using CellexalVR;
using CellexalVR.AnalysisObjects;
using CellexalVR.Interaction;
// using CellexalVR.AnalysisLogic;
// using CellexalVR.AnalysisObjects;
// using CellexalVR.General;
using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.VFX;
using Random = Unity.Mathematics.Random;

namespace DefaultNamespace
{
    public struct Point : IComponentData
    {
        public int group;
        public bool selected;
        public int xindex;
        public int yindex;
        public float3 offset;
        public int parentID;
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
        public Dictionary<int, string> clusterDict = new Dictionary<int, string>();
        public Dictionary<string, Color> colorDict = new Dictionary<string, Color>();

        public float3 scaledOffset;
        // public List<string> cells = new List<string>();

        private float3 diffCoordValues;
        private Dictionary<string, float3> points = new Dictionary<string, float3>();
        public Dictionary<string, float3> scaledCoordinates = new Dictionary<string, float3>();
        private int graphNr;
        private Random random;
        private float spawnTimer;
        private EntityManager entityManager;
        private List<PointCloud> pointClouds = new List<PointCloud>();
        private EntityArchetype entityArchetype;
        private QuadrantSystem quadrantSystem;

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
            quadrantSystem.graphParentTransforms.Add(transform);
            quadrantSystem.graphParentTransforms[nrOfGraphs] = pc.transform;
            pc.Initialize(nrOfGraphs);
            nrOfGraphs++;
            return pc;
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
                scaledCoordinates[cellName] = scaledCoord;
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

        public void SpawnPoints(PointCloud pc, bool spatial)
        {
            Entity parent = entityManager.CreateEntity(entityArchetype);
            entityManager.SetComponentData(parent, new Translation {Value = new float3(0, 0, 0)});
            entityManager.SetComponentData(parent, new Rotation {Value = new quaternion(0, 0, 0, 0)});
            entityManager.SetComponentData(parent, new Scale {Value = 1f});
            LocalToWorld localToWorld = entityManager.GetComponentData<LocalToWorld>(parent);
            PointCloudComponent pointCloudComponent = new PointCloudComponent{pointCloudId = graphNr};
            entityManager.SetComponentData(parent, pointCloudComponent);
            creatingGraph = true;
            ScaleAllCoordinates();
            pc.minCoordValues = minCoordValues;
            pc.maxCoordValues = maxCoordValues;
            pc.scaledOffset = scaledOffset;
            pc.longestAxis = longestAxis;
            graphNr++;
            pointClouds.Add(pc);
            foreach (KeyValuePair<string, float3> pointPair in points)
            {
                float3 pos = ScaleCoordinate(pointPair.Key);
            }


            StartCoroutine(pc.CreatePositionTextureMap(scaledCoordinates.Values.ToList()));
            print($" graphs : {nrOfGraphs}, files : {mdsFileCount}");
            if (nrOfGraphs == mdsFileCount)
            {
                StartCoroutine(CreateColorTextureMap(scaledCoordinates.Values.ToList().Count));
            }

            creatingGraph = false;
        }

        private IEnumerator CreateColorTextureMap(int pointCount)
        {
            int width = (int) math.ceil(math.sqrt(pointCount));
            int height = width;
            colorMap = new Texture2D(width, height, TextureFormat.RGBAFloat, true, true);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int ind = x + (width * y);
                    if (ind >= pointCount) continue;
                    Color c = clusterDict.Count == 0 ? Color.white : colorDict[clusterDict[ind]];
                    colorMap.SetPixel(x, y, c);
                }

                yield return null;
            }

            colorMap.Apply();
            foreach (PointCloud pc in pointClouds)
            {
                VisualEffect vfx = pc.GetComponent<VisualEffect>();
                vfx.enabled = true;
                vfx.Play();
                vfx.SetTexture("ColorMapTex", colorMap);
            }

            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<TextureHandler>().colorTextureMap = colorMap;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<QuadrantSystem>().SetHashMap(nrOfGraphs);
        }

        // public void ColorPoints(PointCloud pc)
        // {
        //     CreateColorTextureMap();
        // }

        public void ReadMetaData(PointCloud pc, string dir)
        {
            // string[] tissues = new string[]
            // {
            //     "Adrenal", "Cerebellum", "Cerebrum", "Eye", "Heart", "Intestine", "Kidney", "Liver", "Lung", "Muscle", "Pancreas", "Placenta",
            //     "Spleen", "Stomach", "Thymus"
            // };
            //
            // int i = 0;
            // Dictionary<string, Color> colorDict = new Dictionary<string, Color>();
            // foreach (string tissue in tissues)
            // {
            //     colorDict[tissue] = SelectionTool.instance.colors[i++ % SelectionTool.instance.colors.Length];
            // }
            //

            colorDict = new Dictionary<string, Color>();
            clusterDict = new Dictionary<int, string>();
            int id = 0;
            string[] metafiles = Directory.GetFiles(dir, "*metadata.csv");
            using (StreamReader sr = new StreamReader(metafiles[0]))
            {
                string header = sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    string[] words = sr.ReadLine().Split(',');
                    string cluster = words[2];
                    clusterDict[id++] = cluster;
                    if (!colorDict.ContainsKey(cluster))
                    {
                        Color c = UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);
                        c.a = 1;
                        colorDict[cluster] = c;
                    }
                }
            }
        }
    }
}