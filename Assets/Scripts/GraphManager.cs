using System.Collections.Generic;

using UnityEngine;
public class GraphManager : MonoBehaviour
{

public Graph graphPrefab;
public AudioSource goodSound;
public AudioSource badSound;
private Graph[] graphs;
private int activeGraph = 0;
private List<Graph> graphClones;
public CellManager cellManager;
private Vector3[] startPosition;
private Vector3[] finalPosition;
private bool moving = false;
private float currentTime;
private float arrivalTime;
//private float movingTime;

void Awake () {
	//cells = new Dictionary<string, Cell>();
	graphs = new Graph[2];
	startPosition = new Vector3[2];
	finalPosition = new Vector3[2];

}

void Update() {
	if (moving) {
		for (int i = 0; i < graphs.Length; i++) {
			graphs[i].transform.position = Vector3.Lerp(startPosition[i], finalPosition[i], currentTime / arrivalTime);

		}
		currentTime += Time.deltaTime;
		if (currentTime > arrivalTime) {
			moving = false;
		}

	}
}

public void SetActiveGraph(int i) {
	activeGraph = i;
}

public void SetGraphStartPosition() {
	// these values are hard coded for your convenience
	graphs [0].transform.position = new Vector3 (0f, -3f, -0.913f);
	graphs [1].transform.position = new Vector3 (-.6f, -3f, 0.33f);
}

public void MoveGraphs(Vector3 direction, float time) {
	moving = true;
	currentTime = Time.time;
	arrivalTime = currentTime + time;
	for (int i = 0; i < graphs.Length; i++) {
		startPosition[i] = graphs[i].transform.position;
		finalPosition[i] = graphs[i].transform.position + direction;
	}
}

public void CreateGraph(int i) {
	graphs[i] = Instantiate (graphPrefab);
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
