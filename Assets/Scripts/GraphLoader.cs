using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CellexalVR.AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.DesktopUI;
using CellexalVR.Extensions;
using CellexalVR.General;
using UnityEngine;
using Valve.VR;

namespace CellexalVR.AnalysisLogic
{
    public class GraphLoader : MonoBehaviour
    {
        public ReferenceManager _referenceManager;
        private readonly char[] separators = new char[] { ' ', '\t' };
        private PythonInterpreter _pythonInterpreter;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                _referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            _pythonInterpreter =
                GameObject.Find("PythonInterpreter").GetComponent<PythonInterpreter>();
        }

        [ConsoleCommand("graphLoader", aliases: new string[] { "loadgraph", "lg" })]
        public void LoadGraph(string coordFile, string graphName)
        {
            if (!File.Exists(coordFile)) return;
            LoadGraphCoroutine(coordFile, graphName);
            _referenceManager.graphGenerator.isCreating = true;
        }

        private void LoadGraphCoroutine(string coordFile, string graphName)
        {
            coordFile = coordFile.FixFilePath();
            Stopwatch totalTime = new Stopwatch();
            totalTime.Start();
            string path = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;
            string[] mdsFiles = Directory.GetFiles(path, "coordFile.txt");
            StartCoroutine(_referenceManager.inputReader.mdsReader.ReadMDSFiles(path, mdsFiles));
            //const float maximumDeltaTime = 0.05f; // 20 fps
            //int maximumItemsPerFrame = CellexalConfig.Config.GraphLoadingCellsPerFrameStartCount;
            //int totalNbrOfCells = 0;

            //while (_referenceManager.graphGenerator.isCreating)
            //{
            //    yield return null;
            //}

            //Graph combGraph = _referenceManager.graphGenerator.CreateGraph(GraphGenerator.GraphType.MDS);

            //combGraph.GraphName = graphName;
            //_referenceManager.graphManager.Graphs.Add(combGraph);
            //_referenceManager.graphManager.originalGraphs.Add(combGraph);
            //combGraph.hasVelocityInfo = false;
            //// _referenceManager.cellManager.

            //string[] axes = new string[3];
            //int i = 0;
            //using (StreamReader streamReader = new StreamReader(coordFile))
            //{
            //    string header = streamReader.ReadLine();
            //    if (header != null && header.Split(null)[0].Equals("CellID"))
            //    {
            //        string[] columns = header.Split(null).Skip(1).ToArray();
            //        Array.Copy(columns, 0, axes, 0, 3);
            //    }
            //    else if (header != null)
            //    {
            //        string[] words = header.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            //        if (words.Length != 4)
            //        {
            //            PythonInterpreter.WriteToOutput(
            //                $"Could not load graph. Wrong number of columns. Found {words.Length} columns but should be {4}");
            //        }

            //        string cellName = words[0];
            //        //_referenceManager.cellManager.cellNames.Add(cellName);
            //        float x = float.Parse(words[1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            //        float y = float.Parse(words[2], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            //        float z = float.Parse(words[3], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            //        Cell cell = _referenceManager.cellManager.AddCell(cellName);
            //        _referenceManager.graphGenerator.AddGraphPoint(cell, x, y, z);
            //        axes[0] = "x";
            //        axes[1] = "y";
            //        axes[2] = "z";
            //    }

            //    combGraph.axisNames = axes;
            //    while (!streamReader.EndOfStream)
            //    {
            //        int itemsThisFrame = 0;
            //        for (int j = 0; j < maximumItemsPerFrame && !streamReader.EndOfStream; ++j)
            //        {
            //            string[] words = streamReader.ReadLine()
            //                .Split(separators, StringSplitOptions.RemoveEmptyEntries);
            //            if (words.Length != 4 && words.Length != 7)
            //            {
            //                continue;
            //            }

            //            string cellName = words[0];
            //            float x = float.Parse(words[1],
            //                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            //            float y = float.Parse(words[2],
            //                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            //            float z = float.Parse(words[3],
            //                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            //            Cell cell = _referenceManager.cellManager.AddCell(cellName);
            //            //_referenceManager.cellManager.cellNames.Add(cellName);
            //            _referenceManager.graphGenerator.AddGraphPoint(cell, x, y, z);
            //            itemsThisFrame++;
            //        }

            //        totalNbrOfCells += itemsThisFrame;
            //        // wait for end of frame
            //        yield return null;

            //        float lastFrame = Time.deltaTime;
            //        if (lastFrame < maximumDeltaTime)
            //        {
            //            // we had some time over last frame
            //            maximumItemsPerFrame += CellexalConfig.Config.GraphLoadingCellsPerFrameIncrement;
            //        }
            //        else if (lastFrame > maximumDeltaTime && maximumItemsPerFrame >
            //            CellexalConfig.Config.GraphLoadingCellsPerFrameIncrement * 2)
            //        {
            //            // we took too much time last frame
            //            maximumItemsPerFrame -= CellexalConfig.Config.GraphLoadingCellsPerFrameIncrement;
            //        }
            //    }
            //}

            //_referenceManager.graphGenerator.SliceClustering();
            //_referenceManager.graphGenerator.AddAxes(combGraph, axes);
            //combGraph.SetInfoText();
            //while (_referenceManager.graphGenerator.isCreating)
            //{
            //    yield return null;
            //}

            // File.Delete(coordFile);
            totalTime.Stop();
            CellexalEvents.GraphsLoaded.Invoke();
            PythonInterpreter.WriteToOutput(
                $"Graph loaded in {totalTime.Elapsed} seconds. Graph consists of {_referenceManager.cellManager.GetCells().Length} unique cells");
        }
    }
}