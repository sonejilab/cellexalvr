using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TextureDrawTool : MonoBehaviour
{
    private Transform rightHand;
    private bool drawing;
    private bool firstDraw = true;
    private Vector3 lastPosition;
    private Vector2 lastHit;
    private List<LineRenderer> temporaryLines = new List<LineRenderer>();
    [SerializeField] private LineRenderer linePrefab;
    private BoxCollider boxCollider => GetComponent<BoxCollider>();
    [SerializeField] private int brushWidth = 4;
    [SerializeField] private Color brushColor;
    private Color[] blockColors;


    private void Start()
    {
        rightHand = ReferenceManager.instance.rightController.transform;

        CellexalEvents.RightTriggerClick.AddListener(OnRightTriggerClick);
        CellexalEvents.RightTriggerUp.AddListener(OnRightTriggerUp);

        blockColors = new Color[brushWidth * 2 * brushWidth * 2];
        for (int i = 0; i < blockColors.Length; i++)
        {
            blockColors[i] = brushColor;
        }
    }

    private void Update()
    {
        if (drawing)
        {
            DrawOnTexture();
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            drawing = true;
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            drawing = false;
            firstDraw = true;
        }
    }

    private void OnRightTriggerClick()
    {
        drawing = true;
    }

    private void OnRightTriggerUp()
    {
        if (drawing)
        {
            drawing = false;
            //MergeLinesIntoOne();
        }
        firstDraw = true;
    }

    private void DrawLineRenderer()
    {
        //Ray ray = new Ray(rightHand.position, rightHand.forward);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out RaycastHit hit, 0.1f, 1 << LayerMask.NameToLayer("EnvironmentButtonLayer"));
        if (hit.collider)
        {
            //Vector3 pointInDrawableSpace = hit.collider.transform.InverseTransformPoint(hit.point);
            if (firstDraw)
            {
                lastPosition = hit.point;
                firstDraw = false;
            }
            temporaryLines.Add(SpawnNewLine(brushColor, new Vector3[] { lastPosition, hit.point }));
            lastPosition = hit.point;
        }
    }

    private void DrawOnTexture()
    {
        Ray ray = new Ray(rightHand.position, rightHand.forward);
        Physics.Raycast(ray, out RaycastHit hit, 0.08f, 1 << LayerMask.NameToLayer("EnvironmentButtonLayer"));
        //var pos = Mouse.current.position.ReadValue();
        //Ray ray = Camera.main.ScreenPointToRay(pos);
        //Physics.Raycast(ray, out RaycastHit hit, 1f, 1 << LayerMask.NameToLayer("EnvironmentButtonLayer"));
        if (hit.collider && hit.collider.TryGetComponent(out DrawableTexture drawableTexture))
        {
            if (firstDraw)
            {
                lastHit = hit.textureCoord;
                firstDraw = false;
            }

            float lineLength = Vector2.Distance(hit.textureCoord * 1000, lastHit * 1000);
            int lerpCountAdjustNum = 3;
            int lerpCount = Mathf.CeilToInt(lineLength / lerpCountAdjustNum);
            for (int i = 1; i <= lerpCount; i++)
            {
                float lerpWeight = (float)i / lerpCount;
                var lerpPosition = Vector2.Lerp(lastHit * 1000, hit.textureCoord * 1000, lerpWeight);
                //drawableTexture.texture.SetPixels((int)lerpPosition.x - 5, (int)lerpPosition.y - 5, 10, 10, blockColors2);
                //drawableTexture.texture.Apply();
                drawableTexture.texture.SetPixels((int)lerpPosition.x - brushWidth, (int)lerpPosition.y - brushWidth, brushWidth * 2, brushWidth * 2, blockColors);
                drawableTexture.texture.Apply();

            }
            lastHit = hit.textureCoord;
        }
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


        LineRenderer newLine = SpawnNewLine(Color.white, newLinePositions);
        //lines.Add(newLine);
        Vector3 center = newLine.GetPosition(newLine.positionCount - 1);
        Vector3 halfExtents = Vector3.one * 0.1f;
        LayerMask layerMask = 1 << LayerMask.NameToLayer("GraphLayer");
        Collider[] collidesWith = Physics.OverlapBox(center, halfExtents, Quaternion.identity, layerMask, QueryTriggerInteraction.Collide);
        if (collidesWith.Length > 0)
        {
            newLine.transform.parent = collidesWith[0].transform;
        }

        foreach (LineRenderer line in temporaryLines)
        {
            Destroy(line.gameObject);
        }

        temporaryLines.Clear();
    }


    private LineRenderer SpawnNewLine(Color col, Vector3[] positions)
    {
        var newLine = Instantiate(linePrefab);

        newLine.positionCount = positions.Length;
        newLine.SetPositions(positions);
        newLine.startColor = col;
        newLine.endColor = col;
        return newLine;
    }

    private LineRenderer SpawnNewLine(Color col, Vector3[] positions, Transform parent)
    {
        var newLine = Instantiate(linePrefab);
        newLine.useWorldSpace = false;
        newLine.transform.parent = parent;
        newLine.transform.localPosition = Vector3.zero;
        newLine.transform.localRotation = Quaternion.identity;
        newLine.positionCount = positions.Length;
        newLine.SetPositions(positions);
        newLine.startColor = col;
        newLine.endColor = col;
        return newLine;
    }
}
