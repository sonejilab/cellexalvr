using System.Collections;
using UnityEngine;

/// <summary>
/// This class represents the button that moves the folders when a controller is moved inside of it.
/// </summary>
public class MoveFoldersDownButton : MonoBehaviour
{
    public Transform folderList;
    public int moveTime;
    public float[] dY;

    private Vector3 moveDistanceWhenPressed = new Vector3(0f, -0.3f, 0f);
    private Vector3 colliderMoveDistanceWhenPressed = new Vector3(0f, 0f, 0.3f);
    private BoxCollider boxCollider;
    private MeshRenderer meshRenderer;
    private Color emissionColor;
    private bool increaseEmission;
    private int emissionPropertyID;
    private bool controllerInside = false;
    private bool coroutineRunning = false;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        emissionPropertyID = Shader.PropertyToID("_EmissionColor");
        boxCollider = GetComponent<BoxCollider>();
        // calculate how much it the folders should move every frame once
        dY = new float[moveTime];
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
        if (other.gameObject.CompareTag("Smaller Controller Collider"))
        {
            controllerInside = true;
            StartCoroutine(Blink());
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

    private IEnumerator Blink()
    {
        transform.localPosition += moveDistanceWhenPressed;
        boxCollider.center += colliderMoveDistanceWhenPressed;
        emissionColor = new Color(0.15f, 0.15f, 0.15f);
        meshRenderer.material.SetColor(emissionPropertyID, emissionColor);
        while (controllerInside)
        {
            if (increaseEmission)
            {
                emissionColor.r = emissionColor.r + 0.025f;
                emissionColor.g = emissionColor.g + 0.025f;
                emissionColor.b = emissionColor.b + 0.025f;
                if (emissionColor.r > 0.6f)
                {
                    increaseEmission = false;
                }
            }
            else
            {
                emissionColor.r = emissionColor.r - 0.025f;
                emissionColor.g = emissionColor.g - 0.025f;
                emissionColor.b = emissionColor.b - 0.025f;
                if (emissionColor.r <= 0.15)
                {
                    increaseEmission = true;
                }
            }
            meshRenderer.material.SetColor(emissionPropertyID, emissionColor);
            yield return null;
        }
        transform.localPosition -= moveDistanceWhenPressed;
        boxCollider.center -= colliderMoveDistanceWhenPressed;
        meshRenderer.material.SetColor(emissionPropertyID, Color.black);
    }
}