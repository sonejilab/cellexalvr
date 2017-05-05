using System.Collections;
using System;
using UnityEngine;
using System.IO;

public class Graph : MonoBehaviour
{
    public GraphPoint graphpoint;

    private GraphPoint newGraphpoint;
    private ArrayList points;
    private Vector3 maxCoordValues;
    private Vector3 minCoordValues;
    private Vector3 diffCoordValues;
    private Vector3 minAreaValues;
    private Vector3 maxAreaValues;
    private Vector3 areaSize;

    void Start()
    {
        //points = new ArrayList();
    }

    //Called before any Start()-function. Avoids nullReferenceException in addGraphPoint().
    void Awake()
    {
        points = new ArrayList();
        // Grabs the location and size of the graphArea.
        minAreaValues = this.GetComponent<Renderer>().bounds.min;
        maxAreaValues = this.GetComponent<Renderer>().bounds.max;
        areaSize = this.GetComponent<Renderer>().bounds.size;

    }

    public void addGraphPoint(Cell cell, float x, float y, float z)
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
		newGraphpoint.setCoordinates (cell, scaledCoordinates.x, scaledCoordinates.y, scaledCoordinates.z, areaSize);

        /**
		 * TODO: Do something like the commented line below to add
		 * the spheres as children to the this so that they
		 * move along with it
		 **/
        newGraphpoint.transform.SetParent(this.transform);

        points.Add(newGraphpoint);
    }

    public void setMinMaxCoords(Vector3 min, Vector3 max)
    {
        minCoordValues = min;
        maxCoordValues = max;
        diffCoordValues = maxCoordValues - minCoordValues;

    }


}
