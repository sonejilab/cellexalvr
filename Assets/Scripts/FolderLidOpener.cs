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
        // calcuate an array of how much to rotate the lid every frame.
        // uses a sinus function to make it a bit more smooth.
        dAngle = new float[moveTime];
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


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Smaller Controller Collider"))
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
        if (other.gameObject.CompareTag("Smaller Controller Collider"))
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