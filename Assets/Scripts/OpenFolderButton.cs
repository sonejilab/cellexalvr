using UnityEngine;
using System.IO;

public class OpenFolderButton : MonoBehaviour
{
    public string path;

    public void OpenFolder()
    {
        string currentDir = Directory.GetCurrentDirectory();
        System.Diagnostics.Process.Start(currentDir + "\\" + path);
    }
}
