using UnityEngine;
using System.Collections;
using UnityEngine.Pool;
using CellexalVR.Spatial;
using AnalysisLogic;

public class PointCloudSpawner : MonoBehaviour
{
    public static PointCloudSpawner instance;
    public ObjectPool<PointCloud> pool;
    [SerializeField] private PointCloud pointCloudPrefab;
    private int capacity = 40;

    private void Awake()
    {
        instance = this;
        pool = new ObjectPool<PointCloud>(CreatePooledObject, OnTakePooledObject, OnReleasePooledObject, OnDestroyPooledObject, true, 40);
    }

    private PointCloud CreatePooledObject()
    {
        var pointCloud = Instantiate(pointCloudPrefab, transform);
        pointCloud.SetPool(pool);
        pointCloud.gameObject.SetActive(false);
        return pointCloud;
    }

    private void OnTakePooledObject(PointCloud pointCloud)
    {
        pointCloud.gameObject.SetActive(true);
    }

    private void OnReleasePooledObject(PointCloud pointCloud)
    {
        pointCloud.gameObject.SetActive(false);
    }

    private void OnDestroyPooledObject(PointCloud pointCloud)
    {
        Destroy(pointCloud.gameObject);
    }

    
}
