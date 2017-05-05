using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphPoint : MonoBehaviour
{
	public GameObject prefab;
	private Cell cell;
	private float x, y, z;
	//private GameObject sphere;

	public void setCoordinates (Cell cell, float x, float y, float z, Vector3 graphAreaSize)
	{
		this.cell = cell;
		this.x = x;
		this.y = y;
		this.z = z;
		// sphere = Instantiate (prefab, new Vector3 (x, y, z), Quaternion.identity);
		// sphere.transform.SetParent (this.transform);
		Color color = Color.white;
		Vector3 localPos = transform.localPosition;


	}

	public string getLabel() {
		return cell.Label;
	}
		
	/* public GameObject getSphere(){
		return sphere;
	}
	*/
}

