using UnityEngine;
using System.IO;
using System;

/// <summary>
/// This class generates the boxes that represents folders with input data.
/// </summary>
public class InputFolderGenerator : MonoBehaviour
{

    public GameObject folderPrefab;
    // 6 is the number of boxes on each "floor"
    private Vector3[] folderBaseCoords = new Vector3[6];

    void Start()
    {
        // the - 2 comes from subtracting the runtimeGroups directory and then one more for the integer division to work as intended.
        var folderAngle = -(Math.PI * 1.1d) / 2d;
        folderBaseCoords = new Vector3[6];
        for (int i = 0; i < 3; ++i)
        {
            folderBaseCoords[i] = new Vector3((float)Math.Cos(folderAngle), 0, (float)Math.Sin(folderAngle));
            folderAngle -= (Math.PI * .9d) / 6d;
        }
        int j = 1;
        for (int i = 3; i < 6; ++i, j += 2)
        {
            folderBaseCoords[i] = new Vector3(folderBaseCoords[i - j].x, 0, -folderBaseCoords[i - j].z);
        }
        GenerateFolders();
    }

    /// <summary>
    /// Generates the boxes that represent folders.
    /// </summary>
    public void GenerateFolders()
    {
        if (!Directory.Exists(Directory.GetCurrentDirectory() + "/Data/runtimeGroups"))
        {
            print("creating runtimeGroups directory");
            CellExAlLog.Log("Creating directory " + heatmapDirectory);
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/Data/runtimeGroups");
        }

        string[] directories = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/Data");
        if (directories.Length == 0)
        {
            print("No input directeries found");
            return;
        }

        var nfolder = 0;
        foreach (string directory in directories)
        {
            if (directory.Substring(directory.Length - 13) == "runtimeGroups")
            {
                continue;
            }
            Vector3 heightVector = new Vector3(0f, 1 + nfolder / 6, 0f);
            GameObject newFolder = Instantiate(folderPrefab, folderBaseCoords[nfolder % 6] + heightVector, Quaternion.identity);
            newFolder.GetComponentInChildren<CellsToLoad>().Directory = directory;
            newFolder.transform.parent = transform;
            newFolder.transform.LookAt(transform.position + heightVector - new Vector3(0f, 1f, 0f));
            newFolder.transform.Rotate(0, -90f, 0);
            newFolder.GetComponentInChildren<CellsToLoad>().SavePosition();
            nfolder++;
            int forwardSlashIndex = directory.LastIndexOf('/');
            int backwardSlashIndex = directory.LastIndexOf('\\');
            string croppedDirectoryName;

            // Handle both forwardslash and backwardslash
            if (backwardSlashIndex > forwardSlashIndex)
            {
                croppedDirectoryName = directory.Substring(backwardSlashIndex + 1);
            }
            else
            {
                croppedDirectoryName = directory.Substring(forwardSlashIndex + 1);
            }

            // Set text on folder box
            newFolder.GetComponentInChildren<TextMesh>().text = croppedDirectoryName;
        }
    }

    /// <summary>
    /// Destroys all folders.
    /// </summary>
    public void DestroyFolders()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
