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
	private Color selectedColor;
	private Graph defaultParent;

	public void SetCoordinates (Cell cell, float x, float y, float z, Vector3 graphAreaSize)
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

	public string GetLabel() {
		return cell.Label;
	}
		
	 public GameObject GetSphere(){
		return sphere;
	}

	public bool IsSelected() {
		return selected;
	}

	public void SetSelected(bool isSelected){
		selected=isSelected;
	}

	public void SetMaterial(Material material){
		sphere.GetComponent<Renderer> ().material = material;
	}

	public Material GetMaterial(){
		return sphere.GetComponent<Renderer> ().material;
	}

	public Material GetDefaultMaterial(){
		return defaultMat;
	}

	public void SetSelectedColor(Color col){
		selectedColor = col;
	}

    public Color GetSelectedColor()
    {
        return selectedColor;
    }

    public void ColorByGene(string geneName){
		if (!selected) {
			SetMaterial (cell.getGeneMaterial (geneName));
		}

		defaultMat = cell.getGeneMaterial (geneName);
	}

	public void ResetCoords(){
		transform.position = new Vector3 (x, y, z);
		transform.localScale = new Vector3 (2.5f, 2.5f, 2.5f); //hard-coded to current sphere size
		transform.SetParent (defaultParent.transform);
		//sphere.transform.position = new Vector3 (x, y, z);
		Rigidbody rig = GetComponent<Rigidbody> ();
		if (rig != null) {
			Destroy (rig);
		}
		selected = false;
		defaultMat = Resources.Load ("SphereDefault", typeof(Material)) as Material;
		SetMaterial (defaultMat);
	}

	public Vector3 GetCoordinates(){
		return new Vector3 (x, y, z);
	}

	public Cell GetCell(){
		return cell;
	}

	public void SaveParent(Graph parent) {
		defaultParent = parent;
	}
}
