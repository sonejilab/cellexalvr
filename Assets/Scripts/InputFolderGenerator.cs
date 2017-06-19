using UnityEngine;
using System.IO;
using System;

public class InputFolderGenerator : MonoBehaviour {

public GameObject folderPrefab;

// Use this for initialization
void Start () {
	string[] directories = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/Assets/Data");
	float folderAngle  = 0;

	foreach (string directory in directories) {
		if (directory.Substring(directory.Length - 13) == "runtimeGroups") {
			continue;
		}
		GameObject newFolder = Instantiate(folderPrefab, new Vector3((float)Math.Cos(folderAngle), 1.0f, (float)Math.Sin(folderAngle)), Quaternion.identity);
		newFolder.GetComponentInChildren<CellsToLoad>().SetDirectory(directory);
		newFolder.transform.parent = transform;
		newFolder.transform.LookAt(transform);
		newFolder.transform.Rotate(0, -90f, 0f);
		folderAngle += 360 / directories.Length;
		int forwardSlashIndex = directory.LastIndexOf('/');
		int backwardSlashIndex = directory.LastIndexOf('\\');
		string croppedDirectoryName;

		// Handle both forwardslash and backwardslash
		if (backwardSlashIndex > forwardSlashIndex) {
			croppedDirectoryName = directory.Substring(backwardSlashIndex + 1);
		} else {
			croppedDirectoryName = directory.Substring(forwardSlashIndex + 1);
		}

		// Set text on folser box
		newFolder.GetComponentInChildren<TextMesh>().text = croppedDirectoryName;
	}
}

}
