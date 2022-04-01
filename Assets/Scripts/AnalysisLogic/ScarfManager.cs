using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using JetBrains.Annotations;
using SQLiter;
using System.Threading;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using CellexalVR.DesktopUI;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine.InputSystem;

namespace CellexalVR.AnalysisLogic
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
        private static readonly string url = "http://127.0.0.1:9977/";

        public static ScarfObject scarfObject;
        public static Dictionary<string, List<float>> cellStats;

        private int progress = 0;
        private int firstLineLength;
        public string[] consoleLines = new string[5] { "\n", "\n ", "\n ", "\n ", "\n " };

        private static int numOutputLines = 0;
        public static StringBuilder procOutput = null;
        public static StringBuilder procOutput2 = null;
        private static Process p;
        private Task readTask;

        private void Start()
        {
            instance = this;

            //InitServer();
        }

        private void Update()
        {
            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                StartCoroutine(GetCellValues("RNA_leiden_cluster", "clusters"));
            }
            if (Keyboard.current.jKey.wasPressedThisFrame)
            {
                StartCoroutine(GetCellValues("CD14", "gene"));
            }
        }

        public void InitServer()
        {
            string res = "";
            Thread t = new Thread(() => { res = StartServer(); });
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            t.Start();
        }

        private string StartServer()
        {
            string result = string.Empty;
            try
            {
                string path = "D:\\scarf_for_cellexalvr\\run_scarf_server.bat";

                var info = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    WorkingDirectory = Path.GetDirectoryName(path),
                    Arguments = "/c" + path,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                p = new Process();
                p.StartInfo = info;
                string eOut = null;
                p.EnableRaisingEvents = false;
                p.OutputDataReceived += OutputHandler;
                p.ErrorDataReceived += OutputHandler;

                procOutput = new StringBuilder();
                p.Start();
                p.StandardInput.Close();
                //var _ = ConsumeReader(proc.StandardError);
                //readTask = ConsumeReader(p.StandardOutput);
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();
                p.Close();

                //PrintToFile(procOutput.ToString(), "scarfout.txt");

                return result;
            }

            catch (Exception ex)
            {
                if (ex.GetType() == typeof(System.ComponentModel.Win32Exception) ||
                    ex.GetType() == typeof(ArgumentException))
                {
                    return "Failed to Start Server";
                }
                throw new Exception("Script failed: " + result, ex);
            }
        }

        async Task ConsumeReader(TextReader reader)
        {
            char[] buffer = new char[1];
            string line = "";
            while ((await reader.ReadAsync(buffer, 0, 1)) > 0)
            {
                // process character...for example:
                if (buffer[0] == '\n')
                {
                    print(procOutput.ToString());
                    procOutput.Clear();
                }
                else
                {
                    procOutput.Append(buffer[0]);
                }
            }
        }

        private void PrintToFile(string data, string fp)
        {
            using (StreamWriter sw = new StreamWriter(fp))
            {
                sw.Write(data);
            }
        }


        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                var text = outLine.Data.Split('|');
                var firstPart = text[0];
                var line = $"[{numOutputLines}] - {firstPart}" + Environment.NewLine;
                //// Add the text to the collected output.
                int nrOfLines = consoleLines.Length;
                if (numOutputLines >= nrOfLines)
                {
                    string[] copy = new string[nrOfLines];
                    Array.Copy(consoleLines, copy, nrOfLines);
                    for (int i = consoleLines.Length - 1; i > 0; i--)
                    {
                        consoleLines[i - 1] = copy[i];
                    }
                    consoleLines[nrOfLines - 1] = line;
                }
                else
                {
                    consoleLines[numOutputLines] = line;
                }
                //procOutput.Append($"[{numOutputLines}] - {outLine.Data}" + Environment.NewLine);
                numOutputLines++;
            }
        }

        public IEnumerator ConvertToZarrCoroutine(string dataLabel, string rawData, VisualElement running, VisualElement done)
        {
            string reqURL = $"{url}convert_to_zarr/{dataLabel}/{rawData}";
            print(reqURL);
            UnityWebRequest req = UnityWebRequest.Get(reqURL);
            ScarfUIManager.instance.ToggleProgressBar(true);
            running.RemoveFromClassList("inactive");
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
            {
                print(req.error);
                yield break;
            }

            yield return StartCoroutine(StageDataCoroutine(dataLabel, running, done));

        }

        public IEnumerator StageDataCoroutine(string dataLabel, VisualElement running, VisualElement done)
        {
            string reqURL = $"{url}stage_data/{dataLabel}";
            UnityWebRequest req = UnityWebRequest.Get(reqURL);

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
            {
                print(req.error);
                yield break;
            }
            running.AddToClassList("inactive");
            done.RemoveFromClassList("inactive");
            ScarfUIManager.instance.ToggleProgressBar(false);
            progress++;
        }

        public IEnumerator MarkHVGSCoroutine(string topN, VisualElement running, VisualElement done)
        {
            running.RemoveFromClassList("inactive");
            while (progress < 1) yield return null;
            ScarfUIManager.instance.ToggleProgressBar(true);
            string reqURL = $"{url}mark_hvgs/{topN}";
            UnityWebRequest req = UnityWebRequest.Get(reqURL);

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
            {
                print(req.error);
                yield break;
            }

            running.AddToClassList("inactive");
            done.RemoveFromClassList("inactive");
            ScarfUIManager.instance.ToggleProgressBar(false);
            progress++;
        }


        public IEnumerator MakeGraphCoroutine(string featureKey, VisualElement running, VisualElement done)
        {
            running.RemoveFromClassList("inactive");
            while (progress < 2) yield return null;
            ScarfUIManager.instance.ToggleProgressBar(true);
            string reqURL = $"{url}make_graph/{featureKey}";
            UnityWebRequest req = UnityWebRequest.Get(reqURL);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
            {
                print(req.error);
                yield break;
            }
            running.AddToClassList("inactive");
            done.RemoveFromClassList("inactive");
            ScarfUIManager.instance.ToggleProgressBar(false);
            progress++;
        }

        public IEnumerator RunClusteringCoroutine(string resolution, VisualElement running, VisualElement done)
        {
            running.RemoveFromClassList("inactive");
            while (progress < 4) yield return null;
            ScarfUIManager.instance.ToggleProgressBar(true);
            string reqURL = $"{url}run_clustering/{resolution}";
            UnityWebRequest req = UnityWebRequest.Get(reqURL);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
            {
                print(req.error);
                yield break;
            }
            running.AddToClassList("inactive");
            done.RemoveFromClassList("inactive");
            ScarfUIManager.instance.ToggleProgressBar(false);
            progress++;
        }

        public IEnumerator RunUMAPCoroutine(string nEpochs, VisualElement running, VisualElement done)
        {
            running.RemoveFromClassList("inactive");
            while (progress < 3) yield return null;
            ScarfUIManager.instance.ToggleProgressBar(true);
            string reqURL = $"{url}run_umap/{nEpochs}";

            UnityWebRequest req = UnityWebRequest.Get(reqURL);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
            {
                print(req.error);
                yield break;
            }
            running.AddToClassList("inactive");
            done.RemoveFromClassList("inactive");
            ScarfUIManager.instance.ToggleProgressBar(false);
            CellexalEvents.ScarfUMAPFinished.Invoke();
            progress++;
        }

        private IEnumerator GetCellValues(string key, string type)
        {
            string reqURL = $"{url}get_cell_values/{key}";
            print(reqURL);
            UnityWebRequest req = UnityWebRequest.Get(reqURL);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
            {
                print(req.error);
                yield break;
            }

            string response = System.Text.Encoding.UTF8.GetString(req.downloadHandler.data);
            JObject jObject = JObject.Parse(response);
            List<float> x = jObject[$"values"]
                .Children()
                .Select(v => v.Value<float>())
                .ToList();

            switch (type)
            {
                case "clusters":
                    ReferenceManager.instance.cellManager.ColorAllClusters(x.ToArray());
                    break;
                case "gene":
                    ReferenceManager.instance.cellManager.ColorByGene(x.ToArray());
                    break;
                default:
                    break;
            }
        }

        public void CloseServer()
        {
            StartCoroutine(CloseServerCoroutine());
        }

        private IEnumerator CloseServerCoroutine()
        {
            string reqURL = $"{url}shutdown";
            UnityWebRequest req = UnityWebRequest.Get(reqURL);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
            {
                print(req.error);
                yield break;
            }

            print("Server shutdown");
        }



        public void LoadData(string dataset, string key)
        {
            StartCoroutine(CreateGraph(dataset, key));
        }

        private IEnumerator CreateGraph(string dataset, string key)
        {

            string reqURL = $"{url}get_coords/{dataset}/{key}";
            UnityWebRequest req = UnityWebRequest.Get(reqURL);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
            {
                print(req.error);
                yield break;
            }

            string response = System.Text.Encoding.UTF8.GetString(req.downloadHandler.data);
            JObject jObject = JObject.Parse(response);
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

            CellexalEvents.GraphsLoaded.Invoke();


        }

    }
}

