﻿using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using CellexalVR.General;

namespace CellexalVR.AnalysisLogic
{

    /// <summary>
    /// This class runs R code from a file using the console or runs a command via an R server session.
    /// </summary>
    public class RScriptRunner
    {
        public static ReferenceManager referenceManager;

        public static ArrayList geneResult = new ArrayList();
        public static bool serverIdle = true;

        public static void SetReferenceManager(ReferenceManager rm)
        {
            referenceManager = rm;
        }

        public static void SetRScriptPath(string path)
        {
            if (path.Equals(string.Empty)) return;

            if (File.Exists(path) && path.Contains(".exe"))
            {
                bool hasChanged = path != CellexalConfig.Config.RscriptexePath;
                CrossSceneInformation.RScriptPath = path;
                if (hasChanged)
                {
                    CellexalConfig.Config.RscriptexePath = path;
                    string configDir = Path.Combine(Directory.GetCurrentDirectory(), "Config");
                    string configPath = Path.Combine(configDir, "default_config.xml");
                    if (!Directory.Exists("Config"))
                    {
                        Directory.CreateDirectory("Config");
                        CellexalLog.Log("Created directory " + CellexalLog.FixFilePath(configDir));
                    }

                    CellexalLog.Log("Started saving the config file");

                    FileStream fileStream = new FileStream(configPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    StreamWriter streamWriter = new StreamWriter(fileStream);

                    XmlSerializer ser = new XmlSerializer(typeof(Config));
                    ser.Serialize(streamWriter, CellexalConfig.Config);
                    streamWriter.Close();
                    fileStream.Close();

                }
            }
        }

        private static bool readGenes;
        private static bool sendGenes;

        /// <summary>
        /// Runs an R script from a file using Rscript.exe.
        /// Example:
        ///   RScriptRunner.RunFromCmd(curDirectory + @"\ImageClustering.r", curDirectory.Replace('\\','/'));
        /// Getting args passed from C# using R:
        ///   args = commandArgs(trailingOnly = TRUE)
        ///   print(args[1]);
        /// </summary>
        /// <param name="rCodeFilePath"> File where your R code is located. </param>
        /// <param name="args"> Multiple R args can be seperated by spaces. </param>
        /// <param name="writeOut"> If the stdout and stderror should be appended to the R log. </param>
        /// <returns>Returns a string with the R responses.</returns>
        public static string RunFromCmd(string rCodeFilePath, string args, bool writeOut)
        {
            string result = string.Empty;
            try
            {
                string rPath = CellexalConfig.Config.RscriptexePath;

                var info = new ProcessStartInfo
                {
                    FileName = rPath,
                    WorkingDirectory = Path.GetDirectoryName(rPath),
                    Arguments = rCodeFilePath + " " + args,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var proc = new Process();
                proc.StartInfo = info;
                proc.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
                {
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        using (StreamWriter stderrorWriter =
                                new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "Output", "r_log.txt"), true))
                        {
                            stderrorWriter.WriteLine("\n STDERROR: " + e.Data);
                        }
                    }
                });

                proc.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                {
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        if (writeOut)
                        {
                            using (StreamWriter stdoutWriter =
                                    new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "Output", "r_log.txt"), true))
                            {
                                stdoutWriter.WriteLine("\n STDOUT: " + e.Data);
                            }

                        }
                    }
                });

                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                //CellexalLog.Log("R script finished. stderr:", proc.StandardError.ReadToEnd());
                proc.WaitForExit();

                proc.Close();
                return result;

            }

            catch (Exception ex)
            {
                //CellexalLog.Log("Error, R script failed.", string.Format("The R script at {0} failed with the message {1}", rCodeFilePath, ex.ToString()));
                //if (ex.GetType() == typeof(System.ComponentModel.Win32Exception))
                //{
                //    throw new Exception("Rrrr Script failed: " + result, ex);

                //}
                if (ex.GetType() == typeof(System.ComponentModel.Win32Exception) ||
                    ex.GetType() == typeof(ArgumentException))
                {
                    return "Failed to Start Server";
                    //CellexalEvents.ScriptFailed.Invoke();
                }
                throw new Exception("R Script failed: " + result, ex);
            }
            //catch (System.ComponentModel.Win32Exception win32ex)
            //{
            //    CellexalError.SpawnError("Failed to start R Server", "Check if you have set the correct R path in the launcher menu");
            //    throw new Exception("Rrrr Script failed: " + result, win32ex);
            //}
        }

        static void GeneProcHandler(object sender, DataReceivedEventArgs e)
        {
            //new StreamWriter(Directory.GetCurrentDirectory() + "\\Output\\gene_log.txt", true))
            using (StreamWriter geneWriter =
                new StreamWriter(Path.Combine(CellexalUser.UserSpecificFolder, "gene_expr.txt"), false))
            {

                geneWriter.WriteLine(e.Data);
            }
            //if (!String.IsNullOrEmpty(e.Data))
            //{
            //    if (e.Data == "BEGIN")
            //    {
            //        // START
            //        UnityEngine.Debug.Log("begin");
            //        readGenes = true;
            //    }

            //    else if (e.Data == "END")
            //    {
            //        // STOP
            //        UnityEngine.Debug.Log("end");
            //        readGenes = false;
            //        UnityEngine.Debug.Log("send genes");
            //        //referenceManager.cellManager.HandleGeneResult();
            //        CellexalEvents.GeneExpressionsRetrieved.Invoke(); 
            //        geneResult.Clear();
            //    }

            //    else if (readGenes)
            //    {
            //        string[] words = e.Data.Split(' ');
            //        string name = words[0];
            //        float expr = float.Parse(words[1]);
            //        geneResult.Add(new Tuple<string, float>(name, expr));
            //    }
            //    //else if (sendGenes)
            //    //{

            //    //    sendGenes = false;
            //    //}
            //    //referenceManager.cellManager.expressions.Add();
            //    else
            //    {
            //        using (StreamWriter geneWriter =
            //            new StreamWriter(Directory.GetCurrentDirectory() + "\\Output\\gene_log.txt", true))
            //        {

            //            geneWriter.WriteLine(e.Data);
            //        }
            //    }

        }


        /// <summary>
        /// Run R code on a running r server session instead of starting a new process and loading the object again. 
        /// Send it to the server by writing the command to run into the "<servername>.input.R". 
        /// The R backend will read this file once every second and run the commands inside it via "source(input.R)".
        /// </summary>
        /// <param name="s">s is the full string to be written into the input.R file which the server will then read.</param>
        /// <param name="isFile">If the s argument instead is a filePath the function copies that entire file to input.R</param>
        public static void WriteToServer(string s)
        {
            string inputFilePath = Path.Combine(CellexalUser.UserSpecificFolder, "server.input.R");
            File.WriteAllText(inputFilePath, s);
            //if (!File.Exists(inputFilePath + ".input.lock"))
            //{
            //    using (FileStream fs = File.Create(inputFilePath + ".input.lock"))
            //    {
            //    }
            //    File.Delete(inputFilePath + ".input.lock");
            //}
        }

        /// <summary>
        /// Runs an R script.
        /// </summary>
        /// <param name="path">The path to the R script.</param>
        /// <param name="args">The arguments, seperated by spaces.</param>
        /// <returns>The result from the R process.</returns>
        public static string RunRScript(string path, string args = "")
        {

            string mainserverPidPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.pid");
            string mainserverInputPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.R");
            string mainserverInputLockPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.lock");

            bool rServerReady = File.Exists(mainserverPidPath) &&
                                !File.Exists(mainserverInputPath) &&
                                !File.Exists(mainserverInputLockPath);

            while (!rServerReady || !RScriptRunner.serverIdle)
            {
                rServerReady = File.Exists(mainserverPidPath) &&
                                !File.Exists(mainserverInputPath) &&
                                !File.Exists(mainserverInputLockPath);

                System.Threading.Thread.Sleep(50);
            }

            serverIdle = false;
            string result = null;
            string inputFilePath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.lock");
            if (!File.Exists(inputFilePath))
            {
                try
                {
                    using (FileStream fs = File.Create(inputFilePath))
                    {
                        result = RunFromCmd(path, args, true);
                    }
                }
                catch (Exception e)
                {
                    CellexalLog.Log(e.Source, e.Message, e.StackTrace);
                    // Command Failed. Removing lock file on input.R 
                    File.Delete(inputFilePath);
                }
                File.Delete(inputFilePath);
            }
            serverIdle = true;
            return result;
        }


    }
}
