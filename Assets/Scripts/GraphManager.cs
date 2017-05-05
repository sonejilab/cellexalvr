using System.Collections.Generic;

using UnityEngine;
public class GraphManager : MonoBehaviour
{

	public Graph graphPrefab;
	private Graph graphs;
	public CellManager cellManager;


	void Start ()
	{
		//cells = new Dictionary<string, Cell>();
		graphs = Instantiate(graphPrefab);
		graphs.gameObject.SetActive (true);
		graphs.transform.parent = this.transform;

	}


	public void addCell(string label, float x, float y, float z) {
		graphs.addGraphPoint (cellManager.addCell(label), x, y, z);
	}

	public void setMinMaxCoords(Vector3 min, Vector3 max){
		graphs.setMinMaxCoords (min, max);
	}


}
