using UnityEngine;

﻿using System.Collections;

public class LoaderController : MonoBehaviour
{

public InputReader inputReader;
public InputFolderGenerator inputFolderGenerator;
public GraphManager GraphManager;
private bool isFading = false;
private float fadingTime = 0.5f;
private float t;
private bool cellsEntered = false;
// private bool graphsMoved= false;
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
		}

	}

	if (timeEntered + 2 < Time.time && cellsEntered && !collidersDestroyed) {
		collidersDestroyed = true;
		// gameObject.AddComponent<Rigidbody>();
		//MoveLoader(new Vector3(0f, -1f, 0f), 8);

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
			}
		}

	}
	/*if (timeEntered + 5 < Time.time && cellsEntered && !graphsMoved) {
	    GraphManager.MoveGraphs(new Vector3(0, 2.7f, 0), 8f);
	    graphsMoved = true;
	   }*/

}

public void MoveLoader(Vector3 direction, float time) {
	moving = true;
	currentTime = 0;
	arrivalTime = time;
	startPosition = gameObject.transform.position;
	finalPosition = gameObject.transform.position + direction;
}

void OnTriggerEnter(Collider collider) {
	//print(collider.gameObject.tag);
	if (collider.gameObject.tag == "Sphere") {
		Transform cellParent = collider.transform.parent;
		if (cellParent != null) {

			if (timeEntered == 0) {
				timeEntered = Time.time;
				cellsEntered = true;
			}

			if (!cellParent.GetComponent<CellsToLoad>().GraphsLoaded()) {
				inputReader.ReadFolder(cellParent.GetComponent<CellsToLoad>().GetDirectory());
				//StartCoroutine(FadeTo(0.0f, 1.0f));
			}

			foreach (Transform child in cellParent) {
				child.parent = null;
				child.gameObject.AddComponent<Rigidbody>();
				cellsToDestroy.Add(child);
			}
		}

	}
}

// IEnumerator FadeTo(float aValue, float aTime) {
//  float alpha = transform.GetComponent<Renderer>().material.color.a;
//  for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime) {
//      Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha,aValue,t));
//      transform.GetComponent<Renderer>().material.color = newColor;
//      yield return null;
//  }
// }

/* void FadeOut() {
    foreach (Renderer rend in childrenRenderers) {
        rend.material.Lerp(originalMaterial, transparentMaterial, t);
    }
    t = t + fadingTime * Time.deltaTime;
    if (t >= 1) {
        isFading = false;
        //Destroy (this.gameObject);
        t = 0;
    }
    print(t);
   }*/

}
