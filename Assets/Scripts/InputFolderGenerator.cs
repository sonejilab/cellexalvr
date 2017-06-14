using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class InputFolderGenerator : MonoBehaviour
{

public GameObject folderPrefab;

// Use this for initialization
void Start () {
	string[] directories = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/Assets/Data");
	float offset = 0;
	foreach (string directory in directories) {
		if (directory.Substring(directory.Length - 13) == "runtimeGroups") {
			continue;
		}

		GameObject newFolder = Instantiate(folderPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z + offset), Quaternion.identity);
		newFolder.transform.Rotate(0f, 0f, -40f);
		newFolder.transform.parent = transform;
		int forwardSlashIndex = directory.LastIndexOf('/');
		int backwardSlashIndex = directory.LastIndexOf('\\');
		string croppedDirectoryName;
		if (backwardSlashIndex > forwardSlashIndex) {
			croppedDirectoryName = directory.Substring(backwardSlashIndex + 1);
		} else {
			croppedDirectoryName = directory.Substring(forwardSlashIndex + 1);
		}

		newFolder.GetComponentInChildren<TextMesh>().text = croppedDirectoryName;
		offset = offset + 1f;

	}

}
}
