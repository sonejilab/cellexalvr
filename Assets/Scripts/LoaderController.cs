using UnityEngine;
﻿using System.Collections;

public class LoaderController : MonoBehaviour {

public InputReader inputReader;
public InputFolderGenerator inputFolderGenerator;
public GraphManager GraphManager;
public AudioSource sound;
private bool cellsEntered = false;
private float timeEntered = 0;
private ArrayList cellsToDestroy;
private bool collidersDestroyed = false;
private Vector3 startPosition;
private Vector3 finalPosition;
private bool moving = false;
private float currentTime;
private float arrivalTime;

void Start() {
	cellsToDestroy = new ArrayList();
}

void Update() {
	if (moving) {
		gameObject.transform.position = Vector3.Lerp(startPosition, finalPosition, currentTime / arrivalTime);
		currentTime += Time.deltaTime;
		if (currentTime > arrivalTime) {
			moving = false;
			sound.Stop();
		}
	}
	if (timeEntered + 2 < Time.time && cellsEntered && !collidersDestroyed) {
		collidersDestroyed = true;

		foreach (Collider c in GetComponentsInChildren<Collider>()) {
			Destroy(c);
		}

		foreach (Transform child in cellsToDestroy) {
			Destroy(child.gameObject.GetComponent<Collider>());
		}

		foreach (Collider c in inputFolderGenerator.GetComponentsInChildren<Collider>()) {
			Destroy(c);
		}

		foreach (Transform child in inputFolderGenerator.transform) {
			if (child.tag == "Folder") {
				child.gameObject.AddComponent<Rigidbody>();
				child.gameObject.GetComponent<CellFolder>().PlaySound();
			}
		}

	}

}

public void MoveLoader(Vector3 direction, float time) {
	sound.Play();
	moving = true;
	currentTime = 0;
	arrivalTime = time;
	startPosition = gameObject.transform.position;
	finalPosition = gameObject.transform.position + direction;
}

void OnTriggerEnter(Collider collider) {
	if (collider.gameObject.tag == "Sphere") {
		Transform cellParent = collider.transform.parent;
		if (cellParent != null) {

			if (timeEntered == 0) {
				timeEntered = Time.time;
				cellsEntered = true;
			}

			if (!cellParent.GetComponent<CellsToLoad>().GraphsLoaded()) {
				inputReader.ReadFolder(cellParent.GetComponent<CellsToLoad>().GetDirectory());
			}

			foreach (Transform child in cellParent) {
				child.parent = null;
				child.gameObject.AddComponent<Rigidbody>();
				cellsToDestroy.Add(child);
			}
		}

	}
}

}
