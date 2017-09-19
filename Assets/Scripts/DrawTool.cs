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
    public Color LineColor = Color.white;

    private SteamVR_TrackedObject rightController;
    private Transform rightControllerTransform;
    private BoxCollider controllerMenuCollider;
    private List<LineRenderer> lines = new List<LineRenderer>();
    private LineRenderer[] temporaryLines = new LineRenderer[5];
    private int temporaryLinesIndex = 0;
    private Vector3 lastPosition;
    private bool skipNextDraw;
    private bool drawing;

    private void Start()
    {
        rightController = referenceManager.rightController;
        rightControllerTransform = rightController.gameObject.transform;
        lastPosition = rightControllerTransform.position;
        controllerMenuCollider = referenceManager.controllerMenuCollider;
    }

    private void LateUpdate()
    {

        var device = SteamVR_Controller.Input((int)rightController.index);

        if (drawing)
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

        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            if (skipNextDraw)
            {
                skipNextDraw = false;
            }
            else
            {
                // if the trigger was pressed we need to make sure that the controller is not inside a button.
                drawing = true;
                var colliders = Physics.OverlapBox(controllerMenuCollider.center, controllerMenuCollider.bounds.extents, controllerMenuCollider.gameObject.transform.rotation);
                foreach (Collider collider in colliders)
                {
                    if (collider.gameObject.GetComponent<StationaryButton>() || collider.gameObject.GetComponent<RotatableButton>())
                    {
                        drawing = false;
                        break;
                    }
                }
            }
            // this happens only once when you press the trigger
            for (int i = 0; i < temporaryLines.Length; i++)
            {
                Destroy(temporaryLines[i]);
                temporaryLines[i] = null;
            }
        }
        if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
        {
            drawing = false;
        }
        lastPosition = rightControllerTransform.position;
    }

    /// <summary>
    /// Tells the draw tool to not start drawing until the trigger is pressed again.
    /// Useful if the user is pressing a button with the draw tool activated so it won't draw a small blob.
    /// </summary>
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
        rightController = referenceManager.rightController;
        rightControllerTransform = rightController.gameObject.transform;
        lastPosition = rightControllerTransform.position;
    }

    private void OnDisable()
    {
        skipNextDraw = false;
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
        newLine.startColor = LineColor;
        newLine.endColor = LineColor;
        return newLine;
    }
}
