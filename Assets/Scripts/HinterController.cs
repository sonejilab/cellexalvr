using UnityEngine;
using System.Collections;

public class HinterController : MonoBehaviour
{

    public GameObject hinter;
    private Vector3 hiddenPosition;
    private Vector3 showingPosition;
    // Use this for initialization
    void Start()
    {
        hiddenPosition = new Vector3(0, 0, 0);
        showingPosition = new Vector3(0, -50f, 0);
        StartCoroutine(ShowHinter(5f));
    }

    private IEnumerator ShowHinter(float seconds)
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(MoveHinter(showingPosition, 0.5f));
        yield return new WaitForSeconds(seconds);
        StartCoroutine(MoveHinter(hiddenPosition, 0.5f));
        yield return new WaitForSeconds(0.5f);
        hinter.SetActive(false);
    }

    private IEnumerator MoveHinter(Vector3 newPosition, float inTime)
    {
        Vector3 startPosition = hinter.transform.localPosition;
        float t = 0f;
        do
        {
            hinter.transform.localPosition = Vector3.Lerp(startPosition, newPosition, Mathf.SmoothStep(0f, 1f, t));
            t += Time.deltaTime / inTime;
            yield return null;

        } while (t < 1);
    }


    public void HideHinter()
    {
        StopAllCoroutines();
        hinter.SetActive(false);
    }
}
