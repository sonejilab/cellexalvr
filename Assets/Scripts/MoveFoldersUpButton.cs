﻿using System.Collections;
using UnityEngine;

/// <summary>
/// This class represents the button that moves the folders when a controller is moved inside of it.
/// </summary>
public class MoveFoldersUpButton : MonoBehaviour
{
    public Transform folderList;
    public int moveTime = 10;
    public float[] dY;
    private bool controllerInside = false;
    private bool coroutineRunning = false;

    private void Start()
    {
        dY = new float[moveTime];
        //var tau = 2 * Mathf.PI;
        var total = 0f;
        for (int i = 0; i < moveTime; ++i)
        {
            dY[i] = Mathf.Sin(Mathf.PI * ((float)i / moveTime));
            total += dY[i];
        }
        for (int i = 0; i < moveTime; ++i)
        {
            dY[i] /= total;
        }
    }


    private void Update()
    {
        if (controllerInside && !coroutineRunning)
            StartCoroutine(MoveFolders());
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Smaller Controller Collider"))
        {
            controllerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Smaller Controller Collider"))
        {
            controllerInside = false;
        }
    }

    IEnumerator MoveFolders()
    {
        coroutineRunning = true;
        for (int i = 0; i < moveTime; ++i)
        {
            folderList.Translate(0f, dY[i], 0f);
            yield return null;
        }
        coroutineRunning = false;
    }
}
