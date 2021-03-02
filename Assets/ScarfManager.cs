using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using CellexalVR.AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using JetBrains.Annotations;
using SQLiter;
using Unity.Mathematics;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Utilities;

namespace CellexalVR
{
    public class PostParams
    {
        public string uid;
        public List<string> feature;
        public string feat_key;
    }


    [System.Serializable]
    public class ScarfFeatureNames
    {
    }

    [System.Serializable]
    public class ScarfObject
    {
        [HideInInspector] public Dictionary<string, List<float>> cellStats { get; set; }
        [HideInInspector] public Dictionary<string, List<string>> cluster { get; set; }
        [HideInInspector] public Dictionary<string, Dictionary<string, List<float>>> layout { get; set; }
        [HideInInspector] public List<string> feature_names;
    }

    public class ScarfManager : MonoBehaviour
    {
        public static ScarfManager instance;

        // private static string url = "https://scarfweb.xyz";
        // private static string analysisId = "tenx_10k_pbmc_citeseq";
        private static string url = "http://127.0.0.1:5000/";

        // private static string analysisId = "id_6021326fbf37";
        private static string analysisId = "id_94dace868526";

        private List<string> ids = new List<string>() {"id_777f9f373f34", "id_6021326fbf37"};
        private List<string> keys = new List<string>() {"ADT", "RNA"};

        public static ScarfObject scarfObject;
        public static Dictionary<string, List<float>> cellStats;


        private void Start()
        {
            instance = this;
            HttpWebRequest myReq = (HttpWebRequest) WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse) myReq.GetResponse();
            HttpStatusCode status = response.StatusCode;
            if (status == HttpStatusCode.OK)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string jsonResponse = reader.ReadToEnd();
                reader.Close();
                CellexalLog.Log($"{status}, {jsonResponse}");
            }

            else
            {
                CellexalLog.Log($"{status}, Could not connect");
            }
        }

        private static HttpWebRequest CreatePostRequest(PostParams postParams, string req)
        {
            string json = JsonUtility.ToJson(postParams);
            HttpWebRequest myReq = (HttpWebRequest) WebRequest.Create(url + req);
            myReq.Method = "POST";
            byte[] postData = Encoding.UTF8.GetBytes(json);
            myReq.ContentType = "application/json";
            myReq.ContentLength = postData.Length;
            Stream stream = myReq.GetRequestStream();
            stream.Write(postData, 0, postData.Length);
            stream.Close();

            return myReq;
        }

        private IEnumerator LoadAnalyzedData()
        {
            List<string> allClusters = new List<string>();
            for (int i = 0; i < keys.Count; i++)
            {
                // string id = ids[i];
                string feat_key = keys[i];
                if (!ReferenceManager.instance.loaderController.loaderMovedDown)
                {
                    ReferenceManager.instance.loaderController.loaderMovedDown = true;
                    ReferenceManager.instance.loaderController.MoveLoader(new Vector3(0f, -2f, 0f), 2f);
                }

                PostParams param = new PostParams {uid = analysisId, feat_key = feat_key};
                HttpWebRequest request = CreatePostRequest(param, "/load_analyzed_data");

                HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                HttpStatusCode status = response.StatusCode;
                if (status == HttpStatusCode.OK)
                {
                    CellexalLog.Log($"{response.StatusDescription}");
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string jsonResponse = reader.ReadToEnd();
                    scarfObject = JsonConvert.DeserializeObject<ScarfObject>(jsonResponse);
                    reader.Close();
                    int grapNr = 0;
                    // Graph point coordinates
                    foreach (Dictionary<string, List<float>> dict in scarfObject.layout.Values)
                    {
                        Graph combGraph = ReferenceManager.instance.graphGenerator.CreateGraph(GraphGenerator.GraphType.MDS);
                        combGraph.GraphName = feat_key + "_" + scarfObject.layout.Keys.ToList()[grapNr];
                        ReferenceManager.instance.graphManager.originalGraphs.Add(combGraph);
                        ReferenceManager.instance.graphManager.Graphs.Add(combGraph);
                        ReferenceManager.instance.inputReader.mdsReader.CreateFromCoordinates(dict["x"], dict["y"], dict["z"]);
                        StartCoroutine(ReferenceManager.instance.graphGenerator.SliceClusteringLOD(1));
                        while (ReferenceManager.instance.graphGenerator.isCreating)
                            yield return null;
                        grapNr++;
                    }

                    // cluster/attributes
                    foreach (string key in scarfObject.cluster.Keys)
                    {
                        List<string> clusters = scarfObject.cluster[key];
                        List<string> uniqueClusters = clusters.Distinct().ToList();
                        List<string> clustersWithCat = new List<string>();
                        for (int l = 0; l < clusters.Count; l++)
                        {
                            clustersWithCat.Add(key + "@" + clusters[l]);
                            ReferenceManager.instance.cellManager.AddAttribute(
                                l.ToString(),
                                clustersWithCat[l],
                                int.Parse(clusters[l]) - 1 % CellexalConfig.Config.SelectionToolColors.Length
                            );
                        }

                        allClusters.AddRange(clustersWithCat.Distinct());
                    }
                }

                response.Close();
                // ReferenceManager.instance.cellManager.SetCellStats(scarfObject.cellStats);

                GetFeatureNames();
                CellexalEvents.ScarfObjectLoaded.Invoke();
            }

            // ReferenceManager.instance.cellManager.Attributes = new string[allClusters.Count];
            ReferenceManager.instance.attributeSubMenu.CreateButtons(allClusters.ToArray());
            // ReferenceManager.instance.cellManager.Attributes = allClusters.ToArray();
            // cellStats = scarfObject.cellStats;
            // ReferenceManager.instance.cellStatMenu.CreateButtons(cellStats.Keys.ToArray());
        }


        private void GetFeatureNames()
        {
            PostParams param = new PostParams {uid = analysisId};
            HttpWebRequest request = CreatePostRequest(param, "/get_feature_names");
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            HttpStatusCode status = response.StatusCode;
            if (status == HttpStatusCode.OK)
            {
                CellexalLog.Log($"{response.StatusDescription}");
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string jsonResponse = reader.ReadToEnd();
                scarfObject = JsonConvert.DeserializeObject<ScarfObject>(jsonResponse);
                reader.Close();
            }
        }

        public static void ColorByCellStat(string statName)
        {
            if (scarfObject == null || ReferenceManager.instance.graphGenerator.isCreating) return;
            if (!cellStats.ContainsKey(statName))
            {
                CellexalLog.Log($"Could not find {statName} in scarf object");
                return;
            }

            ArrayList expressions = new ArrayList();
            float highestVal = cellStats[statName].Max();
            float minVal = cellStats[statName].Min();

            highestVal *= 1.0001f;
            float binSize = (highestVal - minVal) / CellexalConfig.Config.GraphNumberOfExpressionColors;
            for (int i = 0; i < cellStats[statName].Count; i++)
            {
                float val = cellStats[statName][i];
                if (val == 0f) continue;
                int colInd = (int) ((val - minVal) / binSize);
                CellExpressionPair pair = new CellExpressionPair(i.ToString(), val, colInd);
                expressions.Add(pair);
            }

            ReferenceManager.instance.graphManager.ColorAllGraphsByGeneExpression(statName, expressions);
        }

        public static ArrayList GetFeatureValues(string name)
        {
            if (scarfObject == null || ReferenceManager.instance.graphGenerator.isCreating) return null;
            ArrayList result = new ArrayList();
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            PostParams param = new PostParams {uid = analysisId, feature = new List<string> {name}};
            HttpWebRequest request = CreatePostRequest(param, "/get_feature_values");
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            HttpStatusCode status = response.StatusCode;
            if (status == HttpStatusCode.OK)
            {
                CellexalLog.Log($"{response.StatusDescription}");
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string jsonResponse = reader.ReadToEnd();
                var resp = JsonConvert.DeserializeObject<Dictionary<string, List<float>>>(jsonResponse);
                var values = resp[name];
                List<CellExpressionPair> expressions = new List<CellExpressionPair>();

                float highestVal = values.Max();
                float minVal = values.Min(); //scarfObject.cellStats["percentRibo"].Min();
                float binSize = (highestVal - minVal) / CellexalConfig.Config.GraphNumberOfExpressionColors;
                for (int i = 0; i < values.Count; i++)
                {
                    float val = values[i];
                    if (val == 0f) continue;
                    int colInd = (int) ((val - minVal) / binSize);
                    CellExpressionPair pair = new CellExpressionPair(i.ToString(), val, colInd);
                    expressions.Add(pair);
                }

                // ### Equal number of cells in bin ##
                // float maxVal = values[0];
                // float minVal = values[values.Count - 1];
                //
                // float binSize = (float) values.Count / CellexalConfig.Config.GraphNumberOfExpressionColors;
                // for (int i = 0; i < values.Count; i++)
                // {
                //     float val = values[i];
                //     CellExpressionPair pair = new CellExpressionPair(i.ToString(), val, -1);
                //     expressions.Add(pair);
                // }
                //
                // expressions.Sort();
                // for (int j = 0; j < expressions.Count; j++)
                // {
                //     expressions[j].Color = (int) (j / binSize);
                // }

                result.AddRange(expressions);
                stopWatch.Stop();
                print(stopWatch.Elapsed.TotalSeconds);
                reader.Close();
            }

            return result;
        }


        [ItemCanBeNull]
        public static Dictionary<string, List<Tuple<string, float>>> GetFeatureValues(List<string> genes, List<string> cellIds)
        {
            if (scarfObject == null || ReferenceManager.instance.graphGenerator.isCreating) return null;
            Dictionary<string, List<Tuple<string, float>>> result = new Dictionary<string, List<Tuple<string, float>>>();
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            PostParams param = new PostParams {uid = analysisId, feature = genes};
            HttpWebRequest request = CreatePostRequest(param, "/get_feature_values");
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            HttpStatusCode status = response.StatusCode;
            if (status == HttpStatusCode.OK)
            {
                CellexalLog.Log($"{response.StatusDescription}");
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string jsonResponse = reader.ReadToEnd();
                var resp = JsonConvert.DeserializeObject<Dictionary<string, List<float>>>(jsonResponse);
                // var selectedValues = new Dictionary<string, List<Tuple<int, float>>>();
                foreach (string gene in genes)
                {
                    result[gene] = new List<Tuple<string, float>>();
                    foreach (string i in cellIds)
                    {
                        float value = resp[gene][int.Parse(i)];
                        if (value == 0f) continue;
                        result[gene].Add(new Tuple<string, float>(i, value));
                    }
                }
            }

            return result;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                StartCoroutine(LoadAnalyzedData());
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                ColorByCellStat("percentRibo");
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                GetFeatureNames();
                print(scarfObject.feature_names[0]);
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                ArrayList res = GetFeatureValues("MS4A1");
                ReferenceManager.instance.graphManager.ColorAllGraphsByGeneExpression(name, res);
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                // GenerateHeatmap();
            }
        }

        private void GenerateHeatmap()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            List<Graph.GraphPoint> gps = ReferenceManager.instance.selectionManager.GetLastSelection();
            List<string> gpIds = new List<string>();
            foreach (var gp in gps)
            {
                gpIds.Add(gp.Label);
            }

            List<string> genes = scarfObject.feature_names.GetRange(0, 50);
            // Dictionary<string, ArrayList> values = new Dictionary<string, ArrayList>();
            GetFeatureValues(genes, gpIds);
            // foreach (string gene in genes)
            // {
            //     values[gene] = GetFeatureValues(gene);
            //     yield return null;
            // }

            stopWatch.Stop();
            print(stopWatch.Elapsed.TotalSeconds);
        }
    }
}