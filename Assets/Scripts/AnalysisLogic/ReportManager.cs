using UnityEngine;
using System.Collections;
using CellexalVR.General;
using System.IO;
using System;
using CellexalVR.AnalysisObjects;
using System.Threading;
using CellexalVR.Extensions;
using CellexalVR.Menu.Buttons.Report;

namespace CellexalVR.AnalysisLogic
{

    public class ReportManager : MonoBehaviour
    {
        public ReferenceManager referenceManager;

        private bool goAnalysisRunning;
        // Use this for initialization
        void Start()
        {

        }

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        /// <summary>
        /// Calls R logging function to start the logging session.
        /// </summary>
        public IEnumerator LogStart()
        {

            //string script = "if ( !is.null(cellexalObj@usedObj$sessionPath) ) { \n" +
            //                "cellexalObj @usedObj$sessionPath = NULL \n" +
            //                " cellexalObj @usedObj$sessionRmdFiles = NULL \n" +
            //                "cellexalObj @usedObj$sessionName = NULL } \n " +
            //                "cellexalObj = sessionPath(cellexalObj, \"" + CellexalUser.UserSpecificFolder.UnFixFilePath() + "\")" ;

            string args = CellexalUser.UserSpecificFolder.MakeDoubleBackslash();
            string rScriptFilePath = Path.Combine(Application.streamingAssetsPath, "R", "logStart.R");

            // Wait for other processes to finish and for server to have started.
            string mainserverPidPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.pid");
            string mainserverInputPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.R");
            string mainserverInputLockPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.lock");

            bool rServerReady = File.Exists(mainserverPidPath)
                                && !File.Exists(mainserverInputPath)
                                && !File.Exists(mainserverInputLockPath);
            while (referenceManager.selectionManager.RObjectUpdating || !rServerReady || !RScriptRunner.serverIdle)
            {
                rServerReady = File.Exists(mainserverPidPath)
                               && !File.Exists(mainserverInputPath)
                               && !File.Exists(mainserverInputLockPath);
                yield return null;
            }
            CellexalLog.Log("Running R script : " + rScriptFilePath);
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            Thread t = new Thread(() => RScriptRunner.RunRScript(rScriptFilePath, args));
            t.Start();

            // Wait for this process to finish.
            while (t.IsAlive || File.Exists(mainserverPidPath))
            {
                yield return null;
            }
            stopwatch.Stop();
            CellexalLog.Log("R log script finished in " + stopwatch.Elapsed.ToString());
            CellexalEvents.ScriptFinished.Invoke();
        }

        /// <summary>
        /// Calls R logging function to stop the logging session.
        /// </summary>
        public IEnumerator LogStop(SaveButton saveButton)
        {
            saveButton.descriptionText.text = "Compiling report..";
            referenceManager.floor.StartPulse();
            string args = CellexalUser.UserSpecificFolder.MakeDoubleBackslash();
            string rScriptFilePath = Path.Combine(Application.streamingAssetsPath, "R", "logStart.R");

            string mainserverPidPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.pid");
            string mainserverInputPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.R");
            string mainserverInputLockPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.lock");

            bool rServerReady = File.Exists(mainserverPidPath)
                                && !File.Exists(mainserverInputPath)
                                && !File.Exists(mainserverInputLockPath);
            while (referenceManager.selectionManager.RObjectUpdating || !rServerReady || !RScriptRunner.serverIdle)
            {
                rServerReady = File.Exists(mainserverPidPath)
                               && !File.Exists(mainserverInputPath)
                               && !File.Exists(mainserverInputLockPath);
                yield return null;
            }
            CellexalLog.Log("Running R script " + CellexalLog.FixFilePath(rScriptFilePath) + " with the arguments \"" + args + "\"");
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            Thread t = new Thread(() => RScriptRunner.RunRScript(rScriptFilePath, args));
            t.Start();

            while (t.IsAlive || File.Exists(mainserverInputPath))
            {
                yield return null;
            }

            stopwatch.Stop();
            CellexalLog.Log("R log script finished in " + stopwatch.Elapsed.ToString());

            yield return null;
            CellexalEvents.ScriptFinished.Invoke();
            saveButton.changeSprite = false;
            saveButton.descriptionText.text = "";
            saveButton.SetButtonActivated(true);
            referenceManager.notificationManager.SpawnNotification("Session report compiled.");
            //ZipFile.CreateFromDirectory(startPath, zipPath);
            //reportList.GenerateList();

            //SendIt();
            //string startPath = @"c:\example\start";
            //string zipPath = @"c:\example\result.zip";
        }

        #region Heatmap

        /// <summary>
        /// Calls R logging function to save heatmap for session report.
        /// </summary>
        public IEnumerator LogHeatmap(string heatmapImageFilePath, Heatmap heatmap)
        {
            heatmap.removable = true;
            //CellexalEvents.ScriptRunning.Invoke();
            heatmap.saveImageButton.SetButtonActivated(false);
            heatmap.statusText.text = "Saving Heatmap...";
            referenceManager.floor.StartPulse();
            string genesFilePath = Path.Combine(CellexalUser.UserSpecificFolder, "Heatmap", heatmap.name + ".txt");
            string groupingsFilepath = heatmap.selection.savedSelectionFilePath;
            string rScriptFilePath = Path.Combine(Application.streamingAssetsPath, "R", "logHeatmap.R");
            string args = CellexalUser.UserSpecificFolder.MakeDoubleBackslash() + " " + genesFilePath.MakeDoubleBackslash() + " " + heatmapImageFilePath.MakeDoubleBackslash() + " " + groupingsFilepath;

            string mainserverPidPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.pid");
            string mainserverInputPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.R");
            string mainserverInputLockPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.lock");

            bool rServerReady = File.Exists(mainserverPidPath)
                                && !File.Exists(mainserverInputPath)
                                && !File.Exists(mainserverInputLockPath);

            while (referenceManager.selectionManager.RObjectUpdating || !rServerReady || !RScriptRunner.serverIdle)
            {
                rServerReady = File.Exists(mainserverPidPath)
                               && !File.Exists(mainserverInputPath)
                               && !File.Exists(mainserverInputLockPath);
                yield return null;
            }
            CellexalLog.Log("Running R script " + rScriptFilePath + " with the arguments \"" + args + "\"");
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            Thread t = new Thread(() => RScriptRunner.RunRScript(rScriptFilePath, args));
            t.Start();

            while (t.IsAlive || File.Exists(mainserverInputPath))
            {
                yield return null;
            }
            stopwatch.Stop();
            CellexalLog.Log("R log script finished in " + stopwatch.Elapsed.ToString());
            heatmap.saveImageButton.FinishedButton();
            heatmap.statusText.text = "";
            yield return null;
            CellexalEvents.ScriptFinished.Invoke();
            heatmap.removable = false;
            //saveImageButton.SetButtonActivated(true);
        }

        /// <summary>
        /// Does a GO analysis of the genes on the heatmap. The Rscript does this and needs the genelist to do it.
        /// </summary>
        public void GOanalysis(Heatmap heatmap)
        {
            heatmap.goAnalysisButton.SetButtonActivated(false);
            string goAnalysisDirectory = CellexalUser.UserSpecificFolder;
            if (!Directory.Exists(goAnalysisDirectory))
            {
                Directory.CreateDirectory(goAnalysisDirectory);
                CellexalLog.Log("Created directory " + goAnalysisDirectory);
            }

            goAnalysisDirectory = Path.Combine(goAnalysisDirectory, "Heatmap");
            if (!Directory.Exists(goAnalysisDirectory))
            {
                Directory.CreateDirectory(goAnalysisDirectory);
                CellexalLog.Log("Created directory " + goAnalysisDirectory);
            }
            StartCoroutine(GOAnalysis(goAnalysisDirectory, heatmap));
        }

        /// <summary>
        /// Calls the R function with the filepath to the genes to analyse (this is the same as the heatmap directory).
        /// </summary>
        /// <param name="goAnalysisDirectory"></param>
        /// <returns></returns>
        IEnumerator GOAnalysis(string goAnalysisDirectory, Heatmap heatmap)
        {
            heatmap.statusText.text = "Doing GO Analysis...";
            referenceManager.floor.StartPulse();
            goAnalysisRunning = true;
            heatmap.removable = true;
            string genesFilePath = Path.Combine(CellexalUser.UserSpecificFolder, "Heatmap", heatmap.name + ".txt");
            string rScriptFilePath = Path.Combine(Application.streamingAssetsPath, "R", "GOanalysis.R");
            string groupingsFilepath = heatmap.selection.savedSelectionFilePath;
            string args = CellexalUser.UserSpecificFolder.MakeDoubleBackslash() + " " + genesFilePath.MakeDoubleBackslash() + " " + groupingsFilepath.MakeDoubleBackslash();

            string mainserverPidPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.pid");
            string mainserverInputPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.R");
            string mainserverInputLockPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.lock");

            bool rServerReady = File.Exists(mainserverPidPath)
                                && !File.Exists(mainserverInputPath)
                                && !File.Exists(mainserverInputLockPath);
            while (referenceManager.selectionManager.RObjectUpdating || !rServerReady || !RScriptRunner.serverIdle)
            {
                rServerReady = File.Exists(mainserverPidPath)
                               && !File.Exists(mainserverInputPath)
                               && !File.Exists(mainserverInputLockPath);
                yield return null;
            }
            CellexalLog.Log("Running R script " + rScriptFilePath + " with the arguments \"" + args + "\"");
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            Thread t = new Thread(() => RScriptRunner.RunRScript(rScriptFilePath, args));
            t.Start();
            while (t.IsAlive || File.Exists(mainserverInputPath))
            {
                yield return null;
            }
            stopwatch.Stop();
            CellexalLog.Log("R log script finished in " + stopwatch.Elapsed.ToString());
            CellexalEvents.ScriptFinished.Invoke();
            heatmap.statusText.text = "";
            heatmap.goAnalysisButton.FinishedButton();
            heatmap.removable = false;
            goAnalysisRunning = false;
        }
        #endregion

        #region Networks

        /// <summary>
        /// Calls R logging function to save network for session report.
        /// </summary>
        public IEnumerator LogNetwork(string networkImageFilePath, NetworkCenter nc)
        {
            //handler.runningScript = true;
            CellexalEvents.ScriptRunning.Invoke();
            referenceManager.floor.StartPulse();
            nc.saveImageButton.SetButtonActivated(false);
            nc.saveImageButton.descriptionText.text = "Saving image...";
            string groupingsFilepath = nc.Handler.selection.savedSelectionFilePath;
            string args = CellexalUser.UserSpecificFolder.MakeDoubleBackslash() + " " + networkImageFilePath.MakeDoubleBackslash() + " " + groupingsFilepath.MakeDoubleBackslash();
            string rScriptFilePath = Path.Combine(Application.streamingAssetsPath, "R", "logNetwork.R");

            string mainserverPidPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.pid");
            string mainserverInputPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.R");
            string mainserverInputLockPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.lock");

            bool rServerReady = File.Exists(mainserverPidPath)
                                && !File.Exists(mainserverInputPath)
                                && !File.Exists(mainserverInputLockPath);
            while (referenceManager.selectionManager.RObjectUpdating || !rServerReady || !RScriptRunner.serverIdle)
            {
                rServerReady = File.Exists(mainserverPidPath)
                               && !File.Exists(mainserverInputPath)
                               && !File.Exists(mainserverInputLockPath);
                yield return null;
            }
            CellexalLog.Log("Running R script " + CellexalLog.FixFilePath(rScriptFilePath) + " with the arguments \"" + args + "\"");
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            Thread t = new Thread(() => RScriptRunner.RunRScript(rScriptFilePath, args));
            t.Start();

            while (t.IsAlive || File.Exists(mainserverInputPath))
            {
                yield return null;
            }
            stopwatch.Stop();
            CellexalLog.Log("R log script finished in " + stopwatch.Elapsed.ToString());
            nc.saveImageButton.FinishedButton();
            //handler.runningScript = false;
            yield return null;
            CellexalEvents.ScriptFinished.Invoke();
            nc.saveImageButton.descriptionText.text = "";

        }

        #endregion

    }
}
