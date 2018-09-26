using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class CombinedGraph : MonoBehaviour
{
    public GameObject tempGraphPointPrefab;
    public GameObject combinedGraphpointsPrefab;
    public Mesh graphPointMesh;

    //public float maxClusterSize = 50;
    public int nbrOfClusters = 25;
    public int nbrOfMaxIterations = 100;

    public List<CombinedGraphPoint> points = new List<CombinedGraphPoint>();
    public string GraphName { get; set; }
    public string DirectoryName { get; set; }

    private Vector3 minCoordValues = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    private Vector3 maxCoordValues = new Vector3(float.MinValue, float.MinValue, float.MinValue);
    private Vector3 diffCoordValues;
    private float longestAxis;
    private Vector3 scaledOffset;

    public void AddGraphPoint(string label, float x, float y, float z)
    {
        points.Add(new CombinedGraphPoint(label, x, y, z));
    }

    private float SquaredEuclideanDistance(Vector3 v1, Vector3 v2)
    {
        float distance = Vector3.Distance(v1, v2);
        return distance * distance;
    }

    public void SliceClustering()
    {
        StartCoroutine(SliceClusteringCoroutine());
    }

    private IEnumerator SliceClusteringCoroutine()
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        ScaleAllCoordinates();

        int clusterMaxSize = 65534 / graphPointMesh.vertexCount;
        // nbrOfClusters = (int)Math.Ceiling((float)points.Count * graphPointMesh.vertexCount / 65534);
        var clusters = new List<HashSet<CombinedGraphPoint>>();

        // place all points in one big cluster
        var firstCluster = new HashSet<CombinedGraphPoint>();
        foreach (var point in points)
        {
            firstCluster.Add(point);
        }

        Thread t = new Thread(() => clusters = SplitCluster(firstCluster, clusterMaxSize));
        t.Start();
        while (t.IsAlive)
        {
            yield return null;
        }
        stopwatch.Stop();
        print(string.Format("clustered {0} in {1}", GraphName, stopwatch.Elapsed.ToString()));
        stopwatch.Restart();
        MakeMeshes(clusters);
        stopwatch.Stop();
        print(string.Format("made meshes for {0} in {1}", GraphName, stopwatch.Elapsed.ToString()));
    }

    private List<HashSet<CombinedGraphPoint>> SplitCluster(HashSet<CombinedGraphPoint> cluster, int clusterMaxSize)
    {
        var result = new List<HashSet<CombinedGraphPoint>>();
        if (cluster.Count <= clusterMaxSize)
        {
            result.Add(cluster);
            return result;
        }

        // cluster is too big, split it
        // calculate center
        Vector3 splitCenter = Vector3.zero;
        foreach (var point in cluster)
        {
            splitCenter += point.Position;
        }
        splitCenter /= cluster.Count;

        // initialise new clusters
        List<HashSet<CombinedGraphPoint>> newClusters = new List<HashSet<CombinedGraphPoint>>(8);
        for (int i = 0; i < 8; ++i)
        {
            newClusters.Add(new HashSet<CombinedGraphPoint>());
        }

        // assign points to the new clusters
        foreach (var point in cluster)
        {
            int chosenClusterIndex = 0;
            Vector3 position = point.Position;
            if (point.Position.x > splitCenter.x)
            {
                chosenClusterIndex += 1;
            }
            if (position.y > splitCenter.y)
            {
                chosenClusterIndex += 2;
            }
            if (position.z > splitCenter.z)
            {
                chosenClusterIndex += 4;
            }
            newClusters[chosenClusterIndex].Add(point);
        }

        bool[] mergedIndices = new bool[8];
        for (int i = 0; i < mergedIndices.Length; ++i)
        {
            mergedIndices[i] = false;
        }


        // merge clusters
        // Merge2Clusters(ref newClusters, ref mergedIndices, 0, 1, clusterMaxSize);
        // Merge2Clusters(ref newClusters, ref mergedIndices, 2, 3, clusterMaxSize);
        // Merge2Clusters(ref newClusters, ref mergedIndices, 4, 5, clusterMaxSize);
        // Merge2Clusters(ref newClusters, ref mergedIndices, 6, 7, clusterMaxSize);

        // Merge2Clusters(ref newClusters, ref mergedIndices, 3, 7, clusterMaxSize);
        // Merge2Clusters(ref newClusters, ref mergedIndices, 0, 4, clusterMaxSize);
        // Merge2Clusters(ref newClusters, ref mergedIndices, 1, 5, clusterMaxSize);
        // Merge2Clusters(ref newClusters, ref mergedIndices, 2, 6, clusterMaxSize);

        // Merge2Clusters(ref newClusters, ref mergedIndices, 0, 2, clusterMaxSize);
        // Merge2Clusters(ref newClusters, ref mergedIndices, 1, 3, clusterMaxSize);
        // Merge2Clusters(ref newClusters, ref mergedIndices, 4, 6, clusterMaxSize);
        // Merge2Clusters(ref newClusters, ref mergedIndices, 5, 7, clusterMaxSize);

        // call recursively for each new cluster
        for (int i = 0; i < newClusters.Count; ++i)
        {
            result.AddRange(SplitCluster(newClusters[i], clusterMaxSize));
        }
        return result;
    }

    // private bool Merge4Clusters(ref List<HashSet<CombinedGraphPoint>> clusters, int clusterIndex1, int clusterIndex2, int clusterIndex3, int clusterIndex4, int clusterMaxSize)
    // {
    //
    // }

    //private void Merge2Clusters(ref List<HashSet<CombinedGraphPoint>> clusters, ref bool[] mergedIndices, int clusterIndex1, int clusterIndex2, int clusterMaxSize)
    //{
    //    // check if the two clusters are already merged
    //    if (mergedIndices[clusterIndex1] || mergedIndices[clusterIndex2])
    //    {
    //        return;
    //    }
    //    // check if the merged cluster would be too large
    //    var cluster1 = clusters[clusterIndex1];
    //    var cluster2 = clusters[clusterIndex2];
    //    if (cluster1.Count + cluster2.Count > clusterMaxSize)
    //    {
    //        return;
    //    }
    //
    //    // merge the cluster
    //    cluster1.UnionWith(cluster2);
    //    cluster2.Clear();
    //    mergedIndices[clusterIndex1] = true;
    //    mergedIndices[clusterIndex2] = true;
    //}

    public void KMeansClustering()
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        ScaleAllCoordinates();
        // a mesh may only have 65534 vertices
        int clusterMaxSize = 65534 / graphPointMesh.vertexCount;
        nbrOfClusters = (int)Math.Ceiling((float)points.Count * graphPointMesh.vertexCount / 65534);
        int[] clusterAssignment = new int[points.Count];
        Vector3[] centroids = new Vector3[nbrOfClusters * 2];
        int[] clusterSizes = new int[nbrOfClusters * 2];
        //int nbrOfClusterLess = points.Count;
        int i = 0;
        // give the clusters random starting positions
        for (i = 0; i < nbrOfClusters; ++i)
        {
            centroids[i] = points[UnityEngine.Random.Range(0, points.Count)].Position;
        }
        // all points start out in no cluster
        for (i = 0; i < clusterAssignment.Length; ++i)
        {
            clusterAssignment[i] = -1;

        }

        for (i = 0; i < nbrOfMaxIterations; ++i)
        {
            // assign points to centroids
            int nbrOfChanges = 0;
            for (int j = 0; j < points.Count; ++j)
            {
                var point = points[j];
                float smallestDistance = float.MaxValue;
                int closestCentroid = -1;
                for (int k = 0; k < nbrOfClusters; ++k)
                {
                    float distance = SquaredEuclideanDistance(centroids[k], point.Position);
                    if (distance < smallestDistance)
                    {
                        closestCentroid = k;
                        smallestDistance = distance;
                    }
                }

                if (clusterAssignment[j] != closestCentroid)
                {
                    nbrOfChanges++;
                    if (clusterAssignment[j] != -1)
                    {
                        // reduce the size of the cluster this point was in
                        clusterSizes[clusterAssignment[j]]--;
                    }
                    // increase the size og the cluster we are moving this point to
                    clusterSizes[closestCentroid]++;
                    // assign the point to the cluster
                    clusterAssignment[j] = closestCentroid;
                }
            }
            int nbrOfClustersAdded = 0;
            for (int j = 0; j < nbrOfClusters; ++j)
            {
                if (clusterSizes[j] > clusterMaxSize)
                {
                    // split cluster
                    int newClusterIndex = nbrOfClusters + nbrOfClustersAdded;
                    if (newClusterIndex >= centroids.Length)
                    {
                        // increase array sizes if needed
                        Vector3[] newCentroids = new Vector3[nbrOfClusters * 2];
                        Array.Copy(centroids, newCentroids, nbrOfClusters);
                        centroids = newCentroids;
                        int[] newClusterSizes = new int[nbrOfClusters * 2];
                        Array.Copy(clusterSizes, newClusterSizes, nbrOfClusters);
                        clusterSizes = newClusterSizes;
                    }
                    // move the clusters apart slightly
                    Vector3 firstPoint = FindFirstPoint(ref clusterAssignment, j);
                    Vector3 distanceToMove = (centroids[j] - firstPoint) / 2f;
                    centroids[newClusterIndex] = centroids[j] - distanceToMove;
                    centroids[j] = centroids[j] + distanceToMove;
                    clusterSizes[j] = 0;
                    clusterSizes[newClusterIndex] = 0;
                    // reassign points to their now nearest cluster
                    for (int k = 0; k < clusterAssignment.Length; ++k)
                    {
                        if (clusterAssignment[k] == j)
                        {
                            float dist1 = SquaredEuclideanDistance(points[k].Position, centroids[j]);
                            float dist2 = SquaredEuclideanDistance(points[k].Position, centroids[newClusterIndex]);
                            int nearestCluster = dist1 < dist2 ? j : newClusterIndex;
                            clusterAssignment[k] = nearestCluster;
                            clusterSizes[nearestCluster]++;
                            nbrOfChanges++;
                        }
                    }
                    nbrOfClustersAdded++;
                }
            }
            nbrOfClusters += nbrOfClustersAdded;

            if (nbrOfChanges < nbrOfClusters * 5)
            {
                i++;
                break;
            }

            // reset centroids
            for (int j = 0; j < nbrOfClusters; ++j)
            {
                centroids[j] = Vector3.zero;
            }

            // move centroids
            for (int j = 0; j < clusterAssignment.Length; ++j)
            {
                if (clusterAssignment[j] != -1)
                {
                    centroids[clusterAssignment[j]] += points[j].Position;
                }
            }
            for (int j = 0; j < nbrOfClusters; ++j)
            {
                centroids[j] /= clusterSizes[j];
            }
        }

        stopwatch.Stop();
        print(string.Format("clustered {0} in {1} iterations in {2}", GraphName, i, stopwatch.Elapsed.ToString()));
        stopwatch.Restart();
        MakeMeshes(ref clusterAssignment, ref nbrOfClusters, ref clusterSizes);
        stopwatch.Stop();
        print(string.Format("made meshes for {0} in {1}", GraphName, stopwatch.Elapsed.ToString()));

    }

    private Vector3 FindFirstPoint(ref int[] clusterAssignment, int cluster)
    {
        for (int i = 0; i < clusterAssignment.Length; ++i)
        {
            if (clusterAssignment[i] == cluster)
            {
                return points[i].Position;
            }
        }
        return Vector3.zero;
    }

    private void ScaleAllCoordinates()
    {
        diffCoordValues = maxCoordValues - minCoordValues;
        longestAxis = Math.Max(Math.Max(diffCoordValues.x, diffCoordValues.y), diffCoordValues.z);
        scaledOffset = (diffCoordValues / longestAxis) / 2;

        foreach (var point in points)
        {
            point.Position = ScaleCoordinates(point.Position);
        }
    }

    private void MakeMeshes(ref int[] clusterAssignment, ref int nbrOfClusters, ref int[] clusterSizes)
    {
        diffCoordValues = maxCoordValues - minCoordValues;
        longestAxis = Math.Max(Math.Max(diffCoordValues.x, diffCoordValues.y), diffCoordValues.z);
        scaledOffset = (diffCoordValues / longestAxis) / 2;
        CombineInstance[][] combine = new CombineInstance[nbrOfClusters][];
        // the next index to add a point to in each cluster
        int[] indices = new int[nbrOfClusters];

        for (int i = 0; i < nbrOfClusters; ++i)
        {
            combine[i] = new CombineInstance[clusterSizes[i]];
        }

        for (int i = 0; i < clusterAssignment.Length; ++i)
        {
            int cluster = clusterAssignment[i];
            var newCombine = new CombineInstance();
            newCombine.mesh = graphPointMesh;
            newCombine.transform = Matrix4x4.TRS(points[i].Position, Quaternion.identity, Vector3.one * 0.6f);
            combine[cluster][indices[cluster]] = newCombine;
            indices[cluster]++;
        }

        for (int i = 0; i < nbrOfClusters; ++i)
        {
            CombineMeshes(combine[i]);
        }
    }

    private void MakeMeshes(List<HashSet<CombinedGraphPoint>> clusters)
    {
        diffCoordValues = maxCoordValues - minCoordValues;
        longestAxis = Math.Max(Math.Max(diffCoordValues.x, diffCoordValues.y), diffCoordValues.z);
        scaledOffset = (diffCoordValues / longestAxis) / 2;

        for (int i = 0; i < clusters.Count; ++i)
        {
            var cluster = clusters[i];

            if (cluster.Count == 0)
                continue;

            CombineInstance[] combine = new CombineInstance[cluster.Count];
            int j = 0;
            foreach (var point in cluster)
            {
                combine[j] = new CombineInstance();
                combine[j].mesh = graphPointMesh;
                combine[j].transform = Matrix4x4.TRS(point.Position, Quaternion.identity, Vector3.one * 0.6f);
                j++;
            }
            CombineMeshes(combine);
        }
    }

    private void CombineMeshes(CombineInstance[] combine)
    {
        var newCombinedPart = Instantiate(combinedGraphpointsPrefab);
        newCombinedPart.transform.parent = transform;
        var meshFilter = newCombinedPart.GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.CombineMeshes(combine, true, true);
        meshFilter.mesh.RecalculateBounds();
        meshFilter.mesh.RecalculateNormals();

        newCombinedPart.GetComponent<Renderer>().material.color = UnityEngine.Random.ColorHSV(0f, 1f, .5f, 1f, .5f, 1f);
    }

    public Vector3 ScaleCoordinates(Vector3 coords)
    {
        // move one of the graph's corners to origo
        coords -= minCoordValues;

        // uniformly scale all axes down based on the longest axis
        // this makes the longest axis have length 1 and keeps the proportions of the graph
        coords /= longestAxis;

        // move the graph a bit so (0, 0, 0) is the center point
        coords -= scaledOffset;

        return coords;
    }

    public void UpdateMinMaxCoords(float x, float y, float z)
    {
        if (x < minCoordValues.x)
            minCoordValues.x = x;
        if (y < minCoordValues.y)
            minCoordValues.y = y;
        if (z < minCoordValues.z)
            minCoordValues.z = z;
        if (x > maxCoordValues.x)
            maxCoordValues.x = x;
        if (y > maxCoordValues.y)
            maxCoordValues.y = y;
        if (z > maxCoordValues.z)
            maxCoordValues.z = z;
    }
}
