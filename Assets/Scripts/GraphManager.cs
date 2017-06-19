using System.Collections.Generic;
using UnityEngine;

public class GraphManager : MonoBehaviour {

public CellManager cellManager;
public Graph graphPrefab;
public AudioSource goodSound;
public AudioSource badSound;
private Graph[] graphs;
private int activeGraph = 0;
private List<Graph> graphClones;

void Awake () {
	graphs = new Graph[2];
}

public void SetActiveGraph(int i) {
	activeGraph = i;
}

public void SetGraphStartPosition() {
	// these values are hard coded for your convenience
	graphs [0].transform.position = new Vector3 (0.686f, .5f, -0.157f);
	graphs [1].transform.position = new Vector3 (-.456f, .5f, -0.119f);
}

public void CreateGraph(int i) {
	// more hardcoded values
	if (i == 0) {
		graphs[0] = Instantiate (graphPrefab, new Vector3(0.686f, .5f, -0.157f), Quaternion.identity);
	} else if (i == 1) {
		graphs[1] = Instantiate (graphPrefab, new Vector3 (-.456f, .5f, -0.119f), Quaternion.identity);
	}
	graphs[i].gameObject.SetActive (true);
	graphs[i].transform.parent = this.transform;
	graphClones = new List<Graph> ();
}

public void AddCell(string label, float x, float y, float z) {
	graphs[activeGraph].AddGraphPoint (cellManager.AddCell(label), x, y, z);
}

public void SetMinMaxCoords(Vector3 min, Vector3 max) {
	graphs[activeGraph].SetMinMaxCoords (min, max);
}

public void ColorAllGraphsByGene(string geneName) {
	foreach (Graph g in graphs) {
		if (cellManager.GeneExists (geneName)) {
			g.ColorGraphByGene (geneName);
			goodSound.Play ();
		} else {
			badSound.Play ();
		}
	}
}

public void ResetGraph() {
	foreach (Graph g in graphs) {
		g.ResetGraph();
	}
	RemoveClones ();
	SetGraphStartPosition();
}

private void RemoveClones() {
	foreach (Graph graph in graphClones) {
		Destroy (graph.gameObject);
	}
	graphClones.Clear ();
}

public Graph NewGraphClone() {
	Graph newGraph = Instantiate (graphPrefab);
	newGraph.gameObject.SetActive (true);
	newGraph.transform.parent = this.transform;
	graphClones.Add (newGraph);
	return newGraph;
}

public void DestroyRigidbodies() {
	Destroy (graphs[activeGraph].GetComponent<Rigidbody> ());
	foreach (Graph clone in graphClones) {
		Destroy (clone.GetComponent<Rigidbody> ());
	}
}

public void CreateRigidbodies() {
	graphs[activeGraph].gameObject.AddComponent<Rigidbody> ();
	graphs[activeGraph].gameObject.GetComponent<Rigidbody> ().isKinematic = true;
	graphs[activeGraph].gameObject.GetComponent<Rigidbody> ().useGravity = false;
	graphs[activeGraph].gameObject.GetComponent<Rigidbody> ().angularDrag = Mathf.Infinity;
	foreach (Graph clone in graphClones) {
		clone.gameObject.AddComponent<Rigidbody> ();
		clone.gameObject.GetComponent<Rigidbody> ().isKinematic = true;
		clone.gameObject.GetComponent<Rigidbody> ().useGravity = false;
		clone.gameObject.GetComponent<Rigidbody> ().angularDrag = Mathf.Infinity;
	}
}

public void HideDDRGraph() {
	graphs[0].gameObject.SetActive(!graphs[0].gameObject.activeSelf);
}

public void HideTSNEGraph() {
	graphs[1].gameObject.SetActive(!graphs[1].gameObject.activeSelf);;
}

}
