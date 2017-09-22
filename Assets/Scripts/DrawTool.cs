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
        lastPosition = transform.position;
        controllerMenuCollider = referenceManager.controllerMenuCollider;
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        var device = SteamVR_Controller.Input((int)rightController.index);

        if (drawing)
        {
            // this happens every frame the trigger is pressed
            var newLine = SpawnNewLine(LineColor, new Vector3[] { lastPosition, transform.position });
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
            trailLines[temporaryLinesIndex] = SpawnNewLine(LineColor, new Vector3[] { lastPosition, transform.position });
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
            // this happens only once when you press the trigger
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
                MergeLinesIntoOne();
            }
        }
        lastPosition = transform.position;
    }

    /// <summary>
    /// Helper method to merge all spawned lines into one.
    /// </summary>
    private void MergeLinesIntoOne()
    {
        Vector3[] newLinePositions = new Vector3[temporaryLines.Count + 1];
        // the network can't send Vector3 so we have to divide the array to 3 float arrays
        float[] xcoords = new float[temporaryLines.Count + 1];
        float[] ycoords = new float[temporaryLines.Count + 1];
        float[] zcoords = new float[temporaryLines.Count + 1];
        // set the starting position
        newLinePositions[0] = temporaryLines[0].GetPosition(0);
        xcoords[0] = newLinePositions[0].x;
        ycoords[0] = newLinePositions[0].y;
        zcoords[0] = newLinePositions[0].z;
        // now take every line's end position and stitch them together to one long line
        for (int i = 1; i <= temporaryLines.Count; i++)
        {
            newLinePositions[i] = temporaryLines[i - 1].GetPosition(1);
            xcoords[i] = newLinePositions[i - 1].x;
            ycoords[i] = newLinePositions[i - 1].y;
            zcoords[i] = newLinePositions[i - 1].z;
        }
        referenceManager.gameManager.InformDrawLine(LineColor.r, LineColor.g, LineColor.b, xcoords, ycoords, zcoords);

        SpawnNewLine(LineColor, newLinePositions);
        foreach (LineRenderer line in temporaryLines)
        {
            Destroy(line.gameObject);
        }
        temporaryLines.Clear();
    }

    /// <summary>
    /// Draws a new line.
    /// </summary>
    /// <param name="col"> The line's color. </param>
    /// <param name="coords"> An array with the world space coordinates for each point the line should pass through. </param>
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
        foreach (LineRenderer line in lines)
        {
            Destroy(line.gameObject);
        }
        lines.Clear();
    }

    private void OnEnable()
    {
        rightController = referenceManager.rightController;
        lastPosition = transform.position;
    }

    private void OnDisable()
    {
        skipNextDraw = false;
        for (int i = 0; i < trailLines.Length; i++)
        {
            if (trailLines[i])
            {
                Destroy(trailLines[i].gameObject);
                trailLines[i] = null;
            }
        }
    }

    /// <summary>
    /// Helper method to create a new line.
    /// </summary>
    /// <param name="col"> The line's color. </param>
    /// <param name="positions"> An array containing the world space positions that the line should pass through. </param>
    /// <returns> The new line. </returns
    private LineRenderer SpawnNewLine(Color col, Vector3[] positions)
    {
        var newLine = Instantiate(linePrefab).GetComponent<LineRenderer>();
        newLine.positionCount = positions.Length;
        newLine.SetPositions(positions);
        newLine.startColor = col;
        newLine.endColor = col;
        return newLine;
    }
}
