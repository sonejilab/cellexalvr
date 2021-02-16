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
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using SQLiter;
using Unity.Mathematics;
using Valve.Newtonsoft.Json;

public class PostParams
{
    public string uid;
    public string feature;
}

[System.Serializable]
public class ScarfObject
{
    public Dictionary<string, List<float>> cellStats { get; set; }
    public Dictionary<string, List<int>> cluster { get; set; }
    public Dictionary<string, Dictionary<string, List<float>>> layout { get; set; }
    public List<string> feature_names;
}

[System.Serializable]
public class ScarfFeatureNames
{
}

public class ScarfManager : MonoBehaviour
{
    // Start is called before the first frame update
    private string url = "https://scarfweb.xyz";
    private string analysisId = "tenx_10k_pbmc_citeseq";

    public ScarfObject scarfObject;
    public ScarfFeatureNames scarfFeatureNames;

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

    private IEnumerator LoadAnalyzedData()
    {
        PostParams param = new PostParams {uid = analysisId};
        string json = JsonUtility.ToJson(param);
        HttpWebRequest myReq = (HttpWebRequest) WebRequest.Create(url + "/load_analyzed_data");
        myReq.Method = "POST";
        byte[] postData = Encoding.UTF8.GetBytes(json);
        myReq.ContentType = "application/json";
        myReq.ContentLength = postData.Length;
        Stream stream = myReq.GetRequestStream();
        stream.Write(postData, 0, postData.Length);
        stream.Close();
        HttpWebResponse response = (HttpWebResponse) myReq.GetResponse();
        HttpStatusCode status = response.StatusCode;
        if (status == HttpStatusCode.OK)
        {
            CellexalLog.Log($"{response.StatusDescription}");
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string jsonResponse = reader.ReadToEnd();
            reader.Close();

            scarfObject = JsonConvert.DeserializeObject<ScarfObject>(jsonResponse);
            foreach (Dictionary<string, List<float>> dict in scarfObject.layout.Values)
            {
                Graph combGraph = ReferenceManager.instance.graphGenerator.CreateGraph(GraphGenerator.GraphType.MDS);
                ReferenceManager.instance.graphManager.Graphs.Add(combGraph);
                ReferenceManager.instance.inputReader.mdsReader.CreateFromCoordinates(dict["x"], dict["y"]);
                StartCoroutine(ReferenceManager.instance.graphGenerator.SliceClusteringLOD(1));
                while (ReferenceManager.instance.graphGenerator.isCreating)
                    yield return null;
            }
        }

        response.Close();
    }

    private void GetFeatureNames()
    {
        PostParams param = new PostParams {uid = analysisId};
        string json = JsonUtility.ToJson(param);
        HttpWebRequest myReq = (HttpWebRequest) WebRequest.Create(url + "/get_feature_names");
        myReq.Method = "POST";
        byte[] postData = Encoding.UTF8.GetBytes(json);
        myReq.ContentType = "application/json";
        myReq.ContentLength = postData.Length;
        Stream stream = myReq.GetRequestStream();
        stream.Write(postData, 0, postData.Length);
        stream.Close();
        HttpWebResponse response = (HttpWebResponse) myReq.GetResponse();
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

    private void ColorByExpression()
    {
        if (scarfObject == null || ReferenceManager.instance.graphGenerator.isCreating) return;
        ArrayList expressions = new ArrayList();
        float highestVal = scarfObject.cellStats["percentRibo"].Max();
        float minVal = scarfObject.cellStats["percentRibo"].Min();
        float binSize = (highestVal - minVal) / CellexalConfig.Config.GraphNumberOfExpressionColors;
        for (int i = 0; i < scarfObject.cellStats["percentRibo"].Count; i++)
        {
            float val = scarfObject.cellStats["percentRibo"][i];
            int colInd = (int) ((val - minVal) / binSize);
            CellExpressionPair pair = new CellExpressionPair(i.ToString(), val, colInd);
            expressions.Add(pair);
        }

        ReferenceManager.instance.graphManager.ColorAllGraphsByGeneExpression("percentRibo", expressions);
    }

    private void ColorByFeature(string name)
    {
        if (scarfObject == null || ReferenceManager.instance.graphGenerator.isCreating || scarfFeatureNames == null) return;
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        print($"{name}, {scarfObject.feature_names.Contains(name)}, {scarfObject.feature_names.Count}");
        PostParams param = new PostParams {uid = analysisId, feature = name};
        string json = JsonUtility.ToJson(param);
        HttpWebRequest myReq = (HttpWebRequest) WebRequest.Create(url + "/get_feature_values");
        myReq.Method = "POST";
        byte[] postData = Encoding.UTF8.GetBytes(json);
        myReq.ContentType = "application/json";
        myReq.ContentLength = postData.Length;
        Stream stream = myReq.GetRequestStream();
        stream.Write(postData, 0, postData.Length);
        stream.Close();
        HttpWebResponse response = (HttpWebResponse) myReq.GetResponse();
        HttpStatusCode status = response.StatusCode;
        if (status == HttpStatusCode.OK)
        {
            CellexalLog.Log($"{response.StatusDescription}");
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string jsonResponse = reader.ReadToEnd();
            var resp = JsonConvert.DeserializeObject<Dictionary<string, List<float>>>(jsonResponse);
            var values = resp[name];
            ArrayList expressions = new ArrayList();
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

            ReferenceManager.instance.graphManager.ColorAllGraphsByGeneExpression(name, expressions);
            stopWatch.Stop();
            print(stopWatch.Elapsed.Seconds);
            reader.Close();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(LoadAnalyzedData());
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            ColorByExpression();
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