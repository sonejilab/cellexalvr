using UnityEngine;
using System.Collections;

public class RotateMenu : MonoBehaviour
{

    private float finalZRotation = 0;
    private bool isRotating = false;
    private Vector3 fromAngle;
    public int rotation
    {
        get; private set;
    }

    void Start()
    {
        rotation = 0;
    }
    public void RotateRight()
    {
        if (!isRotating)
        {
            if (rotation == 3)
                rotation = 0;
            else
                rotation++;

            StartCoroutine(RotateMe(-90f, 0.15f));
        }
    }

    public void RotateLeft()
    {
        if (!isRotating)
        {
            if (rotation == 0)
                rotation = 3;
            else
                rotation--;
            StartCoroutine(RotateMe(90f, 0.15f));
        }
    }

    IEnumerator RotateMe(float zAngles, float inTime)
    {
        isRotating = true;
        // how much we have rotated so far
        float rotatedTotal = 0;
        float zAnglesAbs = Mathf.Abs(zAngles);
        // how much we should rotate each frame
        float rotationPerFrame = zAngles / (zAnglesAbs * inTime);
        //float rotationPerFrame = yAngles >= 0 ? 1 / inTime : -1 / inTime;
        while (rotatedTotal < zAnglesAbs && rotatedTotal > -zAnglesAbs)
        {
            rotatedTotal += rotationPerFrame;
            // if we are about to rotate it too far
            if (rotatedTotal > zAnglesAbs || rotatedTotal < -zAnglesAbs)
            {
                // only rotate the menu as much as there is left to rotate
                transform.Rotate(0, 0, rotationPerFrame - (rotatedTotal - zAngles));

            }
            else
            {
                transform.Rotate(0, 0, rotationPerFrame);
            }
            yield return null;
        }
        // fromAngle = transform.rotation.eulerAngles;
        isRotating = false;
    }

}
