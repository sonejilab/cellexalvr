using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Handles the statuses displayed on the right controller.
/// </summary>
public class StatusDisplay : MonoBehaviour
{

    public GameObject statusPrefab;
    public GameObject defaultStatus;

    private Dictionary<int, TextMesh> statuses = new Dictionary<int, TextMesh>();
    private List<Transform> statusPositions = new List<Transform>();
    public Vector3 startSpawnPos = new Vector3(.055f, -.015f, -.001f);
    public Vector3 nextSpawnPos = new Vector3(.055f, -.015f, -.001f);
    public Vector3 dnextSpawnPos = new Vector3(0f, .007f, 0f);
    public Vector3 newStatusScale = new Vector3(.00025f, .00025f, .00025f);
    public Quaternion rotation;
    private bool active = false;
    private int statusId = 0;

    /// <summary>
    /// Toggles the status display by enabling or disabling all its renderers.
    /// </summary>
    public void ToggleStatusDisplay()
    {
        active = !active;
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
        {
            r.enabled = active;
        }
    }

    /// <summary>
    /// Adds a status with a specified color. Calling this with Color.white is equivalent to just calling AddStatus
    /// </summary>
    /// <param name="text"> The status text. </param>
    /// <param name="color"> The status text's color. </param>
    /// <returns> The status' id. You will need this when removing the status. </returns>
    public int AddStatus(string text, Color color)
    {
        var statusId = AddStatus(text);
        statuses[statusId].gameObject.GetComponent<Renderer>().material.color = color;
        return statusId;
    }

    /// <summary>
    /// Adds a status to the display.
    /// </summary>
    /// <param name="text"> The status text. </param>
    /// <returns> The status' id. You will need this when removing the status. </returns>
    public int AddStatus(string text)
    {
        defaultStatus.SetActive(false);
        var newStatus = Instantiate(statusPrefab);
        newStatus.transform.parent = transform;
        newStatus.transform.localPosition = nextSpawnPos;
        newStatus.transform.localRotation = rotation;
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
        if (!statuses.ContainsKey(id))
        {
            // that status didn't exist
            return;
        }
        statusPositions.Remove(statuses[id].transform);
        Destroy(statuses[id].gameObject);
        statuses.Remove(id);
        // if the status we deleted was not the last status, we should update the positions
        UpdateStatusPositions();
        if (statuses.Count == 0)
        {
            defaultStatus.SetActive(true);
        }
    }

    /// <summary>
    /// Shows a status for a time, then removes it.
    /// </summary>
    /// <param name="text"> The status text. </param>
    /// <param name="time"> The time this status should be shown in seconds. </param>
    /// <param name="color"> The status' color. </param>
    public void ShowStatusForTime(string text, float time, Color color)
    {
        if (active)
        {
            StartCoroutine(ShowStatusForTimeCoroutine(text, time, color));
        }
    }

    /// <summary>
    /// Shows a status for a time, then removes it.
    /// </summary>
    /// <param name="text"> The status text. </param>
    /// <param name="time"> The time this status should be shown in seconds. </param>
    public void ShowStatusForTime(string text, float time)
    {
        StartCoroutine(ShowStatusForTimeCoroutine(text, time, Color.white));
    }

    private IEnumerator ShowStatusForTimeCoroutine(string text, float time, Color color)
    {
        var statusId = AddStatus(text);
        statuses[statusId].gameObject.GetComponent<Renderer>().material.color = color;
        yield return new WaitForSeconds(time);
        RemoveStatus(statusId);
    }

    /// <summary>
    /// Updates the positions of the statuses by ordering them after time created.
    /// </summary>
    private void UpdateStatusPositions()
    {
        if (statusPositions.Count == 0)
        {
            nextSpawnPos = startSpawnPos;
        }
        else
        {
            statusPositions[0].localPosition = startSpawnPos;
            for (int i = 1; i < statusPositions.Count; ++i)
            {
                statusPositions[i].localPosition = statusPositions[i - 1].localPosition + dnextSpawnPos;
            }
            nextSpawnPos = statusPositions[statusPositions.Count - 1].localPosition + dnextSpawnPos;
        }
    }

}