using System.Collections;
using UnityEngine;

/// <summary>
/// This class opens the lids of the boxes automatically when a controller is nearby.
/// Such luxury.
/// </summary>
public class FolderLidOpener : MonoBehaviour
{
    public int moveTime = 10;
    private float[] dAngle;
    private bool lidOpen = false;
    private bool desiredState = false;
    private bool coroutineRunning = false;

    void Start()
    {
        dAngle = new float[moveTime];
        //var tau = 2 * Mathf.PI;
        var total = 0f;
        for (int i = 0; i < moveTime; ++i)
        {
            dAngle[i] = Mathf.Sin(Mathf.PI * ((float)i / moveTime));
            total += Mathf.Abs(dAngle[i]);
        }
        for (int i = 0; i < moveTime; ++i)
        {
            dAngle[i] *= 90 / total;

        }
    }

    void Update()
    {
        if (!coroutineRunning && desiredState != lidOpen)
        {
            lidOpen = desiredState;
            StartCoroutine(SetLidOpen(desiredState));
        }
    }


    bool ControllerTag(Collider other)
    {
        var parent = other.transform.parent;
        if (parent != null)
        {
            parent = parent.parent;
            if (parent != null)
            {
                return parent.tag == "Controller";
            }
        }
        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (ControllerTag(other))
        {
            desiredState = true;
            if (!coroutineRunning && !lidOpen)
            {
                lidOpen = true;
                coroutineRunning = true;
                StartCoroutine(SetLidOpen(true));
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (ControllerTag(other))
        {
            desiredState = false;
            if (!coroutineRunning && lidOpen)
            {
                lidOpen = false;
                coroutineRunning = true;
                StartCoroutine(SetLidOpen(false));
            }
        }
    }


    IEnumerator SetLidOpen(bool open)
    {
        for (int i = 0; i < moveTime; ++i)
        {
            var angle = 0f;
            if (open)
                angle = dAngle[i];
            else
                angle = -dAngle[i];
            transform.Rotate(Vector3.forward, angle);
            yield return null;
        }
        coroutineRunning = false;
    }

}