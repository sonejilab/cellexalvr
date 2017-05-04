using System.Collections.Generic;

using UnityEngine;
public class GraphManager : MonoBehaviour
{

	public Graph graphs;
	public CellManager cellManager;


	void Start ()
	{
		//cells = new Dictionary<string, Cell>();
	}


	public void addCell(string label, float x, float y, float z) {
		graphs.addGraphPoint (cellManager.addCell(label), x, y, z);
	}

	public void setMinMaxCoords(Vector3 min, Vector3 max){
		graphs.setMinMaxCoords (min, max);
	}

	public void colorAllGraphsByGene(string geneName){
		graphs.colorGraphByGene(geneName);
	}


}
