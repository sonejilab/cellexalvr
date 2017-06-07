using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellManager : MonoBehaviour {

	private Dictionary<string, Cell> cells;
	public Cell cell;
	private ArrayList geneNames;
	public List<Material> materialList;


	void Awake(){
		cells = new Dictionary<string, Cell>();
		geneNames = new ArrayList();
	}

	public Cell addCell(string label) {
		if(!cells.ContainsKey(label)) {
			cells [label] = new Cell (label, materialList);
		}
		return cells [label];
	}

	public Cell getCell(string label) {
		return cells [label];
	}

	public bool geneExists(string geneName) {
		return geneNames.Contains (geneName);
	}

	public void setGeneExpression(string cellName, string geneName, int slot){
		Cell cell;
		cells.TryGetValue (cellName, out cell);
		cell.setExpressionData (geneName, slot);
		if (!geneNames.Contains (geneName)) {
			geneNames.Add (geneName);
		}
	}

}
