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
using System.Threading;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using CellexalVR.DesktopUI;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CellexalVR.AnalysisLogic
{

    /// <summary>
    /// Class to handle the communication between the REST-ful scarf api and CellexalVR. The api is a flask app that gets http requests and sends back data in json format.
    /// The cellexal-scarf web api can be found here: https://github.com/sonejilab/scarf_for_cellexalvr/
    /// </summary>
    public class ScarfManager : MonoBehaviour
    {
        public static ScarfManager instance;

        private static readonly string url = "http://127.0.0.1:9977/";

        public string[] datasets;
        public string[] geneNames;
        public string[] markers;
        public float[] cellValues;

        public bool scarfActive;
        public bool reqPending;
        private bool serverStarted;

        private int progress = 0;
        private int firstLineLength;
        public string[] consoleLines = new string[5] { "\n", "\n ", "\n ", "\n ", "\n " };


        private static int numOutputLines = 0;
        public static StringBuilder procOutput = null;
        public static StringBuilder procOutput2 = null;
        private static Process p;
        private Task readTask;

        private void Awake()
        {
            instance = this;
        }

        public void SetZarrLoc(string loc)
        {
            StartCoroutine(SetZarrLocCoroutine(loc));
        }

        private IEnumerator SetZarrLocCoroutine(string loc)
        {
            while (!serverStarted)
                yield return null;
            string reqURL = $"{url}set_zarr_loc/{loc}";
            UnityWebRequest req = UnityWebRequest.Get(reqURL);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
            {
                print(req.error);
                yield break;
            }
        }

        public void LoadPreprocessedData(string label, string layoutKey = "RNA_UMAP")
        {
            StartCoroutine(LoadPreprocessedDataCouroutine(label, layoutKey));
        }

        /// <summary>
        /// Load data into CellexalVR that has been preprocessed in scarf.
        /// </summary>
        /// <param name="label">Name of the dataset folder.</param>
        /// <param name="layoutKey">Name of the value you wish to plot. For example if you have made a umap based on RNA data it is usually called RNA_UMAP. Or whatever else you have named it when pre-processing the data.</param>
        /// <returns></returns>
        public IEnumerator LoadPreprocessedDataCouroutine(string label, string layoutKey)
        {
            yield return StartCoroutine(StageDataCoroutine(label));
            yield return StartCoroutine(CreateGraph(label, layoutKey));
        }

        /// <summary>
        /// Initializes the flask app that runs the api. Start in a seperate thread.
        /// </summary>
        public void InitServer()
        {
            string res = "";
            Thread t = new Thread(() => { res = StartServer(); });
            t.Start();

            StartCoroutine(SetZarrLocCoroutine(Path.Combine(Directory.GetCurrentDirectory(), "Data")));
        }

        /// <summary>
        /// Opens up a cmd window and runs the bat script located in the Scarf script path you have set in the config. This in turn activates a conda environment and starts the flask app.
        /// </summary>
        /// <returns></returns>
        private string StartServer()
        {
            string result = string.Empty;
            try
            {
                string path = Path.Combine(CellexalConfig.Config.ScarfscriptPath, "run_scarf_server.bat");

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
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();
                p.Close();
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

        /// <summary>
        /// Handles output from the thread that runs the flask app. 
        /// </summary>
        /// <param name="sendingProcess"></param>
        /// <param name="outLine"></param>
        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                var text = outLine.Data.Split('|');
                var firstPart = text[0];
                var line = $"[{numOutputLines}] - {firstPart}" + Environment.NewLine;
                if (line.Contains("Running on"))
                {
                    serverStarted = true;
                }
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
                numOutputLines++;
            }
        }

        /// <summary>
        /// Retrieves the scarf datasets in the data folder.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetDatasetsCoroutine()
        {
            while (!serverStarted)
                yield return null;
            reqPending = true;
            string reqURL = $"{url}get_datasets";
            UnityWebRequest req = UnityWebRequest.Get(reqURL);
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
            {
                print(req.error);
                reqPending = false;
                yield break;
            }
            string response = System.Text.Encoding.UTF8.GetString(req.downloadHandler.data);
            JObject jObject = JObject.Parse(response);
            List<string> x = jObject[$"values"]
                .Children()
                .Select(v => v.Value<string>())
                .ToList();

            datasets = x.ToArray();
            reqPending = false;
        }

        /// <summary>
        /// Convert h5 raw data to zarr format.
        /// </summary>
        /// <param name="dataLabel">What you want to call the resulting data folder.</param>
        /// <param name="rawData">The name of the raw data file.</param>
        /// <returns></returns>
        public IEnumerator ConvertToZarrCoroutine(string dataLabel, string rawData)
        {
            while (!serverStarted)
                yield return null;
            string reqURL = $"{url}convert_to_zarr/{dataLabel}/{rawData}";
            UnityWebRequest req = UnityWebRequest.Get(reqURL);
            ScarfUIManager.instance.ToggleProgressBar(true);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
            {
                print(req.error);
                yield break;
            }

            CellexalEvents.ZarrConversionComplete.Invoke();

            yield return StartCoroutine(StageDataCoroutine(dataLabel));

        }

        /// <summary>
        /// Prepares the scarf object to work with.
        /// </summary>
        /// <param name="dataLabel"></param>
        /// <returns></returns>
        public IEnumerator StageDataCoroutine(string dataLabel)
        {
            while (!serverStarted)
                yield return null;
            string reqURL = $"{url}stage_data/{dataLabel}";
            UnityWebRequest req = UnityWebRequest.Get(reqURL);

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
            {
                print(req.error);
                yield break;
            }

            CellexalEvents.DataStaged.Invoke();
            progress++;
        }

        /// <summary>
        /// Calls the scarf backend function to marks highly variable genes in the staged data.
        /// </summary>
        /// <param name="topN">Number of genes.</param>
        /// <param name="running">For the user interface icons animation.</param>
        /// <param name="done">For the user interface icons animation.</param>
        /// <returns></returns>
        public IEnumerator MarkHVGSCoroutine(string topN, VisualElement running, VisualElement done)
        {
            while (!serverStarted)
                yield return null;
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


        /// <summary>
        /// Calls the scarf function to create a neighborhood graph. 
        /// </summary>
        /// <param name="featureKey">The feature key in the scarf object to use to create the graph. E.g. hvgs (if you have marked hvgs that is.)</param>
        /// <param name="running">For the user interface icons animation.</param>
        /// <param name="done">For the user interface icons animation.</param>
        /// <returns></returns>
        public IEnumerator MakeNeighborhoodGraphCoroutine(string featureKey, VisualElement running, VisualElement done)
        {
            while (!serverStarted)
                yield return null;
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

        /// <summary>
        /// Calls the scarf function to do clustering. Requires the neighborhood graph to be made.
        /// </summary>
        /// <param name="resolution">Resolution of clustering. Corresponding to the argument with the same name in the scarf clustering function.</param>
        /// <param name="running">For the user interface icons animation.</param>
        /// <param name="done">For the user interface icons animation.</param>
        /// <returns></returns>
        public IEnumerator RunClusteringCoroutine(string resolution, VisualElement running, VisualElement done)
        {
            while (!serverStarted)
                yield return null;
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

        /// <summary>
        /// Calls the scarf function to create a umap. Requires the neighborhood graph to be made.
        /// </summary>
        /// <param name="nEpochs">Number of epochs to run.</param>
        /// <param name="running">For the user interface icons animation.</param>
        /// <param name="done">For the user interface icons animation.</param>
        /// <returns></returns>
        public IEnumerator RunUMAPCoroutine(string nEpochs, VisualElement running, VisualElement done)
        {
            while (!serverStarted)
                yield return null;
            running.RemoveFromClassList("inactive");
            while (progress < 3) yield return null;
            ScarfUIManager.instance.ToggleProgressBar(true);
            string reqURL = $"{url}run_umap/{nEpochs}/";

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

        /// <summary>
        /// Calls the scarf function to retrieve the feature names present in the dataset. This can be gene names or other features.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetFeatureNames()
        {
            while (!serverStarted)
                yield return null;
            string reqURL = $"{url}get_gene_names";
            UnityWebRequest req = UnityWebRequest.Get(reqURL);
            reqPending = true;
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
            {
                print(req.error);
                reqPending = false;
                yield break;
            }

            string response = System.Text.Encoding.UTF8.GetString(req.downloadHandler.data);
            JObject jObject = JObject.Parse(response);
            List<string> x = jObject[$"values"]
                .Children()
                .Select(v => v.Value<string>())
                .ToList();

            geneNames = x.ToArray();
            reqPending = false;
        }

        /// <summary>
        /// Retrieves the cell values for a specific feature key. This can be a gene or some other feature present in your dataset.
        /// </summary>
        /// <param name="valueKey">The name of the feature.</param>
        /// <returns></returns>
        public IEnumerator GetCellValues(string valueKey)
        {
            while (!serverStarted)
                yield return null;
            string reqURL = $"{url}get_cell_values/{valueKey}";
            UnityWebRequest req = UnityWebRequest.Get(reqURL);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
            {
                print(req.error);
                yield break;
            }

            string response = System.Text.Encoding.UTF8.GetString(req.downloadHandler.data);
            JObject jObject = JObject.Parse(response);
            cellValues = jObject[$"values"]
                .Children()
                .Select(v => v.Value<float>())
                .ToArray();
        }

        /// <summary>
        /// Retrieves the cell values for the clustering and then colors the graphs accordingly.
        /// </summary>
        /// <param name="clusterName">The name of the clustering label e.g. RNA_leiden_cluster.</param>
        /// <returns></returns>
        public IEnumerator ColorByClusters(string clusterName)
        {
            while (!serverStarted)
                yield return null;
            yield return GetCellValues(clusterName);
            ReferenceManager.instance.cellManager.ColorAllClusters(cellValues.ToArray(), true);
        }

        /// <summary>
        /// Retrieves the cell values for the given gene or other feature and then colors the graphs accordingly.
        /// </summary>
        /// <param name="geneName">The name of the gene or feature to color by.</param>
        /// <returns></returns>
        public IEnumerator ColorByGene(string geneName)
        {
            while (!serverStarted)
                yield return null;
            yield return GetCellValues(geneName);
            ReferenceManager.instance.cellManager.ColorByGene(cellValues.ToArray());
        }

        /// <summary>
        /// Find marker genes in the dataset.
        /// </summary>
        /// <param name="groupKey">The key used to separate the cells into groups. For example RNA_leiden clusters.</param>
        /// <param name="threshold">Threshold to use for a gene correlation.</param>
        /// <returns></returns>
        public IEnumerator RunMarkerSearch(string groupKey, float threshold)
        {
            while (!serverStarted)
                yield return null;
            reqPending = true;
            string reqURL = $"{url}run_marker_search/{groupKey}/{threshold}";
            UnityWebRequest req = UnityWebRequest.Get(reqURL);

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
            {
                print(req.error);
                reqPending = false;
                yield break;
            }
            reqPending = false;
        }

        /// <summary>
        /// Retrieves the marker genes. Requires RunMarkerSearch to have been run or have been marked in the dataset in pre-processing.
        /// </summary>
        /// <param name="groupKey">The key used to separate the cells into groups. For example RNA_leiden clusters.</param>
        /// <returns></returns>
        public IEnumerator GetMarkers(string groupKey)
        {
            while (!serverStarted)
                yield return null;
            string reqURL = $"{url}get_markers/{groupKey}";
            UnityWebRequest req = UnityWebRequest.Get(reqURL);

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
            {
                print(req.error);
                yield break;
            }

            string response = System.Text.Encoding.UTF8.GetString(req.downloadHandler.data);
            JObject jObject = JObject.Parse(response);
            markers = jObject[$"values"]
                .Children()
                .Select(v => v.Value<string>())
                .ToArray();

        }

        public void CloseServer()
        {
            StartCoroutine(CloseServerCoroutine());
        }

        /// <summary>
        /// Close server and close down backend process that runs the flask app.
        /// </summary>
        /// <returns></returns>
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
            progress = 0;
        }


        /// <summary>
        /// Load the dataset into CellexalVR which creates the graph object.
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="key"></param>
        public void LoadData(string dataset, string key)
        {
            StartCoroutine(CreateGraph(dataset, key));
        }

        /// <summary>
        /// Retrieves the graph coordinates and then creates the graph object.
        /// </summary>
        /// <param name="dataset">Name of the data folder.</param>
        /// <param name="key">Key used to find the coordinates. E.g. RNA_UMAP or RNA_tSNE.</param>
        /// <returns></returns>
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

            scarfActive = true;
            CellexalEvents.GraphsLoaded.Invoke();

        }

    }
}

