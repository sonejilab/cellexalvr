using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System.IO;

public static class SQLitePostBuild
{
    [PostProcessBuildAttribute(0)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuildProject)
    {
        switch (target)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                pathToBuildProject = Path.Combine(Path.GetDirectoryName(pathToBuildProject), Path.GetFileNameWithoutExtension(pathToBuildProject) + "_Data");
                Debug.Log(Path.Combine(Application.dataPath, ListView.DictionaryList.editorDatabasePath) + ", " + Path.Combine(pathToBuildProject, ListView.DictionaryList.databasePath));
                File.Copy(Path.Combine(Application.dataPath, ListView.DictionaryList.editorDatabasePath), Path.Combine(pathToBuildProject, ListView.DictionaryList.databasePath));
                break;
            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneOSXIntel64:
            case BuildTarget.StandaloneOSXUniversal:
                pathToBuildProject = Path.Combine(Path.Combine(Path.GetDirectoryName(pathToBuildProject), Path.GetFileNameWithoutExtension(pathToBuildProject) + ".app"), "Contents");
                Debug.Log(Path.Combine(Application.dataPath, ListView.DictionaryList.editorDatabasePath) + ", " + Path.Combine(pathToBuildProject, ListView.DictionaryList.databasePath));
                File.Copy(Path.Combine(Application.dataPath, ListView.DictionaryList.editorDatabasePath), Path.Combine(pathToBuildProject, ListView.DictionaryList.databasePath));
                break;
        }
    }
}
