using UnityEngine;
﻿using System.Collections;

public class CellsInBoxController : MonoBehaviour
{

public InputReader inputReader;
public InputFolderGenerator inputFolderGenerator;
public GraphManager GraphManager;
private bool isFading = false;
private float fadingTime = 0.5f;
private Renderer[] childrenRenderers;
public Material originalMaterial;
public Material transparentMaterial;
private float t;
private bool cellsEntered = false;
private float timeEntered = 0;
private ArrayList cellsToDestroy;
private bool collidersDestroyed = false;

void Start() {
	childrenRenderers = GetComponentsInChildren<Renderer>();
	cellsToDestroy = new ArrayList();
}

void Update() {
	if (timeEntered + 2 < Time.time && cellsEntered && !collidersDestroyed) {
		collidersDestroyed = true;
		gameObject.AddComponent<Rigidbody>();
		foreach (Collider c in GetComponentsInChildren<Collider>()) {
			Destroy(c);
		}

		foreach (Transform child in cellsToDestroy) {
			Destroy(child.gameObject.GetComponent<Collider>());
		}

		foreach (Collider c in inputFolderGenerator.GetComponentsInChildren<Collider>()) {
			c.gameObject.AddComponent<Rigidbody>();
			Destroy(c);
		}
		GraphManager.MoveGraphs(new Vector3(0, 4, 0), 5f);
	}
}

void OnTriggerEnter(Collider collider) {
	//print(collider.gameObject.tag);
	if (collider.gameObject.tag == "Sphere") {
		Transform parentTransform = collider.transform.parent;
		if (parentTransform != null) {

			if (timeEntered == 0) {
				timeEntered = Time.time;
				cellsEntered = true;
			}

			if (!parentTransform.GetComponent<CellsInBox>().GraphsLoaded()) {
				inputReader.ReadFolder(parentTransform.GetComponent<CellsInBox>().GetDirectory());
				//StartCoroutine(FadeTo(0.0f, 1.0f));

			}

			foreach (Transform child in parentTransform) {
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
