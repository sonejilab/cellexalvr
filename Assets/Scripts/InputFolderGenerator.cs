using UnityEngine;
using System.IO;
using System;

public class InputFolderGenerator : MonoBehaviour {

public GameObject folderPrefab;

// Use this for initialization
void Start () {
	string[] directories = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/Assets/Data");
	float i = 0;
	Vector3 point1 = new Vector3(1.5f, 0, 0);
	Vector3 point2 = new Vector3(-1.5f, 0, 0);
	Vector3 center = (point1 + point2) * 0.5f;
	center -= new Vector3(1,0,1);
	Vector3 point1RelCenter = point1 - center;
	Vector3 point2RelCenter = point2 - center;
	float folderAngle  = 0;
	foreach (string directory in directories) {
		if (directory.Substring(directory.Length - 13) == "runtimeGroups") {
			continue;
		}
		//Vector3 folderPosition = Vector3.Slerp(point1RelCenter, point2RelCenter, i / directories.Length);
		//ameObject newFolder = Instantiate(folderPrefab, folderPosition, Quaternion.identity);
		//newFolder.transform.Rotate(0f, i*360/directories.Length, -40f);
		GameObject newFolder = Instantiate(folderPrefab, new Vector3((float)Math.Cos(folderAngle), 1.0f, (float)Math.Sin(folderAngle)), Quaternion.identity);
		newFolder.GetComponentInChildren<CellsInBox>().SetDirectory(directory);
		newFolder.transform.parent = transform;
		newFolder.transform.LookAt(transform);
		newFolder.transform.Rotate(0, -90f, 0f);
		folderAngle += 360 / directories.Length;
		int forwardSlashIndex = directory.LastIndexOf('/');
		int backwardSlashIndex = directory.LastIndexOf('\\');
		string croppedDirectoryName;
		if (backwardSlashIndex > forwardSlashIndex) {
			croppedDirectoryName = directory.Substring(backwardSlashIndex + 1);
		} else {
			croppedDirectoryName = directory.Substring(forwardSlashIndex + 1);
		}
		newFolder.GetComponentInChildren<TextMesh>().text = croppedDirectoryName;
		//offset = offset + 1f;
		//i++;
	}
}

}
