using UnityEngine;
using System.IO;
using System;

/// <summary>
/// Generates the boxes that represents folders with input data.
/// </summary>
public class InputFolderGenerator : MonoBehaviour
{

    public GameObject folderPrefab;
    // 6 is the number of boxes on each "floor"
    private Vector3[] folderBaseCoords = new Vector3[6];

    void Start()
    {
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
        string dataDirectory = Directory.GetCurrentDirectory() + "\\Data";
        string[] directories = Directory.GetDirectories(dataDirectory);
        if (directories.Length == 0)
        {
            CellexalLog.Log("No data folders found. Aborting loading.");
            return;
        }
        CellexalLog.Log("Started generating folders from " + CellexalLog.FixFilePath(dataDirectory));

        var nfolder = 0;
        foreach (string directory in directories)
        {
            Vector3 heightVector = new Vector3(0f, 1 + nfolder / 6, 0f);
            GameObject newFolder = Instantiate(folderPrefab, folderBaseCoords[nfolder % 6] + heightVector, Quaternion.identity);
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
            newFolder.GetComponentInChildren<CellsToLoad>().Directory = croppedDirectoryName;
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
