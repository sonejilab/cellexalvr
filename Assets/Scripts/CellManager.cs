using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellManager : MonoBehaviour {

	private Dictionary<string, Cell> cells;
	public Cell cell;


	void Awake(){
		cells = new Dictionary<string, Cell>();
	}

	public Cell addCell(string label) {
		if(!cells.ContainsKey(label)) {
			cells[label] = Instantiate(cell);
			cells [label].transform.SetParent (this.transform);
			cells [label].setLabel (label);
		}
		return cells [label];
	}

}
