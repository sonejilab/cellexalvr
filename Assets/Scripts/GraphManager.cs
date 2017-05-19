using System.Collections.Generic;

using UnityEngine;
public class GraphManager : MonoBehaviour
{

	public Graph graphPrefab;
	public AudioSource goodSound;
	public AudioSource badSound;
	private Graph graphs;
	private List<Graph> graphClones;
	public CellManager cellManager;


	void Awake ()
	{
		//cells = new Dictionary<string, Cell>();
		graphs = Instantiate(graphPrefab);
		graphs.gameObject.SetActive (true);
		graphs.transform.parent = this.transform;
		graphClones = new List<Graph> ();
	}


	public void addCell(string label, float x, float y, float z) {
		graphs.addGraphPoint (cellManager.addCell(label), x, y, z);
	}

	public void setMinMaxCoords(Vector3 min, Vector3 max){
		graphs.setMinMaxCoords (min, max);
	}

	public void colorAllGraphsByGene(string geneName){
		if (cellManager.geneExists(geneName)) {
			graphs.colorGraphByGene(geneName);
			goodSound.Play ();
		} else {
			badSound.Play ();
		}
	}

	public void resetGraph(){
		graphs.transform.position = graphPrefab.transform.position;
		graphs.transform.localScale = graphPrefab.transform.localScale;
		graphs.transform.rotation = graphPrefab.transform.rotation;
		graphs.reset ();
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
		Destroy (graphs.GetComponent<Rigidbody> ());
		foreach (Graph clone in graphClones) {
			Destroy (clone.GetComponent<Rigidbody> ());
		}
	}

	public void createRigidbodies(){
		graphs.gameObject.AddComponent<Rigidbody> ();
		graphs.gameObject.GetComponent<Rigidbody> ().isKinematic = true;
		graphs.gameObject.GetComponent<Rigidbody> ().useGravity = false;
		graphs.gameObject.GetComponent<Rigidbody> ().angularDrag = Mathf.Infinity;
		foreach (Graph clone in graphClones) {
			clone.gameObject.AddComponent<Rigidbody> ();
			clone.gameObject.GetComponent<Rigidbody> ().isKinematic = true;
			clone.gameObject.GetComponent<Rigidbody> ().useGravity = false;
			clone.gameObject.GetComponent<Rigidbody> ().angularDrag = Mathf.Infinity;
		}
	}


}
