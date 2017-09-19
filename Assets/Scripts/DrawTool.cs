using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents the draw tool. It is used to draw lines in the virtual environment.
/// </summary>
public class DrawTool : MonoBehaviour
{
    public GameObject linePrefab;
    public ReferenceManager referenceManager;


    private SteamVR_TrackedObject rightController;
    private Transform rightControllerTransform;
    private List<LineRenderer> lines = new List<LineRenderer>();
    private LineRenderer[] temporaryLines = new LineRenderer[5];
    private int temporaryLinesIndex = 0;
    private Vector3 lastPosition;
    private Color lineColor;
    private bool skipNextDraw;

    private void Start()
    {
        rightController = referenceManager.rightController;
        rightControllerTransform = rightController.gameObject.transform;
        lastPosition = rightControllerTransform.position;

    }

    private void Update()
    {

        var device = SteamVR_Controller.Input((int)rightController.index);
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            skipNextDraw = false;
            // this happens only once when you press the trigger
            for (int i = 0; i < temporaryLines.Length; i++)
            {
                Destroy(temporaryLines[i]);
                temporaryLines[i] = null;
            }
        }


        if (device.GetPress(SteamVR_Controller.ButtonMask.Trigger) && !skipNextDraw)
        {
            // this happens every frame the trigger is pressed
            var newLine = SpawnNewLine();
            lines.Add(newLine);
        }
        else
        {
            // this happens every frame when the trigger is not pressed
            var tempLine = temporaryLines[temporaryLinesIndex];
            if (tempLine != null)
            {
                Destroy(tempLine.gameObject);
            }
            temporaryLines[temporaryLinesIndex] = SpawnNewLine();
            if (temporaryLinesIndex == temporaryLines.Length - 1)
            {
                temporaryLinesIndex = 0;
            }
            else
            {
                temporaryLinesIndex++;
            }
        }
        lastPosition = rightControllerTransform.position;
    }

    internal void SkipNextDraw()
    {
        skipNextDraw = true;
    }

    /// <summary>
    /// Removes all lines that have been drawn.
    /// </summary>
    public void ClearAllLines()
    {
        foreach (LineRenderer line in lines)
        {
            Destroy(line.gameObject);
        }
        lines.Clear();
    }

    private void OnEnable()
    {
        lastPosition = rightControllerTransform.position;
    }

    private void OnDisable()
    {
        for (int i = 0; i < temporaryLines.Length; i++)
        {
            Destroy(temporaryLines[i]);
            temporaryLines[i] = null;
        }
    }

    private LineRenderer SpawnNewLine()
    {
        var newLine = Instantiate(linePrefab).GetComponent<LineRenderer>();
        newLine.SetPositions(new Vector3[] { lastPosition, rightControllerTransform.position });
        return newLine;
    }
}
