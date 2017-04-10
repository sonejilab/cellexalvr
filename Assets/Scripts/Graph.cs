using System.Collections;
using System;
using UnityEngine;
public class Graph : MonoBehaviour
	{
	public GraphPoint graphpoint;
	public GameObject graphArea;

	private GraphPoint newGraphpoint;
	private  ArrayList points;
	private Vector3 maxCoordValues;
	private Vector3 minCoordValues;
	private Vector3 diffCoordValues;
	private Vector3 minAreaValues;
	private Vector3 maxAreaValues;
	private Vector3 areaSize;

	void Start ()
	{
		//points = new ArrayList();
	}

	//Called before any Start()-function. Avoids nullReferenceException in addGraphPoint().
	void Awake() {
		points = new ArrayList();
		minAreaValues = graphArea.GetComponent<Renderer> ().bounds.min;
		Debug.Log ("Min area values = " + minAreaValues);
		maxAreaValues = graphArea.GetComponent<Renderer> ().bounds.max;
		Debug.Log ("Max area values = " + maxAreaValues);
		areaSize = graphArea.GetComponent<Renderer> ().bounds.size;

	}

	public void addGraphPoint(Cell cell, float x, float y, float z) {
		newGraphpoint = Instantiate(graphpoint);
		Vector3 scaledCoordinates = new Vector3 (x, y, z);
		Debug.Log ("Start coords = " + scaledCoordinates);
		scaledCoordinates -= minCoordValues;
		scaledCoordinates.x /= (diffCoordValues.x);
		scaledCoordinates.y /= (diffCoordValues.y);
		scaledCoordinates.z /= (diffCoordValues.z);
		scaledCoordinates.Scale (areaSize);
		scaledCoordinates += minAreaValues;
		Debug.Log ("End coords ? " + scaledCoordinates);
		//Vector3 scaledCoordinates = new Vector3();
		newGraphpoint.setCoordinates (cell, scaledCoordinates.x, scaledCoordinates.y, scaledCoordinates.z);
		newGraphpoint.transform.SetParent (graphArea.transform);
		points.Add (newGraphpoint);

	}

	public void setMinMaxCoords(Vector3 min, Vector3 max){
		minCoordValues = min;
		Debug.Log ("Min values = " + minCoordValues);
		maxCoordValues = max;
		Debug.Log ("Max values = " + maxCoordValues);
		diffCoordValues = maxCoordValues - minCoordValues;

	}
}
