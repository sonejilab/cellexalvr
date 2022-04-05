using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CellexalVR.Extensions;
using CellexalVR.General;
using UnityEngine;

namespace CellexalVR
{
    public class PythonInterpreter : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public static string workingDirName = Directory.GetCurrentDirectory().FixFilePath();

        private float time = 0;
        private static readonly string path = (workingDirName + "/commandfile.txt").FixFilePath();
        private static readonly string lockFile = (workingDirName + "/lockfile.txt").FixFilePath();
        private static readonly string outputFile = (workingDirName + "/output.txt").FixFilePath();
        

        private void Start()
        {
            // Thread t = new Thread(
            //     () =>
            //     {
            //         string value = StartInterpreter();
            //     });
            // t.Start();
        }

        private static string StartInterpreter()
        {
            string result = string.Empty;
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                WorkingDirectory = workingDirName.FixFilePath(),
                Arguments = "python"
            };

            // Process proc = Process.Start("powershell.exe", "python");
            var proc = new Process();
            proc.StartInfo = info;
            proc.Start();
            proc.WaitForExit();
            proc.Close();
            return result;
        }

        private void Update()
        {
            time += Time.deltaTime;
            if (time < 1) return;
            if (!File.Exists(path)) return;
            if (File.Exists(lockFile)) return;
            using (StreamReader streamReader = new StreamReader(path))
            {
                string command = streamReader.ReadLine();
                referenceManager.consoleManager.EnterCommand(command);
            }

            File.Delete(path);
            time = 0f;
        }

        public static void WriteToOutput(string output)
        {
            using (StreamWriter streamWriter = new StreamWriter(outputFile))
            {
                streamWriter.WriteLine(output);
            }
            
        }
        
    }
}