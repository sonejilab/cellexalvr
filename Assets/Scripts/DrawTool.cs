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
    private List<Vector3> newPositions = new List<Vector3>();
    private List<LineRenderer> temporaryLines = new List<LineRenderer>();
    private List<LineRenderer> lines = new List<LineRenderer>();
    private LineRenderer[] trailLines = new LineRenderer[5];
    private int temporaryLinesIndex = 0;
    private Vector3 lastPosition;
    private bool skipNextDraw;
    private bool drawing;

    private void Start()
    {
        rightController = referenceManager.rightController;
        rightControllerTransform = rightController.gameObject.transform;
        lastPosition = transform.position;
        controllerMenuCollider = referenceManager.controllerMenuCollider;
    }

    private void LateUpdate()
    {
        var device = SteamVR_Controller.Input((int)rightController.index);

        if (drawing)
        {
            // this happens every frame the trigger is pressed
            var newLine = SpawnNewLine();
            temporaryLines.Add(newLine);
        }
        else
        {
            // this happens every frame when the trigger is not pressed
            var tempLine = trailLines[temporaryLinesIndex];
            if (tempLine != null)
            {
                Destroy(tempLine.gameObject);
            }
            trailLines[temporaryLinesIndex] = SpawnNewLine();
            if (temporaryLinesIndex == trailLines.Length - 1)
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
            for (int i = 0; i < trailLines.Length; i++)
            {
                if (trailLines[i] != null)
                {
                    Destroy(trailLines[i].gameObject);
                    trailLines[i] = null;
                }
            }
        }
        if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
        {
            // this happens only once when the controller trigger is released
            if (drawing)
            {
                drawing = false;
                // merge all the lines into one gameobject
                Vector3[] newLinePositions = new Vector3[temporaryLines.Count + 1];
                float[] xcoords = new float[temporaryLines.Count + 1];
                float[] ycoords = new float[temporaryLines.Count + 1];
                float[] zcoords = new float[temporaryLines.Count + 1];
                newLinePositions[0] = temporaryLines[0].GetPosition(0);
                xcoords[0] = newLinePositions[0].x;
                ycoords[0] = newLinePositions[0].y;
                zcoords[0] = newLinePositions[0].z;

                for (int i = 1; i <= temporaryLines.Count; i++)
                {
                    newLinePositions[i] = temporaryLines[i - 1].GetPosition(1);
                    xcoords[i] = newLinePositions[i - 1].x;
                    ycoords[i] = newLinePositions[i - 1].y;
                    zcoords[i] = newLinePositions[i - 1].z;
                }
                referenceManager.gameManager.InformDrawLine(LineColor.r, LineColor.g, LineColor.b, xcoords, ycoords, zcoords);

                var newLine = Instantiate(linePrefab).GetComponent<LineRenderer>();
                newLine.positionCount = newLinePositions.Length;
                newLine.SetPositions(newLinePositions);
                newLine.startColor = LineColor;
                newLine.endColor = LineColor;
                foreach (LineRenderer line in temporaryLines)
                {
                    Destroy(line.gameObject);
                }
                temporaryLines.Clear();
            }
        }
        lastPosition = transform.position;
    }

    public void DrawNewLine(Color col, Vector3[] coords)
    {
        var newLine = Instantiate(linePrefab).GetComponent<LineRenderer>();
        newLine.positionCount = coords.Length;
        newLine.SetPositions(coords);
        newLine.startColor = col;
        newLine.endColor = col;
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
        foreach (LineRenderer line in temporaryLines)
        {
            Destroy(line.gameObject);
        }
        temporaryLines.Clear();
    }

    private void OnEnable()
    {
        rightController = referenceManager.rightController;
        rightControllerTransform = rightController.gameObject.transform;
        lastPosition = transform.position;
    }

    private void OnDisable()
    {
        skipNextDraw = false;
        for (int i = 0; i < trailLines.Length; i++)
        {
            Destroy(trailLines[i]);
            trailLines[i] = null;
        }
    }

    private LineRenderer SpawnNewLine()
    {
        var newLine = Instantiate(linePrefab).GetComponent<LineRenderer>();
        newLine.SetPositions(new Vector3[] { lastPosition, transform.position });
        newLine.startColor = LineColor;
        newLine.endColor = LineColor;
        return newLine;
    }
}
