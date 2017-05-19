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
	private Material defaultMat;
	private Graph defaultParent;

	public void setCoordinates (Cell cell, float x, float y, float z, Vector3 graphAreaSize)
	{
		this.cell = cell;
		this.x = x;
		this.y = y;
		this.z = z;
		sphere = Instantiate (prefab, new Vector3 (x, y, z), Quaternion.identity);
		sphere.transform.SetParent (this.transform);
		sphere.gameObject.SetActive (true);
		Color color = Color.white;
		defaultMat = Resources.Load ("SphereDefault", typeof(Material)) as Material;
		Vector3 localPos = transform.localPosition;


	}

	public string getLabel() {
		return cell.Label;
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
		sphere.GetComponent<Renderer> ().material = material;
	}

	public Material getMaterial(){
		return sphere.GetComponent<Renderer> ().material;
	}

	public Material getDefaultMaterial(){
		return defaultMat;
	}

	public void colorByGene(string geneName){
		if (!selected) {
			setMaterial (cell.getGeneMaterial (geneName));
		}

		defaultMat = cell.getGeneMaterial (geneName);
	}

	public void resetCoords(){
		transform.position = new Vector3 (x, y, z);
		transform.SetParent (defaultParent.transform);
		//sphere.transform.position = new Vector3 (x, y, z);
		Rigidbody rig = GetComponent<Rigidbody> ();
		if (rig != null) {
			Destroy (rig);
		}
		setMaterial (defaultMat);
	}

	public Vector3 getCoordinates(){
		return new Vector3 (x, y, z);
	}

	public Cell getCell(){
		return cell;
	}

	public void saveParent(Graph parent) {
		defaultParent = parent;
	}
}
