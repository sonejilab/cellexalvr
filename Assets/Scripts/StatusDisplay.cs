using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles the statuses displayed on the right controller.
/// </summary>
public class StatusDisplay : MonoBehaviour
{

    public GameObject statusPrefab;

    private Dictionary<int, TextMesh> statuses = new Dictionary<int, TextMesh>();
    private List<Transform> statusPositions = new List<Transform>();
    private Vector3 startSpawnPos = new Vector3(.055f, -.015f, -.001f);
    private Vector3 nextSpawnPos = new Vector3(.055f, -.015f, -.001f);
    private Vector3 dnextSpawnPos = new Vector3(0f, .007f, 0f);
    private Vector3 newStatusScale = new Vector3(.0002f, .0002f, .0002f);
    private bool active = true;
    private int statusId = 0;

    void Start()
    {
        ToggleStatusDisplay();
    }

    public void ToggleStatusDisplay()
    {
        active = !active;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = active;
        }
    }

    /// <summary>
    /// Adds a status to the display.
    /// </summary>
    /// <param name="text"> The desired status. </param>
    /// <returns> The status' id. You will need this when removing the status. </returns>
    public int AddStatus(string text)
    {
        var newStatus = Instantiate(statusPrefab);
        newStatus.transform.parent = transform;
        newStatus.transform.localPosition = nextSpawnPos;
        newStatus.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
        newStatus.transform.localScale = newStatusScale;
        newStatus.GetComponent<Renderer>().enabled = active;
        nextSpawnPos += dnextSpawnPos;
        statusPositions.Add(newStatus.transform);
        var textMesh = newStatus.GetComponent<TextMesh>();
        textMesh.text = text;
        statuses[statusId] = textMesh;
        statusId++;
        return statusId - 1;
    }

    /// <summary>
    /// Updates a status with a new text, overwriting the old.
    /// </summary>
    /// <param name="id"> The status id. (return value from AddStatus) </param>
    /// <param name="text"> The new status text. </param>
    public void UpdateStatus(int id, string text)
    {
        statuses[id].text = text;
    }

    /// <summary>
    /// Removes a status from the display.
    /// </summary>
    /// <param name="id"> The status id. (return value from AddStatus) </param>
    public void RemoveStatus(int id)
    {
        statusPositions.Remove(statuses[id].transform);
        Destroy(statuses[id].gameObject);
        statuses.Remove(id);
        // if the status we deleted was not the last status, we should update the positions
        UpdateStatusPositions();
    }

    /// <summary>
    /// Updates the positions of the statuses. The idea is that the oldest status (first added, not least recently updated) is on the top. And the rest is in a nice list below.
    /// </summary>
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