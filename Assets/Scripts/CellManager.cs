using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellManager : MonoBehaviour {

	private Dictionary<string, Cell> cells;


	void Awake(){
		cells = new Dictionary<string, Cell>();
	}

	public Cell addCell(string label) {
		if(!cells.ContainsKey(label)) {
			cells[label] = new Cell(label);
		}
		cells [label];
	}

}
