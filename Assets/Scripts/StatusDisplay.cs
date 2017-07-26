using UnityEngine;
using System.Collections.Generic;

public class StatusDisplay : MonoBehaviour
{

    public GameObject statusPrefab;
    private Dictionary<int, TextMesh> statuses = new Dictionary<int, TextMesh>();
    private Vector3 startSpawnPos = new Vector3(.055f, -.017f, -.001f);
    private Vector3 nextSpawnPos = new Vector3(.055f, -.017f, -.001f);
    private Vector3 dnextSpawnPos = new Vector3(0f, -.007f, 0f);
    private List<Transform> statusPositions = new List<Transform>();
    private int statusId = 0;


    void Start()
    {

    }

    public int AddStatus(string text)
    {
        var newStatus = Instantiate(statusPrefab);
        newStatus.transform.parent = transform;
        newStatus.transform.localPosition = nextSpawnPos;
        newStatus.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
        nextSpawnPos += dnextSpawnPos;
        statusPositions.Add(newStatus.transform);
        var textMesh = newStatus.GetComponent<TextMesh>();
        textMesh.text = text;
        statuses[statusId] = textMesh;
        statusId++;
        return statusId - 1;
    }

    public void UpdateStatus(int id, string text)
    {
        statuses[id].text = text;
    }

    public void RemoveStatus(int id)
    {
        statusPositions.Remove(statuses[id].transform);
        Destroy(statuses[id]);
        statuses.Remove(id);
        UpdateStatusPositions();
    }

    private void UpdateStatusPositions()
    {
        if (statusPositions.Count == 0) return;

        statusPositions[0].localPosition = startSpawnPos;
        for (int i = 1; i < statusPositions.Count; ++i)
        {
            statusPositions[i].localPosition = statusPositions[i - 1].localPosition + dnextSpawnPos;
        }
        nextSpawnPos = statusPositions[statusPositions.Count - 1].localPosition;
    }

}