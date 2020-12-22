using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnalysisLogic;
// using CellexalVR.AnalysisLogic;
// using CellexalVR.AnalysisObjects;
// using CellexalVR.General;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace DefaultNamespace
{
    public class PointCloudGenerator : MonoBehaviour
    {
        public static PointCloudGenerator instance;
        [SerializeField] public Material material;
        [SerializeField] public Mesh mesh;
        [HideInInspector] public int nrOfGraphs = 0;
        public bool creatingGraph;
        public GameObject parentPrefab;
        public float3 minCoordValues;
        public float3 maxCoordValues;
        public float3 longestAxis;

        public float3 scaledOffset;
        // public List<string> cells = new List<string>();

        private float3 diffCoordValues;
        private Dictionary<string, float3> points = new Dictionary<string, float3>();
        public Dictionary<string, float3> scaledCoordinates = new Dictionary<string, float3>();
        private int graphNr;
        private Random random;
        private float spawnTimer;
        private bool spawned;

        private void Awake()
        {
            instance = this;
        }

        public PointCloud CreateNewPointCloud()
        {
            points.Clear();
            scaledCoordinates.Clear();
            minCoordValues = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            maxCoordValues = new float3(float.MinValue, float.MinValue, float.MinValue);
            PointCloud pc = Instantiate(parentPrefab, new float3(graphNr, 1, graphNr), quaternion.identity).GetComponent<PointCloud>();
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

        public void SpawnPoints(PointCloud pc, bool spatial)
        {
            creatingGraph = true;
            ScaleAllCoordinates();
            pc.minCoordValues = minCoordValues;
            pc.maxCoordValues = maxCoordValues;
            pc.scaledOffset = scaledOffset;
            pc.longestAxis = longestAxis;
            graphNr++;
            foreach (KeyValuePair<string, float3> pointPair in points)
            {
                float3 scaledPos = ScaleCoordinate(pointPair.Key);
            }

            pc.CreatePositionTextureMap(scaledCoordinates.Values.ToList());
            // pc.gameObject.SetActive(false);
            creatingGraph = false;
        }

        public void ColorPoints(PointCloud pc)
        {
            pc.CreateColorTextureMap();
        }

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

            pc.colorDict = new Dictionary<string, Color>();
            pc.clusterDict = new Dictionary<int, string>();
            int id = 0;
            string[] metafiles = Directory.GetFiles(dir, "*metadata.csv");
            using (StreamReader sr = new StreamReader(metafiles[0]))
            {
                string header = sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    string[] words = sr.ReadLine().Split(',');
                    //int.Parse(words[0]);
                    string cluster = words[2];
                    pc.clusterDict[id++] = cluster;
                    if (!pc.colorDict.ContainsKey(cluster))
                    {
                        Color c = UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);
                        c.a = 1;
                        pc.colorDict[cluster] = c;
                    }
                }
            }
        }

        
    }
}