using System.Collections.Generic;

using UnityEngine;
public class GraphManager : MonoBehaviour
{

	public Graph graphs;
	public Cell cell;
	private Dictionary<string, Cell> cells;


	void Start ()
	{
		cells = new Dictionary<string, Cell>();
	}

	public void addCell(string label, float x, float y, float z) {
		if(!cells.ContainsKey(label)) {
			cells[label] = Instantiate(cell);
		}
		graphs.addGraphPoint (cells [label], x, y, z);
	}

	public void setMinMaxCoords(Vector3 min, Vector3 max){
		graphs.setMinMaxCoords (min, max);
	}


}
