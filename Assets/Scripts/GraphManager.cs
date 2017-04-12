using System.Collections.Generic;

using UnityEngine;
public class GraphManager : MonoBehaviour
{

	public Graph graphs;
	public Cell cell;
	public CellManager cellManager;


	void Start ()
	{
		//cells = new Dictionary<string, Cell>();
	}


	public void addCell(string label, float x, float y, float z) {
		Cell newCell = cellManager.addCell (label);
		graphs.addGraphPoint (newCell, x, y, z);
	}

	public void setMinMaxCoords(Vector3 min, Vector3 max){
		graphs.setMinMaxCoords (min, max);
	}


}
