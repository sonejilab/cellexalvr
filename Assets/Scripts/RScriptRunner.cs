using System;
using System.Diagnostics;
using System.IO;

/// <summary>
/// This class runs R code from a file using the console.
/// </summary>
public class RScriptRunner
{

    /// <summary>
    /// Runs an R script from a file using Rscript.exe.
    /// Example:
    ///   RScriptRunner.RunFromCmd(curDirectory + @"\ImageClustering.r", "rscript.exe", curDirectory.Replace('\\','/'));
    /// Getting args passed from C# using R:
    ///   args = commandArgs(trailingOnly = TRUE)
    ///   print(args[1]);
    /// </summary>
    /// <param name="rCodeFilePath"> File where your R code is located. </param>
    /// <param name="rScriptExecutablePath"> Usually only requires "rscript.exe" </param>
    /// <param name="args"> Multiple R args can be seperated by spaces. </param>
    /// <returns>Returns a string with the R responses.</returns>

    public static string RunFromCmd(string rCodeFilePath, string args)
    {
        string result = string.Empty;
        try
        {
            string home = Directory.GetCurrentDirectory();
            using (StreamReader r = new StreamReader(home + "Assets/Config/config.txt"))
            {
                string rawInput = r.ReadToEnd();
                string[] input = rawInput.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                string rPath = input[0];
                //r.Close();

                var info = new ProcessStartInfo();
                info.FileName = rPath;
                info.WorkingDirectory = Path.GetDirectoryName(rPath);
                info.Arguments = home + rCodeFilePath + " " + args;
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
                      new StreamWriter(Directory.GetCurrentDirectory() + "/Assets/Config/r_log.txt"))
                {
                    writetofile.WriteLine(result);
                    writetofile.Flush();
                    writetofile.Close();
                }
                return result;
            }

        }
        catch (Exception ex)
        {
            using (StreamWriter writetofile =
                       new StreamWriter(Directory.GetCurrentDirectory() + "/Assets/Config/error.txt"))
            {
                writetofile.WriteLine("R Script failed: " + ex);
                writetofile.Flush();
                writetofile.Close();
            }
            throw new Exception("R Script failed: " + result, ex);
        }
    }


}
