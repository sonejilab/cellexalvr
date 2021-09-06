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
using Valve.Newtonsoft.Json.Linq;
using UnityEngine.InputSystem;

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
        [HideInInspector] public Dictionary<string, List<float>> coords { get; set; }
        [HideInInspector] public List<string> feature_names;
    }

    public class ScarfManager : MonoBehaviour
    {
        public static ScarfManager instance;

        // private static string url = "https://scarfweb.xyz";
        private static string url = "http://127.0.0.1:5000/";

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
            myReq.ContentLength = 0;
            Stream stream = myReq.GetRequestStream();
            stream.Write(postData, 0, postData.Length);
            stream.Close();

            return myReq;
        }

        private static HttpWebRequest CreateGetRequest(string req)
        {
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url + req);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            myReq.ContentLength = 0;
            return myReq;
        }


        private IEnumerator CreateGraph(string dataset, string key)
        {
            HttpWebRequest request = CreateGetRequest($"get_coords/{dataset}/{key}");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            HttpStatusCode status = response.StatusCode;
            if (status == HttpStatusCode.OK)
            {
                print($"{response.StatusDescription}");
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string jsonResponse = reader.ReadToEnd();

                JObject jObject = JObject.Parse(jsonResponse);

                var x = jObject[$"{key}1"]
                    .Children()
                    .Select(v => v.Value<float>())
                    .ToList();
                var y = jObject[$"{key}2"]
                    .Children()
                    .Select(v => v.Value<float>())
                    .ToList();
                var z = jObject[$"{key}3"]
                    .Children()
                    .Select(v => v.Value<float>())
                    .ToList();


                Graph combGraph = ReferenceManager.instance.graphGenerator.CreateGraph(GraphGenerator.GraphType.MDS);
                combGraph.GraphName = dataset + "_" + key;
                ReferenceManager.instance.graphManager.originalGraphs.Add(combGraph);
                ReferenceManager.instance.graphManager.Graphs.Add(combGraph);
                ReferenceManager.instance.inputReader.mdsReader.CreateFromCoordinates(x, y, z);
                StartCoroutine(ReferenceManager.instance.graphGenerator.SliceClusteringLOD(1));
                while (ReferenceManager.instance.graphGenerator.isCreating)
                    yield return null;

                }
        }


        private void GetFeatureNames()
        {
            HttpWebRequest request = CreateGetRequest("/get_feature_names");
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
            HttpWebRequest request = CreateGetRequest("/get_feature_values");
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
            HttpWebRequest request = CreateGetRequest("/get_feature_values");
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
            if (Keyboard.current.jKey.wasPressedThisFrame)
            {
                //StartCoroutine(LoadAnalyzedData());
                StartCoroutine(CreateGraph("pbmc_5k", "RNA_UMAP"));
            }

            if (Keyboard.current.kKey.wasPressedThisFrame)
            {
                ColorByCellStat("percentRibo");
            }

            if (Keyboard.current.lKey.wasPressedThisFrame)
            {
                GetFeatureNames();
                print(scarfObject.feature_names[0]);
            }

            if (Keyboard.current.nKey.wasPressedThisFrame)
            {
                ArrayList res = GetFeatureValues("MS4A1");
                ReferenceManager.instance.graphManager.ColorAllGraphsByGeneExpression(name, res);
            }

        }

    }
}