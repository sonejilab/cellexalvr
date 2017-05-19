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


	void Awake ()
	{
		//cells = new Dictionary<string, Cell>();
		graphs = new Graph[2];
	}

	public void setActiveGraph(int i) {
		activeGraph = i;
	}

	public void moveGraphs() {
		// these values are hard coded for your convenience
		graphs [0].transform.position = new Vector3 (-1f, 0.625f, -0.413f);
		graphs [1].transform.position = new Vector3 (0f, 0.6f, 0.33f);
	}

	public void CreateGraph(int i) {
		graphs[i] = Instantiate (graphPrefab);
		graphs[i].gameObject.SetActive (true);
		graphs[i].transform.parent = this.transform;
		graphClones = new List<Graph> ();
	}


	public void addCell(string label, float x, float y, float z) {
		graphs[activeGraph].addGraphPoint (cellManager.addCell(label), x, y, z);
	}

	public void setMinMaxCoords(Vector3 min, Vector3 max){
		graphs[activeGraph].setMinMaxCoords (min, max);
	}

	public void colorAllGraphsByGene(string geneName){
		foreach (Graph g in graphs) {
			if (cellManager.geneExists (geneName)) {
				g.colorGraphByGene (geneName);
				goodSound.Play ();
			} else {
				badSound.Play ();
			}
		}
	}

	public void resetGraph(){
		foreach (Graph g in graphs) {
			g.transform.localScale = graphPrefab.transform.localScale;
			g.reset ();
		}
		removeClones ();
	}

	private void removeClones(){
		foreach (Graph graph in graphClones) {
			Destroy (graph.gameObject);
		}
		graphClones.Clear ();
	}

	public Graph newGraphClone(){
		Graph newGraph = Instantiate (graphPrefab);
		newGraph.gameObject.SetActive (true);
		newGraph.transform.parent = this.transform;
		graphClones.Add (newGraph);
		return newGraph;
	}

	public void destroyRigidbodies(){
		Destroy (graphs[activeGraph].GetComponent<Rigidbody> ());
		foreach (Graph clone in graphClones) {
			Destroy (clone.GetComponent<Rigidbody> ());
		}
	}

	public void createRigidbodies(){
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


}
