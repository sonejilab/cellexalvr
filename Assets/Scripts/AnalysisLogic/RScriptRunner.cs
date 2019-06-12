using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using CellexalVR.General;

namespace CellexalVR.AnalysisLogic
{

    /// <summary>
    /// This class runs R code from a file using the console or runs a command via an R server session.
    /// </summary>
    public class RScriptRunner
    {

        private static StringBuilder output;
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
                //output = new StringBuilder();
                proc.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
                {
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        if (writeOut)
                        {
                            using (StreamWriter stderrorWriter =
                                  new StreamWriter(Directory.GetCurrentDirectory() + "\\Output\\r_log.txt", true))
                            {
                                stderrorWriter.WriteLine("\n STDERROR: " + e.Data);
                            }
                        }
                    }
                });
                proc.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                {
                    // Prepend line numbers to each line of the output.
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        if (writeOut)
                        {
                            //output.Append("\n STDOUT: " + e.Data);
                            using (StreamWriter stdoutWriter =
                                  new StreamWriter(Directory.GetCurrentDirectory() + "\\Output\\r_log.txt", true))
                            {
                                stdoutWriter.WriteLine("\n STDOUT: " + e.Data);
                            }
                        }
                        //output.Append("\n -: " + e.Data);
                    }
                });
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();

                result = "\nSTDOUT:\n" + proc.StandardOutput.ReadToEnd() + "\nSTDERR:\n" + proc.StandardError.ReadToEnd() + "\n----------\n";
                //using (var proc = new Process())
                //{
                //    proc.StartInfo = info;
                //    proc.Start();
                //    result = "\nSTDOUT:\n" + proc.StandardOutput.ReadToEnd() + "\nSTDERR:\n" + proc.StandardError.ReadToEnd() + "\n----------\n";
                //}

                //UnityEngine.Debug.Log("RESULT : " + result + " - Exit thread: " + result.Contains("Execution"));
                proc.Close();
                return result;

            }
            catch (Exception ex)
            {
                //CellexalLog.Log("Error, R script failed.", string.Format("The R script at {0} failed with the message {1}", rCodeFilePath, ex.ToString()));
                throw new Exception("R Script failed: " + result, ex);
            }
        }

        static void MyProcOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                Debug.WriteLine(outLine.Data);

            }
        }
        
        /// <summary>
        /// Run Rscript on a running r server session instead of starting a new process and loading the object again. 
        /// Send it to the server by writing the command to run into the "<servername>.input.R". 
        /// The R backend will read this file once every second and run the commands inside it via "source(input.R)".
        /// </summary>
        /// <param name="s">s is the full string to be written into the input.R file which the server will then read.</param>
        /// <param name="isFile">If the s argument instead is a filePath the function copies that entire file to input.R</param>
        public static void RunScript(string s)
        {
            string inputFilePath = CellexalUser.UserSpecificFolder + "\\server";
            File.WriteAllText(inputFilePath + ".input.R", s);
            //if (!File.Exists(inputFilePath + ".input.lock"))
            //{
            //    using (FileStream fs = File.Create(inputFilePath + ".input.lock"))
            //    {
            //    }
            //    File.Delete(inputFilePath + ".input.lock");
            //}
        }

        public static string RunRScript(string path, string args = "")
        {
            string result = null;
            string inputFilePath = CellexalUser.UserSpecificFolder + "\\server";
            if (!File.Exists(inputFilePath + ".input.lock"))
            {
                try
                {
                    using (FileStream fs = File.Create(inputFilePath + ".input.lock"))
                    {
                        result = RunFromCmd(path, args, true);
                    }
                }
                catch (Exception)
                {
                    // Command Failed. Removing lock file on input.R 
                    File.Delete(inputFilePath + ".input.lock");
                }
                File.Delete(inputFilePath + ".input.lock");
            }
            return result;
        }


    }
}
