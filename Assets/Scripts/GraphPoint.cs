using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphPoint : MonoBehaviour
{
	public GameObject prefab;
	private Cell cell;
	private float x, y, z;
	private GameObject sphere;
	private bool selected = false;

	public void setCoordinates (Cell cell, float x, float y, float z, Vector3 graphAreaSize)
	{
		this.cell = cell;
		this.x = x;
		this.y = y;
		this.z = z;
		sphere = Instantiate (prefab, new Vector3 (x, y, z), Quaternion.identity);
		sphere.transform.SetParent (this.transform);
		Color color = Color.white;
	}
		
	public GameObject getSphere(){
		return sphere;
	}

	public bool isSelected() {
		return selected;
	}

	public void setSelected(bool isSelected){
		selected=isSelected;
	}

	public void setMaterial(Material material){
		sphere.GetComponent<Renderer> ().material=material;
	}

	public Material getMaterial(){
		return sphere.GetComponent<Renderer> ().material;
	}

	public void colorByGene(string geneName){
		setMaterial (cell.getGeneMaterial (geneName));
	}
}

