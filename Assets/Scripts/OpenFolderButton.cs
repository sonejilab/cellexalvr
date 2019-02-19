using UnityEngine;
using System.IO;

public class OpenFolderButton : MonoBehaviour
{
    public string path;

    public void OpenFolder()
    {
        string currentDir = Directory.GetCurrentDirectory();
        string fullPath = currentDir + "\\" + path;
        if (path == "$LOG")
        {
            CellexalLog.LogBacklog();
            fullPath = CellexalLog.LogFilePath;
        }
        System.Diagnostics.Process.Start(fullPath);
    }
}
