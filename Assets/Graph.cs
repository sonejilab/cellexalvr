using System.Collections;
using System;
using UnityEngine;
public class Graph : MonoBehaviour
	{
	public GraphPoint graphpoint;

	private GraphPoint newGraphpoint;
	private  ArrayList points;

	void Start ()
	{
		points = new ArrayList();
	}

	public void addGraphPoint(Cell cell, float x, float y, float z) {
		newGraphpoint = Instantiate(graphpoint);
		newGraphpoint.setCoordinates (cell, x, y, z);
		points.Add (newGraphpoint);

	}
}
