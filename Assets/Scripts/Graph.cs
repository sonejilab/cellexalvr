using System.Collections;
using System;
using UnityEngine;
public class Graph : MonoBehaviour
	{
	public GraphPoint graphpoint;

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
		// Grabs the location and size of the graphArea.
		minAreaValues = this.GetComponent<Renderer>().bounds.min;
		maxAreaValues = this.GetComponent<Renderer> ().bounds.max;
		areaSize = this.GetComponent<Renderer> ().bounds.size;

	}

	public void addGraphPoint(Cell cell, float x, float y, float z) {
		newGraphpoint = Instantiate(graphpoint);

		// Scales the sphere coordinates to fit inside the this.
		Vector3 scaledCoordinates = new Vector3 (x, y, z);
		scaledCoordinates -= minCoordValues;
		scaledCoordinates.x /= (diffCoordValues.x);
		scaledCoordinates.y /= (diffCoordValues.y);
		scaledCoordinates.z /= (diffCoordValues.z);
		scaledCoordinates.Scale (areaSize);
		scaledCoordinates += minAreaValues;

		newGraphpoint.setCoordinates (cell, scaledCoordinates.x, scaledCoordinates.y, scaledCoordinates.z, areaSize);

		/**
		 * TODO: Do something like the commented line below to add
		 * the spheres as children to the this so that they
		 * move along with it
		 **/
		if (points.Count % 3 == 0) {
			newGraphpoint.getSphere ().GetComponent<Renderer> ().material.color = Color.red;
		}
		if (points.Count % 3 == 1) {
			newGraphpoint.getSphere ().GetComponent<Renderer> ().material.color = Color.blue;
		}

		newGraphpoint.transform.SetParent (this.transform);

		points.Add (newGraphpoint);
	}

	public void setMinMaxCoords(Vector3 min, Vector3 max){
		minCoordValues = min;
		maxCoordValues = max;
		diffCoordValues = maxCoordValues - minCoordValues;

	}

	public ArrayList getGroups(){
		ArrayList colors = new ArrayList ();
		ArrayList groups = new ArrayList ();
		for (int i = 0; i < points.Count; i++) {
			GraphPoint p = (GraphPoint) points [i];
			Color c = p.getSphere().GetComponent<Renderer> ().material.color;

			if (!colors.Contains (c)) {
				colors.Add (c);
				groups.Add (new ArrayList ());
			}
			int groupIndex = colors.IndexOf (c);
			((ArrayList)groups [groupIndex]).Add (p);
		}
		// Debug for Testing
		Debug.Log("Nbr of colors: " + colors.Count);
		Debug.Log("Color #0: " + colors[0].ToString());
		Debug.Log("Nbr of points in group #0: " + ((ArrayList)groups[0]).Count);

		return groups;
	}
}
