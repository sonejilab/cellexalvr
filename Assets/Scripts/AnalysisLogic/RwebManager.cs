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
    /// Class to handle the communication between the REST-ful R::plumber besed cellexalvrR api and CellexalVR. The api gets http requests and sends back data in json format.
    /// </summary>
    public class RwebManager : MonoBehaviour
    {
        public static RwebManager instance;

        private static readonly string url = "http://127.0.0.1:9977/";

        public string[] datasets;
        public string[] geneNames;
        public string[] markers;
        public float[] cellValues;

        public bool RwebapiActive;
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

    
        /// <summary>
        /// Initializes the flask app that runs the api. Start in a seperate thread.
        /// </summary>
        public string InitServer()
        {
            CellexalLog.Log("               Start the web server R web API ... InitServer");
            string res = "";
            Thread t = new Thread(() => { StartServer(); });
            t.Start();
            string result = "started";
            return result;
        }

        /// <summary>
        /// Opens up a cmd window and runs the bat script located in the Scarf script path you have set in the config. This in turn activates a conda environment and starts the flask app.
        /// </summary>
        /// <returns></returns>
        public IEnumerator StartServer()
        {
            string result = string.Empty;
            CellexalLog.Log("               Start the web server R web API ... StartServer");
            Process currentProcess = Process.GetCurrentProcess();
            string serverType = "rWebApi";
            int pid = currentProcess.Id;
            string rScriptFilePath = Application.streamingAssetsPath + @"\R\start_webapi.R";
            string serverName = CellexalUser.UserSpecificFolder + "\\" + serverType + "Server";
            string dataSourceFolder;
            dataSourceFolder = Directory.GetCurrentDirectory() + @"\Data\" + CellexalUser.DataSourceFolder;


            string args = serverName + " " + dataSourceFolder + " " +
                          CellexalUser.UserSpecificFolder + " " + pid + " " + "9977";
            CellexalLog.Log("Running start webapi script at " + rScriptFilePath + " with the arguments " + args);
            string value = null;
            Thread t = new Thread(
                () => { value = RScriptRunner.RunFromCmd(rScriptFilePath, args, true); });
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            t.Start();

            while (!ServerActive())
            {
                if (value != null && !value.Equals(string.Empty))
                {
                    CellexalError.SpawnError("Failed to start R Server",
                        "Make sure you have set the correct R path in the launcher menu");
                    yield break;
                }

                yield return null;
            }

            stopwatch.Stop();
            CellexalLog.Log("Start Server finished in " + stopwatch.Elapsed.ToString());

        }

        private bool ServerActive()
        {
            throw new NotImplementedException();
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
        /// Calls the web api to retrieve the feature names present in the dataset. This can be gene names or other features.
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
        /// <param name="key"></param>
        public string LoadData()
        {
            StartCoroutine(CreateGraph());
            string ret = "started";
            return(ret);
        }

        /// <summary>
        /// Retrieves the graph coordinates and then creates the graph object.
        /// </summary>
        /// <param name="key">Key used to find the coordinates. E.g. RNA_UMAP or RNA_tSNE.</param>
        /// <returns></returns>
        private IEnumerator CreateGraph()
        {
            // https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.Post.html
            // WWWForm form = new WWWForm();
            // form.AddField("myField", "myData");
            CellexalLog.Log("So here we try to load the drc data from the R web api...");

            string reqURL = $"{url}get_coords/";
            UnityWebRequest req = UnityWebRequest.Get(reqURL);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
            {
                print(req.error);
                yield break;
            }

            string response = System.Text.Encoding.UTF8.GetString(req.downloadHandler.data);
            JObject jObject = JObject.Parse(response);
            
            var na = jObject["names"]
                .Children()
                .Select(v => v.Value<string>())
                .ToArray();
            string name;
            for ( int i = 0; i < na.Length; i ++)
            {
                name = na[i];
                var CellID = jObject[name].Next
                .Select(v => v.Value<string>())
                .ToList();
                var x = jObject[name].Next
                .Select(v => v.Value<float>())
                .ToList();
                var y = jObject[name].Next
                .Select(v => v.Value<float>())
                .ToList();
                var z = jObject[name].Next
                .Select(v => v.Value<float>())
                .ToList();
                Graph combGraph = ReferenceManager.instance.graphGenerator.CreateGraph(GraphGenerator.GraphType.MDS);
                combGraph.GraphName = name;
                ReferenceManager.instance.graphManager.originalGraphs.Add(combGraph);
                ReferenceManager.instance.graphManager.Graphs.Add(combGraph);
                ReferenceManager.instance.inputReader.mdsReader.CreateFromCoordinates(CellID, x, y, z);

                StartCoroutine(ReferenceManager.instance.graphGenerator.SliceClusteringLOD(1));
                while (ReferenceManager.instance.graphGenerator.isCreating)
                    yield return null;
            }
            
            

            RwebapiActive = true;
            CellexalEvents.GraphsLoaded.Invoke();

        }

    }
}

