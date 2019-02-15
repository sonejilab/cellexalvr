using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CellexalExtensions;
using System.Threading;
using System.IO;

public class GeneDistance : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public SQLiter.SQLite database;
    public GameObject plotGameObjectPrefab;
    public GameObject gradientBoxPrefab;
    public GameObject parentGameObjectPrefab;

    private List<GameObject> plots = new List<GameObject>();
    private CombinedGraph graph;
    private bool smoothing;
    private bool invert;

    private Vector3 startPos = new Vector3(0f, 10f, 0f);
    private Vector3 posInc = new Vector3(0f, 0f, 0.1f);

    private List<List<float>> allDistances = new List<List<float>>();
    private GameObject currentParent;


    private void Start()
    {
        CellexalEvents.GraphsLoaded.AddListener(CreateManyPlots);
    }

    private void Update()
    {

    }

    public void CreateManyPlots()
    {
        //StartCoroutine(CreateManyPlotsCoroutine(null));
        StartCoroutine(CreateManyPlotsCoroutineHeatmap());
    }

    public IEnumerator CreateManyPlotsCoroutineHeatmap()
    {
        while (referenceManager.selectionToolHandler.RObjectUpdating)
            yield return null;

        // Start generation of new heatmap in R

        int selectionNr = referenceManager.selectionToolHandler.fileCreationCtr - 1;
        List<CombinedGraph.CombinedGraphPoint> selection = referenceManager.selectionToolHandler.GetLastSelection();
        string heatmapName = "heatmap3d.txt";
        string rScriptFilePath = (Application.streamingAssetsPath + @"\R\make_heatmap.R").FixFilePath();
        string heatmapDirectory = (CellexalUser.UserSpecificFolder + @"\Heatmap").FixFilePath();
        string outputFilePath = (heatmapDirectory + @"\" + heatmapName).FixFilePath();
        string args = heatmapDirectory + " " + CellexalUser.UserSpecificFolder + " " + selectionNr + " " + outputFilePath + " " + CellexalConfig.HeatmapNumberOfGenes;
        if (!Directory.Exists(heatmapDirectory))
        {
            CellexalLog.Log("Creating directory " + heatmapDirectory.FixFilePath());
            Directory.CreateDirectory(heatmapDirectory);
        }
        CellexalLog.Log("Running R script " + rScriptFilePath.FixFilePath() + " with the arguments \"" + args + "\"");
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        Thread t = new Thread(() => RScriptRunner.RunFromCmd(rScriptFilePath, args));
        t.Start();

        while (t.IsAlive)
        {
            yield return null;
        }

        StreamReader streamReader = new StreamReader(outputFilePath);
        int numberOfGenes = int.Parse(streamReader.ReadLine());
        string[] genes = new string[numberOfGenes];
        int i = 0;
        while (!streamReader.EndOfStream)
        {
            genes[i] = streamReader.ReadLine();
            i++;
        }
        streamReader.Close();

        currentParent = Instantiate(parentGameObjectPrefab);
        foreach (var gene in genes)
        {
            //if (i > 500)
            //    break;
            CreateDistancePlot(gene, selection);
            while (database.QueryRunning)
                yield return null;
        }

        i = 0;
        foreach (GameObject plot in plots)
        {
            plot.transform.localPosition = posInc * i;
            i++;
        }

        currentParent.transform.position = new Vector3(0.5f, 0.5f, 0.5f);
        currentParent.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

    }

    public IEnumerator CreateManyPlotsCoroutine()
    {

        database.QueryMostExpressedGenes(250);

        while (database.QueryRunning)
            yield return null;

        List<string> geneNames = new List<string>(database._result.Count);
        foreach (var s in database._result)
        {
            geneNames.Add((string)s);
        }

        int i = 0;
        currentParent = Instantiate(parentGameObjectPrefab);
        foreach (var gene in geneNames)
        {
            i++;
            //if (i > 500)
            //    break;
            CreateDistancePlot(gene);
            while (database.QueryRunning)
                yield return null;
        }

        int numDistances = allDistances[0].Count;
        Cluster root = new Cluster();
        HashSet<Cluster> clusters = new HashSet<Cluster>();
        Dictionary<Tuple<Cluster, Cluster>, float> clusterDistances = new Dictionary<Tuple<Cluster, Cluster>, float>((geneNames.Count * geneNames.Count) / 2, new NodeTupleComparer());

        for (i = 0; i < allDistances.Count; ++i)
        {
            for (int j = i + 1; j < allDistances.Count; ++j)
            {
                //if (i == j)
                //    continue;

                // create new clusters if they dont exist in the hashset already
                var cluster1 = new Cluster();
                cluster1.index = i;
                if (!clusters.Contains(cluster1))
                {
                    clusters.Add(cluster1);
                }
                else
                {
                    cluster1 = clusters.Single((Cluster x) => (x.index == cluster1.index));
                }
                var cluster2 = new Cluster();
                cluster2.index = j;
                if (!clusters.Contains(cluster2))
                {
                    clusters.Add(cluster2);
                }
                else
                {
                    cluster2 = clusters.Single((Cluster x) => (x.index == cluster2.index));
                }

                // create a new pair of these clusters
                var newTuple = new Tuple<Cluster, Cluster>(cluster1, cluster2);
                if (clusterDistances.ContainsKey(newTuple))
                    continue;

                newTuple.Item1.size = 1;
                newTuple.Item2.size = 1;
                newTuple.Item1.values = allDistances[i];
                newTuple.Item2.values = allDistances[j];

                clusterDistances[newTuple] = SquaredEuclideanDistance(allDistances[i], allDistances[j]);
            }
        }

        // apply https://en.wikipedia.org/wiki/Ward%27s_method
        // repeat until there is only one cluster
        while (clusters.Count > 1)
        {
            // find the lowest distance between two clusters
            float lowestDistance = float.PositiveInfinity;
            Tuple<Cluster, Cluster> lowestDistanceClusters = null;
            foreach (var entry in clusterDistances)
            {
                if (entry.Value < lowestDistance)
                {
                    lowestDistance = entry.Value;
                    lowestDistanceClusters = entry.Key;
                }
            }

            int index1 = lowestDistanceClusters.Item1.index;
            int index2 = lowestDistanceClusters.Item2.index;

            // merge the clusters
            List<float> newIdentity = CreateNewIdentity(lowestDistanceClusters.Item1.values, lowestDistanceClusters.Item2.values, numDistances);
            var newCluster = new Cluster
            {
                child1 = lowestDistanceClusters.Item1,
                child2 = lowestDistanceClusters.Item2,
                // choose the lower index
                index = index1 < index2 ? index1 : index2,
                size = lowestDistanceClusters.Item1.size + lowestDistanceClusters.Item2.size,
                values = newIdentity
            };

            // remove all cluster distances that will be updated with distances to the new cluster
            var toBeRemoved = new List<Tuple<Cluster, Cluster>>();
            foreach (var entry in clusterDistances)
            {
                if (entry.Key.Item1.index == index1 || entry.Key.Item2.index == index1 || entry.Key.Item1.index == index2 || entry.Key.Item2.index == index2)
                {
                    toBeRemoved.Add(entry.Key);
                }
            }
            foreach (var entry in toBeRemoved)
            {
                clusterDistances.Remove(entry);
            }
            // remove the clusters that we are merging
            clusters.RemoveWhere((Cluster x) => (x.index == index1 || x.index == index2));

            // calculate new distances
            foreach (var cluster in clusters)
            {
                var newClusters = new Tuple<Cluster, Cluster>(newCluster, cluster);
                int totalSize = newCluster.size + cluster.size;

                float term1 = ((newCluster.child1.size + cluster.size) / (float)totalSize) * SquaredEuclideanDistance(newCluster.child1.values, cluster.values);
                float term2 = ((newCluster.child2.size + cluster.size) / (float)totalSize) * SquaredEuclideanDistance(newCluster.child2.values, cluster.values);
                float term3 = ((cluster.size) / (float)totalSize) * lowestDistance;
                float newDistance = term1 + term2 - term3;

                clusterDistances.Add(newClusters, newDistance);
            }

            clusters.Add(newCluster);
        }
        // reorder the plots
        ReorderPlots(clusters.First());

        currentParent.transform.position = new Vector3(0.5f, 0.5f, 0.5f);
        currentParent.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        //yield break;
    }

    private class NodeTupleComparer : IEqualityComparer<Tuple<Cluster, Cluster>>
    {
        public bool Equals(Tuple<Cluster, Cluster> x, Tuple<Cluster, Cluster> y)
        {
            return x.Item1.index == y.Item1.index && x.Item2.index == y.Item2.index
                || x.Item1.index == y.Item2.index && x.Item2.index == y.Item1.index;
        }

        public int GetHashCode(Tuple<Cluster, Cluster> x)
        {
            return x.Item1.index.GetHashCode() ^ x.Item2.index.GetHashCode();
        }
    }

    private class Cluster : IComparable<Cluster>, IEquatable<Cluster>
    {
        public List<float> values;
        public int index;
        public int size;
        public Cluster child1;
        public Cluster child2;

        public int CompareTo(Cluster other)
        {
            return index - other.index;
        }

        public bool Equals(Cluster other)
        {
            return index == other.index;
        }

        public override int GetHashCode()
        {
            return index.GetHashCode();
        }

        public override string ToString()
        {
            return "Cluster " + index;
        }
    }

    private List<float> CreateNewIdentity(List<float> values1, List<float> values2, int numValues)
    {
        var result = new List<float>(numValues);

        for (int i = 0; i < numValues; ++i)
        {
            float value1 = i < values1.Count ? values1[i] : 0f;
            float value2 = i < values2.Count ? values2[i] : 0f;

            result.Add((value1 + value2) / 2f);
        }
        return result;
    }

    private void ReorderPlots(Cluster clust)
    {
        if (clust.child1 != null)
            ReorderPlots(clust.child1);

        if (clust.child2 != null)
            ReorderPlots(clust.child2);

        if (clust.child1 == null && clust.child2 == null)
        {
            plots[clust.index].transform.localPosition = startPos;
            startPos += posInc;
        }
    }

    public void CreateDistancePlot(string geneName, List<CombinedGraph.CombinedGraphPoint> cells = null)
    {
        graph = referenceManager.graphManager.FindGraph("DDRtree");
        smoothing = true;
        invert = false;

        //database.QueryGene(geneName, CollectResultAndCreatePlot);
        var cellNames = new string[cells.Count];
        for (int i = 0; i < cells.Count; ++i)
        {
            cellNames[i] = cells[i].Label;
        }

        database.QueryGenesInCells(geneName, cellNames, CollectResultAndCreatePlot);
    }

    public void CollectResultAndCreatePlot(SQLiter.SQLite database)
    {
        List<Tuple<string, float>> expressions = new List<Tuple<string, float>>();
        List<float> distances = new List<float>(expressions.Count);

        int numNotInDatabase = referenceManager.graphManager.FindGraph("DDRtree").points.Count - database._result.Count;

        for (int i = 0; i < numNotInDatabase; ++i)
        {
            expressions.Add(new Tuple<string, float>("", 0f));
            distances.Add(0.001f);
        }

        foreach (var entry in database._result)
        {
            expressions.Add((Tuple<string, float>)entry);
        }

        expressions.Sort((Tuple<string, float> x, Tuple<string, float> y) => (x.Item2.CompareTo(y.Item2)));

        var point1 = graph.points[expressions[numNotInDatabase].Item1];
        float lowest = 0;
        float highest = float.NegativeInfinity;
        float highestExpression = expressions.Last().Item2;
        for (int i = numNotInDatabase + 1; i < expressions.Count; ++i)
        {
            var point2 = graph.points[expressions[i].Item1];
            var newDist = /*Mathf.Log(*/Vector3.Distance(point1.Position, point2.Position)/*, 2f)*/;
            point1 = point2;
            distances.Add(newDist);
            if (newDist < lowest)
            {
                lowest = newDist;
            }
            else if (newDist > highest)
            {
                highest = newDist;
            }

        }

        for (int i = numNotInDatabase; i < distances.Count; ++i)
        {
            if (invert)
            {
                distances[i] = highest - distances[i];
            }
            distances[i] = (distances[i] - lowest) / (highest - lowest);
        }



        // smoothing takes the median of the 10 adjecent values (5 previous and 5 next values)
        if (smoothing)
        {
            //TODO CELLEXAL: this is a pretty slow way of doing this, should probably tie a index to each float and remove the lowest index after each calculated median
            List<float> medianDistances = new List<float>(distances.Count);
            List<float> temp = new List<float>();
            for (int i = 0; i < distances.Count; ++i)
            {
                for (int j = i - 5; j <= i + 5; ++j)
                {
                    if (j < 0 || j >= distances.Count)
                    {
                        continue;
                    }
                    temp.Add(distances[j]);
                }
                temp.Sort();
                medianDistances.Add(temp[temp.Count / 2]);
                temp.Clear();
            }
            distances = medianDistances;
        }

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[6 * (distances.Count - 1) + 8];
        Vector3[] normals = new Vector3[6 * (distances.Count - 1) + 8];
        Vector2[] uvs = new Vector2[6 * (distances.Count - 1) + 8];
        int[] triangles = new int[24 * (distances.Count - 1) + 36];

        float desiredTotalWidth = 10f;
        float individualWidth = desiredTotalWidth / distances.Count;
        float desiredDepth = 0.1f;

        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(0, 0, desiredDepth);
        vertices[2] = new Vector3(0, distances[0], 0);
        vertices[3] = new Vector3(0, distances[0], desiredDepth);

        normals[0] = new Vector3(0, -1f, 0);
        normals[1] = new Vector3(0, -1f, 0);
        normals[2] = new Vector3(0, 1f, 0);
        normals[3] = new Vector3(0, 1f, 0);

        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(0, 0);
        uvs[2] = new Vector2(0, distances[0]);
        uvs[3] = new Vector2(0, distances[0]);

        for (int i = 4, distindex = 0; i < vertices.Length - 4; i += 6, ++distindex)
        {
            float x = individualWidth * (distindex + 1);
            vertices[i] = new Vector3(x, 0, 0);
            vertices[i + 1] = new Vector3(x, 0, desiredDepth);
            vertices[i + 2] = new Vector3(x, distances[distindex], 0);
            vertices[i + 3] = new Vector3(x, distances[distindex], desiredDepth);
            vertices[i + 4] = new Vector3(x, distances[distindex + 1], 0);
            vertices[i + 5] = new Vector3(x, distances[distindex + 1], desiredDepth);

            normals[i] = new Vector3(0, -1f, 0);
            normals[i + 1] = new Vector3(0, -1f, 0);
            normals[i + 2] = new Vector3(0, 1f, 0);
            normals[i + 3] = new Vector3(0, 1f, 0);
            normals[i + 4] = new Vector3(0, 1f, 0);
            normals[i + 5] = new Vector3(0, 1f, 0);

            uvs[i] = new Vector2(x, 0);
            uvs[i + 1] = new Vector2(x, 0);
            uvs[i + 2] = new Vector2(x, distances[distindex]);
            uvs[i + 3] = new Vector2(x, distances[distindex]);
            uvs[i + 4] = new Vector2(x, distances[distindex + 1]);
            uvs[i + 5] = new Vector2(x, distances[distindex + 1]);
        }

        vertices[vertices.Length - 4] = new Vector3(desiredTotalWidth, 0, 0);
        vertices[vertices.Length - 3] = new Vector3(desiredTotalWidth, 0, desiredDepth);
        vertices[vertices.Length - 2] = new Vector3(desiredTotalWidth, distances[distances.Count - 1], 0);
        vertices[vertices.Length - 1] = new Vector3(desiredTotalWidth, distances[distances.Count - 1], desiredDepth);

        normals[vertices.Length - 4] = new Vector3(0, -1f, 0);
        normals[vertices.Length - 3] = new Vector3(0, -1f, 0);
        normals[vertices.Length - 2] = new Vector3(0, 1f, 0);
        normals[vertices.Length - 1] = new Vector3(0, 1f, 0);

        uvs[vertices.Length - 4] = new Vector2(desiredTotalWidth, 0);
        uvs[vertices.Length - 3] = new Vector2(desiredTotalWidth, 0);
        uvs[vertices.Length - 2] = new Vector2(desiredTotalWidth, distances[distances.Count - 1]);
        uvs[vertices.Length - 1] = new Vector2(desiredTotalWidth, distances[distances.Count - 1]);

        // first sides
        // left side
        FillRectangle(triangles, 0, 0, 1, 2, 3);

        // top side
        FillRectangle(triangles, 6, 2, 3, 6, 7);

        // front side
        FillRectangle(triangles, 12, 2, 6, 0, 4);

        // back side
        FillRectangle(triangles, 18, 3, 1, 7, 5);

        for (int i = 24, vertexCount = 4; i < triangles.Length - 36; i += 24, vertexCount += 6)
        {
            // left side
            FillRectangle(triangles, i, vertexCount + 2, vertexCount + 3, vertexCount + 4, vertexCount + 5);

            // top side
            FillRectangle(triangles, i + 6, vertexCount + 4, vertexCount + 5, vertexCount + 8, vertexCount + 9);

            // front side
            FillRectangle(triangles, i + 12, vertexCount, vertexCount + 4, vertexCount + 6, vertexCount + 8);

            // back side
            FillRectangle(triangles, i + 18, vertexCount + 5, vertexCount + 1, vertexCount + 9, vertexCount + 7);
        }

        // last sides
        // left side
        FillRectangle(triangles, triangles.Length - 36, vertices.Length - 8, vertices.Length - 7, vertices.Length - 6, vertices.Length - 5);

        // top side
        FillRectangle(triangles, triangles.Length - 30, vertices.Length - 6, vertices.Length - 5, vertices.Length - 2, vertices.Length - 1);

        // front side
        FillRectangle(triangles, triangles.Length - 24, vertices.Length - 10, vertices.Length - 6, vertices.Length - 4, vertices.Length - 2);

        // back side
        FillRectangle(triangles, triangles.Length - 18, vertices.Length - 9, vertices.Length - 3, vertices.Length - 5, vertices.Length - 1);

        // last right side
        FillRectangle(triangles, triangles.Length - 12, vertices.Length - 4, vertices.Length - 2, vertices.Length - 3, vertices.Length - 1);

        // under side
        FillRectangle(triangles, triangles.Length - 6, 0, vertices.Length - 4, 1, vertices.Length - 3);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;

        var data = Instantiate(plotGameObjectPrefab, startPos, Quaternion.identity, currentParent.transform);
        plots.Add(data);
        //startPos += posInc;
        mesh.RecalculateBounds();
        data.GetComponent<MeshFilter>().mesh = mesh;

        data.GetComponent<BoxCollider>().size = new Vector3(desiredTotalWidth, 1, desiredDepth);
        data.GetComponent<BoxCollider>().center = new Vector3(desiredTotalWidth / 2f, 0.5f, desiredDepth / 2f);


        var expressionGradient = Instantiate(gradientBoxPrefab, data.transform);
        expressionGradient.transform.localPosition = new Vector3(5f, -0.1f, 0f);
        var plotBounds = mesh.bounds.size;
        var expressionGradientMeshBounds = expressionGradient.GetComponent<MeshFilter>().mesh.bounds.size;

        var plotLength = plotBounds.x;
        var expressionGradientBoxLength = expressionGradientMeshBounds.x;
        var plotDepth = plotBounds.z;
        var expressionGradientBoxDepth = expressionGradientMeshBounds.z;

        var scale = expressionGradient.transform.localScale;
        scale.x *= plotLength / expressionGradientBoxLength;
        scale.z *= plotDepth / expressionGradientBoxDepth;
        expressionGradient.transform.localScale = scale;

        var expressionsArray = new float[100];
        for (int i = 0; i < 100; ++i)
        {
            expressionsArray[i] = (expressions[(int)(((expressions.Count - 1) / 99f) * i)].Item2) / highestExpression;
        }

        var expressionGradientMesh = expressionGradient.GetComponent<MeshFilter>().mesh;

        expressionGradient.GetComponent<Renderer>().material.SetFloatArray("_Expressions", expressionsArray);
        allDistances.Add(distances);
    }

    /// <summary>
    /// Helper function when creating triangles in a mesh. Fills a rectangle defined by 4 vertices using two triangles.
    /// </summary>
    /// <param name="triangles"> The array of triangles to write to.</param>
    /// <param name="startindex"> The first index of the first vertex in the first triangle. </param>
    /// <param name="vertex1"> The first vertex. </param>
    /// <param name="vertex2"> The second vertex. </param>
    /// <param name="vertex3"> The third vertex. </param>
    /// <param name="vertex4"> The fourth vertex. </param>
    private void FillRectangle(int[] triangles, int startindex, int vertex1, int vertex2, int vertex3, int vertex4)
    {
        triangles[startindex] = vertex1;
        triangles[startindex + 1] = vertex2;
        triangles[startindex + 2] = vertex3;

        triangles[startindex + 3] = vertex2;
        triangles[startindex + 4] = vertex4;
        triangles[startindex + 5] = vertex3;
    }

    /// <summary>
    /// Calculates the squared euclidean distance between two lists of floats.
    /// The lists must have the same legnth.
    /// </summary>
    /// <param name="l1">The first list</param>
    /// <param name="l2">The second list</param>
    /// <returns>The squared euclidean distance</returns>
    private float SquaredEuclideanDistance(List<float> l1, List<float> l2)
    {
        float result = 0f;
        for (int i = 0; i < l1.Count; ++i)
        {
            result += (l1[i] - l2[i]) * (l1[i] - l2[i]);
        }
        return result;
    }
}
