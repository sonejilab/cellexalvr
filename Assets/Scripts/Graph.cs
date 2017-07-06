using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    public GraphPoint graphpoint;
    public SelectionToolHandler selectionToolHandler;
    public GameObject skeletonPrefab;
    private GraphPoint newGraphpoint;
    private List<GraphPoint> points;
    private Vector3 maxCoordValues;
    private Vector3 minCoordValues;
    private Vector3 diffCoordValues;
    private Vector3 minAreaValues;
    private Vector3 areaSize;
    private Vector3 defaultPos;
    private Vector3 defaultScale;
    void Start()
    {
        defaultPos = transform.position;
    }

    //Called before any Start()-function. Avoids nullReferenceException in addGraphPoint().
    void Awake()
    {
        points = new List<GraphPoint>();
        minAreaValues = gameObject.transform.position;
        areaSize = new Vector3(1,1,1);
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
        newGraphpoint.gameObject.SetActive(true);
        newGraphpoint.SetCoordinates(cell, scaledCoordinates.x, scaledCoordinates.y, scaledCoordinates.z, areaSize);
        newGraphpoint.transform.SetParent(this.transform);
        newGraphpoint.SaveParent(this);
        points.Add(newGraphpoint);

        defaultScale = transform.localScale;
    }

    public void SetMinMaxCoords(Vector3 min, Vector3 max)
    {
        minCoordValues = min;
        maxCoordValues = max;
        diffCoordValues = maxCoordValues - minCoordValues;

    }

    //public void ColorGraphByGene(string geneName) {
    //	foreach (GraphPoint point in points) {
    //		point.ColorByGene (geneName);
    //	}
    //}

    public List<List<GraphPoint>> GetGroups()
    {

        List<Color> colors = new List<Color>();
        List<List<GraphPoint>> groups = new List<List<GraphPoint>>();

        for (int i = 0; i < points.Count; i++)
        {
            GraphPoint p = (GraphPoint)points[i];
            Color m = p.GetMaterial().color;

            if (!colors.Contains(m))
            {
                colors.Add(m);
                groups.Add(new List<GraphPoint>());
            }

            int groupIndex = colors.IndexOf(m);
            (groups[groupIndex]).Add(p);
        }

        return groups;

    }

    public void CreateConvexHull()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        GameObject skeleton = Instantiate(skeletonPrefab);
        int scaleFactor = 10;
        int i = 0;
        //print(meshFilters[0].name);
        while (i < meshFilters.Length)
        {
            MeshFilter clone = Instantiate(meshFilters[i]);
            clone.transform.localScale = clone.transform.localScale * scaleFactor;
            // clone.transform.parent = skeleton.transform;
            //MeshFilter clone = Instantiate(meshFilters[i]);
            //combine[i].transform.localScale = combine[i].transform.localScale * scaleFactor;
            //var scaledMesh = meshFilters[i].mesh;
            //var vertices = scaledMesh.vertices;
            //for (int j = 0; j < vertices.Length; ++j)
            //{
            //    var vertex = vertices[j];
            //    var dx = vertex.x * scaleFactor;
            //    var dy = vertex.y * scaleFactor;
            //    var dz = vertex.z * scaleFactor;
            //    vertices[i] = vertex;
            //}
            //scaledMesh.vertices = vertices;
            //meshFilters[i].mesh = scaledMesh;
            combine[i].mesh = clone.sharedMesh;
            combine[i].transform = clone.transform.localToWorldMatrix;
            Destroy(clone);
            i++;
        }
        var skeletonMeshFilter = skeleton.GetComponent<MeshFilter>();
        skeletonMeshFilter.mesh = new Mesh();
        skeletonMeshFilter.mesh.CombineMeshes(combine, true);
        skeleton.GetComponent<MeshCollider>().sharedMesh = skeletonMeshFilter.sharedMesh;
        
    }

    public void ResetGraph()
    {
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

    // Makes scaling of sub-graphs work better
    public void LimitGraphArea(ArrayList points)
    {
        maxCoordValues.x = maxCoordValues.y = maxCoordValues.z = -1000000.0f;
        minCoordValues.x = minCoordValues.y = minCoordValues.z = 1000000.0f;
        foreach (Collider col in points)
        {
            if (col.gameObject.activeSelf)
            {
                Vector3 coordinates = col.transform.position;
                if (coordinates.x > maxCoordValues.x)
                {
                    maxCoordValues.x = coordinates.x;
                }
                if (coordinates.x < minCoordValues.x)
                {
                    minCoordValues.x = coordinates.x;
                }
                if (coordinates.y > maxCoordValues.y)
                {
                    maxCoordValues.y = coordinates.y;
                }
                if (coordinates.y < minCoordValues.y)
                {
                    minCoordValues.y = coordinates.y;
                }
                if (coordinates.z > maxCoordValues.z)
                {
                    maxCoordValues.z = coordinates.z;
                }
                if (coordinates.z < minCoordValues.z)
                {
                    minCoordValues.z = coordinates.z;
                }
            }
        }
        Vector3 newCenter = Vector3.Lerp(minCoordValues, maxCoordValues, (minCoordValues - maxCoordValues).magnitude / 2);
        transform.position = newCenter;
    }

}
