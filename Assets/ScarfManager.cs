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
using SQLiter;
using Unity.Mathematics;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Utilities;

namespace CellexalVR
{
    public class PostParams
    {
        public string uid;
        public string feature;
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
        private static string url = "https://scarfweb.xyz";
        private static string analysisId = "tenx_10k_pbmc_citeseq";

        public static ScarfObject scarfObject;
        public static Dictionary<string, List<float>> cellStats;


        private void Start()
        {
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
            if (!ReferenceManager.instance.loaderController.loaderMovedDown)
            {
                ReferenceManager.instance.loaderController.loaderMovedDown = true;
                ReferenceManager.instance.loaderController.MoveLoader(new Vector3(0f, -2f, 0f), 2f);
            }

            PostParams param = new PostParams {uid = analysisId};
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
                    combGraph.GraphName = scarfObject.layout.Keys.ToList()[grapNr];
                    ReferenceManager.instance.graphManager.originalGraphs.Add(combGraph);
                    ReferenceManager.instance.graphManager.Graphs.Add(combGraph);
                    ReferenceManager.instance.inputReader.mdsReader.CreateFromCoordinates(dict["x"], dict["y"]);
                    StartCoroutine(ReferenceManager.instance.graphGenerator.SliceClusteringLOD(1));
                    while (ReferenceManager.instance.graphGenerator.isCreating)
                        yield return null;
                    grapNr++;
                }

                // cluster/attributes
                List<string> allClusters = new List<string>();
                foreach (string key in scarfObject.cluster.Keys)
                {
                    List<string> clusters = scarfObject.cluster[key];
                    List<string> uniqueClusters = clusters.Distinct().ToList();
                    List<string> clustersWithCat = new List<string>();
                    for (int i = 0; i < clusters.Count; i++)
                    {
                        clustersWithCat.Add(key + "@" + clusters[i]);
                        ReferenceManager.instance.cellManager.AddAttribute(
                            i.ToString(),
                            clustersWithCat[i],
                            uniqueClusters.IndexOf(clusters[i]) % CellexalConfig.Config.SelectionToolColors.Length
                        );
                    }

                    allClusters.AddRange(clustersWithCat.Distinct());
                }


                // ReferenceManager.instance.cellManager.Attributes = new string[allClusters.Count];
                ReferenceManager.instance.attributeSubMenu.CreateButtons(allClusters.ToArray());
                // ReferenceManager.instance.cellManager.Attributes = allClusters.ToArray();
                cellStats = scarfObject.cellStats;
                ReferenceManager.instance.cellStatMenu.CreateButtons(cellStats.Keys.ToArray());
            }

            response.Close();
            // ReferenceManager.instance.cellManager.SetCellStats(scarfObject.cellStats);

            GetFeatureNames();
            CellexalEvents.ScarfObjectLoaded.Invoke();
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
                int colInd = (int) ((val - minVal) / binSize);
                CellExpressionPair pair = new CellExpressionPair(i.ToString(), val, colInd);
                expressions.Add(pair);
            }

            ReferenceManager.instance.graphManager.ColorAllGraphsByGeneExpression(statName, expressions);
        }

        public static void ColorByFeature(string name)
        {
            if (scarfObject == null || ReferenceManager.instance.graphGenerator.isCreating) return;
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            PostParams param = new PostParams {uid = analysisId, feature = name};
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

                ArrayList result = new ArrayList();
                result.AddRange(expressions);
                ReferenceManager.instance.graphManager.ColorAllGraphsByGeneExpression(name, result);
                stopWatch.Stop();
                print(stopWatch.Elapsed.Milliseconds);
                reader.Close();
            }
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
                ColorByFeature("MS4A1");
            }
        }
    }
}