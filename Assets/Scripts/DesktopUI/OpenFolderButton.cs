using CellexalVR.General;
using System;
using System.IO;
using UnityEngine;

namespace CellexalVR.DesktopUI
{
    /// <summary>
    /// Represents the buttons that open a folder or a file.
    /// </summary>
    public class OpenFolderButton : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public string path;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }


        public void OpenFolder()
        {
            string currentDir = Directory.GetCurrentDirectory();
            string fullPath = currentDir + "\\" + path;
            if (path == "$LOG")
            {
                CellexalLog.LogBacklog();
                fullPath = CellexalLog.LogFilePath;
            }
            else if (path == "$UNITY_LOG")
            {
#if UNITY_EDITOR
                fullPath = Path.Combine(Environment.GetEnvironmentVariable("AppData"), "..", "Local", "Unity", "Editor", "Editor.log");
#else
                fullPath = Path.Combine(Environment.GetEnvironmentVariable("AppData"), "..", "LocalLow", Application.companyName, Application.productName, "Player.log");
#endif
            }
            else if (path == "$SCRIPTS_FOLDER")
            {
                fullPath = Path.Combine(Application.streamingAssetsPath, "R");
            }
            else if (path == "$CONFIG")
            {
                fullPath = referenceManager.configManager.currentProfileFullPath;
            }

            System.Diagnostics.Process.Start(fullPath);
        }
    }
}