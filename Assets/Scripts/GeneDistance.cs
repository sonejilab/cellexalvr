using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GeneDistance : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public SQLiter.SQLite database;
    public GameObject prefab;

    private Graph graph;


    private void Start()
    {
        CellexalEvents.GraphsLoaded.AddListener(GetDistance);
    }

    public void GetDistance()
    {
        graph = referenceManager.graphManager.FindGraph("DDRtree");
        string geneName = "gata1";

        database.QueryGene(geneName, CollectResult);
    }

    public void CollectResult(SQLiter.SQLite database)
    {
        List<Tuple<string, float>> result = new List<Tuple<string, float>>();
        foreach (var entry in database._result)
        {
            result.Add((Tuple<string, float>)entry);
        }

        result.Sort((Tuple<string, float> x, Tuple<string, float> y) => (x.Item2.CompareTo(y.Item2)));
        if (result.Count < 2)
            return;

        List<float> distances = new List<float>(result.Count);
        var point1 = graph.points[result[0].Item1];
        for (int i = 1; i < result.Count; ++i)
        {
            var point2 = graph.points[result[i].Item1];
            distances.Add(Vector3.Distance(point1.transform.position, point2.transform.position));
        }
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[6 * (distances.Count + 1) - 2];
        int[] triangles = new int[24 * (distances.Count + 1) - 18];

        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(0, 0, 1);
        vertices[2] = new Vector3(0, distances[0], 0);
        vertices[3] = new Vector3(0, distances[0], 1);

        float desiredTotalWidth = 10f;
        float individualWidth = desiredTotalWidth / distances.Count;

        for (int i = 4, distindex = 0; i < vertices.Length - 6; i += 6, ++distindex)
        {
            float x = individualWidth * distindex;
            vertices[i] = new Vector3(x, 0, 0);
            vertices[i + 1] = new Vector3(x, 0, 1);
            vertices[i + 2] = new Vector3(x, distances[distindex], 0);
            vertices[i + 3] = new Vector3(x, distances[distindex], 1);
            vertices[i + 4] = new Vector3(x, distances[distindex + 1], 0);
            vertices[i + 5] = new Vector3(x, distances[distindex + 1], 1);
        }

        vertices[vertices.Length - 4] = new Vector3(0, 0, 0);
        vertices[vertices.Length - 3] = new Vector3(0, 0, 1);
        vertices[vertices.Length - 2] = new Vector3(0, distances[distances.Count - 1], 0);
        vertices[vertices.Length - 1] = new Vector3(0, distances[distances.Count - 1], 1);

        //todo: add first two triangles

        for (int i = 6, vertexCount = 4; i < triangles.Length - 30; i += 24, vertexCount += 6)
        {
            // left side
            triangles[i] = vertexCount + 2;
            triangles[i + 1] = vertexCount + 3;
            triangles[i + 2] = vertexCount + 4;

            triangles[i + 3] = vertexCount + 3;
            triangles[i + 4] = vertexCount + 5;
            triangles[i + 5] = vertexCount + 4;

            // top side
            triangles[i + 6] = vertexCount + 4;
            triangles[i + 7] = vertexCount + 5;
            triangles[i + 8] = vertexCount + 8;

            triangles[i + 9] = vertexCount + 5;
            triangles[i + 10] = vertexCount + 9;
            triangles[i + 11] = vertexCount + 8;

            // front side
            triangles[i + 12] = vertexCount;
            triangles[i + 13] = vertexCount + 4;
            triangles[i + 14] = vertexCount + 8;

            triangles[i + 15] = vertexCount;
            triangles[i + 16] = vertexCount + 8;
            triangles[i + 17] = vertexCount + 6;

            // back side
            triangles[i + 18] = vertexCount + 1;
            triangles[i + 19] = vertexCount + 5;
            triangles[i + 20] = vertexCount + 9;

            triangles[i + 21] = vertexCount + 1;
            triangles[i + 22] = vertexCount + 9;
            triangles[i + 23] = vertexCount + 7;

        }

        // last side
        triangles[triangles.Length - 6] = vertices.Length - 4;
        triangles[triangles.Length - 5] = vertices.Length - 3;
        triangles[triangles.Length - 4] = vertices.Length - 2;

        triangles[triangles.Length - 3] = vertices.Length - 3;
        triangles[triangles.Length - 2] = vertices.Length - 2;
        triangles[triangles.Length - 1] = vertices.Length - 1;

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        var data = Instantiate(prefab);
        data.GetComponent<MeshFilter>().mesh = mesh;

    }


}

