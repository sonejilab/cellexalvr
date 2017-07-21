using System.Collections;
using UnityEngine;

public class MoveFoldersDownButton : MonoBehaviour
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
            dY[i] = -Mathf.Sin(Mathf.PI * ((float)i / moveTime));
            total += Mathf.Abs(dY[i]);
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
        if (other.tag == "Controller")
        {
            controllerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Controller")
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