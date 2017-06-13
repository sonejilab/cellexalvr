using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;

public class Graph : MonoBehaviour
	{
	public GraphPoint graphpoint;
    public SelectionToolHandler selectionToolHandler;

	private GraphPoint newGraphpoint;
	private List<GraphPoint> points;
	private Vector3 maxCoordValues;
	private Vector3 minCoordValues;
	private Vector3 diffCoordValues;
	private Vector3 minAreaValues;
	// private Vector3 maxAreaValues;
	private Vector3 areaSize;
	private Vector3 defaultPos;
	private Vector3 defaultScale;
	void Start ()
	{
		//points = new ArrayList();
	}

	//Called before any Start()-function. Avoids nullReferenceException in addGraphPoint().
	void Awake() {
		points = new List<GraphPoint>();
		// Grabs the location and size of the graphArea.
		minAreaValues = this.GetComponent<Renderer>().bounds.min;
		// maxAreaValues = this.GetComponent<Renderer> ().bounds.max;
		areaSize = this.GetComponent<Renderer> ().bounds.size;
        graphpoint.gameObject.SetActive(false);

	}

    public void AddGraphPoint(Cell cell, float x, float y, float z)
    {

        // Scales the sphere coordinates to fit inside the this.
        Vector3 scaledCoordinates = new Vector3(x, y, z);
        scaledCoordinates -= minCoordValues;
        scaledCoordinates.x /= (diffCoordValues.x);
        scaledCoordinates.y /= (diffCoordValues.y);
        scaledCoordinates.z /= (diffCoordValues.z);
        scaledCoordinates.Scale(areaSize);
        scaledCoordinates += minAreaValues;

        newGraphpoint = Instantiate(graphpoint, new Vector3(scaledCoordinates.x, scaledCoordinates.y, scaledCoordinates.z), Quaternion.identity);
		newGraphpoint.gameObject.SetActive (true);
		newGraphpoint.SetCoordinates (cell, scaledCoordinates.x, scaledCoordinates.y, scaledCoordinates.z, areaSize);

        newGraphpoint.transform.SetParent(this.transform);
		newGraphpoint.SaveParent (this);

        points.Add(newGraphpoint);

		defaultPos = transform.position;
		defaultScale = transform.localScale;
    }

    public void SetMinMaxCoords(Vector3 min, Vector3 max)
    {
        minCoordValues = min;
        maxCoordValues = max;
        diffCoordValues = maxCoordValues - minCoordValues;

    }
		

	public void ColorGraphByGene(string geneName){
		foreach (GraphPoint point in points) {
			point.ColorByGene (geneName);
		}
	}

	public List<List<GraphPoint>> GetGroups(){
		
		List<Color> colors = new List<Color>();
		List<List<GraphPoint>> groups = new List<List<GraphPoint>> ();

		for (int i = 0; i < points.Count; i++) {
			GraphPoint p = (GraphPoint) points [i];
			Color m = p.GetMaterial ().color;

			if (!colors.Contains (m)) {
				colors.Add (m);
				groups.Add (new List<GraphPoint> ());
			}

			int groupIndex = colors.IndexOf (m);
			(groups [groupIndex]).Add (p);
		}

		// Debug for Testing
//		Debug.Log("Nbr of colors: " + colors.Count);
//		Debug.Log("Color #0: " + colors[0]);
//		Debug.Log("Nbr of points in group #0: " + (groups[0]).Count);
//		Debug.Log("Nbr of points in group #1: " + (groups[1]).Count);

		return groups;

	}

    public void ResetGraph() {
        selectionToolHandler.CancelSelection();
		transform.position = defaultPos;
		transform.localScale = defaultScale;
        foreach (GraphPoint point in points)
        {
            point.ResetCoords();
            if (point.GetComponent<Rigidbody>() != null)
            {
                point.GetComponent<Collider>().isTrigger = true;
                point.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
	}


	/**
	 * Makes scaling of sub-graphs work better
	 **/
	public void LimitGraphArea(ArrayList points){
		maxCoordValues.x = maxCoordValues.y = maxCoordValues.z = -1000000.0f;
		minCoordValues.x = minCoordValues.y = minCoordValues.z = 1000000.0f;
		foreach (Collider col in points) {
			if (col.gameObject.activeSelf) {
				Vector3 coordinates = col.transform.position;
				if (coordinates.x > maxCoordValues.x) {
					maxCoordValues.x = coordinates.x;
				}
				if (coordinates.x < minCoordValues.x) {
					minCoordValues.x = coordinates.x;
				}
				if (coordinates.y > maxCoordValues.y) {
					maxCoordValues.y = coordinates.y;
				}
				if (coordinates.y < minCoordValues.y) {
					minCoordValues.y = coordinates.y;
				}
				if (coordinates.z > maxCoordValues.z) {
					maxCoordValues.z = coordinates.z;
				}
				if (coordinates.z < minCoordValues.z) {
					minCoordValues.z= coordinates.z;
				}
			}
		}
		Vector3 newCenter = Vector3.Lerp (minCoordValues, maxCoordValues, (minCoordValues - maxCoordValues).magnitude / 2);
		transform.position = newCenter;
	}
		
}
