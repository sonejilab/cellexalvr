using System;
using System.Diagnostics;
using System.IO;
using CellexalVR.General;

namespace CellexalVR.AnalysisLogic
{

    /// <summary>
    /// This class runs R code from a file using the console or runs a command via an R server session.
    /// </summary>
    public class RScriptRunner
    {

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
        /// <returns>Returns a string with the R responses.</returns>

        public static string RunFromCmd(string rCodeFilePath, string args)
        {
            string result = string.Empty;
            try
            {
                string rPath = CellexalConfig.Config.RscriptexePath;

                var info = new ProcessStartInfo();
                info.FileName = rPath;
                info.WorkingDirectory = Path.GetDirectoryName(rPath);
                info.Arguments = rCodeFilePath + " " + args;
                info.RedirectStandardInput = false;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.UseShellExecute = false;
                info.CreateNoWindow = true;


                using (var proc = new Process())
                {
                    proc.StartInfo = info;
                    proc.Start();
                    result = "\nSTDOUT:\n" + proc.StandardOutput.ReadToEnd() + "\nSTDERR:\n" + proc.StandardError.ReadToEnd() + "\n----------\n";
                }
                using (StreamWriter writetofile =
                      new StreamWriter(Directory.GetCurrentDirectory() + "\\Output\\r_log.txt", true))
                {
                    writetofile.WriteLine(result);
                }
                //UnityEngine.Debug.Log("RESULT : " + result + " - Exit thread: " + result.Contains("Execution"));
                return result;


            }
            catch (Exception ex)
            {
                //CellexalLog.Log("Error, R script failed.", string.Format("The R script at {0} failed with the message {1}", rCodeFilePath, ex.ToString()));
                throw new Exception("R Script failed: " + result, ex);
            }
        }

        /// <summary>
        /// Run Rscript on a running r server session instead of starting a new process and loading the object again. 
        /// Send it to the server by writing the command to run into the "<servername>.input.R". 
        /// The R backend will read this file once every second and run the command inside it via "source(command(args))".
        /// </summary>
        /// <param name="function">The name of the R function to run. This will be the first thing written to the input.R file.</param>
        /// <param name="args">The arguments to the command. Split up from script string for readability.</param>
        public static string RunRscriptOnServer(string function, string args, string assignment="")
        {
            string result = string.Empty;
            string inputFilePath = CellexalUser.UserSpecificFolder + "\\server";
            if (!File.Exists(inputFilePath + ".input.R"))
            {
                //File.Create(inputFilePath + ".input.lock").Close();
                using (FileStream fs = File.Create(inputFilePath + ".input.lock"))
                {
                    using (StreamWriter file = new StreamWriter(inputFilePath + ".input.R"))
                    {
                        if (!assignment.Equals(string.Empty))
                        {
                            file.Write(assignment + " <- ");
                        }
                        file.Write(function);
                        file.Write('(');
                        file.Write(args + ')');
                        file.WriteLine();
                    }
                    //result = "\nSTDOUT:\n" + proc.StandardOutput.ReadToEnd() + "\nSTDERR:\n" + proc.StandardError.ReadToEnd() + "\n----------\n";
                    using (StreamWriter writetofile =
                            new StreamWriter(Directory.GetCurrentDirectory() + "\\Output\\r_log.txt", true))
                    {
                        writetofile.WriteLine(result);
                    }
                }
            }
            File.Delete(inputFilePath + ".input.lock");
            return result;
        }

        /// <summary>
        /// Run Rscript on a running r server session instead of starting a new process and loading the object again. 
        /// Send it to the server by writing the command to run into the "<servername>.input.R". 
        /// The R backend will read this file once every second and run the commands inside it via "source(input.R)".
        /// </summary>
        /// <param name="s">s is the full string to be written into the input.R file which the server will then read.</param>
        /// <param name="isFile">If the s argument instead is a filePath the function copies that entire file to input.R</param>
        public static string RunScript(string s, bool isFile=false)
        {
            string result = string.Empty;
            string inputFilePath = CellexalUser.UserSpecificFolder + "\\server";
            if (!File.Exists(inputFilePath + ".input.R"))
            {
                using (FileStream fs = File.Create(inputFilePath + ".input.lock"))
                {
                    if (isFile)
                    { 
                        File.Copy(s, inputFilePath + ".input.R");
                    }
                    else
                    {
                        File.WriteAllText(inputFilePath + ".input.R", s);
                        //file.Write(s);
                        //file.WriteLine();
                    }

                    using (StreamWriter writetofile =
                            new StreamWriter(Directory.GetCurrentDirectory() + "\\Output\\r_log.txt", true))
                    {
                        writetofile.WriteLine(result);
                    }
                }
            }
            File.Delete(inputFilePath + ".input.lock");
            return result;
        }


        /// <summary>
        /// To clean up server files after termination. Can be called from outside if the user wants to start a new session (e.g. when loading a new dataset). 
        /// </summary>
        public void CleanUp()
        {
            File.Delete(CellexalUser.UserSpecificFolder + "\\server.pid");
        }
    }
}
