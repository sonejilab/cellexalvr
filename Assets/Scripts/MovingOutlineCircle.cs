using System.Collections;
using UnityEngine;

public class MovingOutlineCircle : MonoBehaviour
{
    public Transform camera;
    [Header("Numeric settings")]
    public float startScale = 100;
    public float minPulseScale = 5;
    public float maxPulseScale = 25;
    public float shrinkTime = 1f;
    public float pulseTime = 0.75f;
    public float pulses = 3f;

    private void OnValidate()
    {
        transform.localScale = new Vector3(startScale, startScale, startScale);
    }

    private void Start()
    {
        StartCoroutine(AnimateCoroutine());
    }

    private IEnumerator AnimateCoroutine()
    {
        float scale = startScale;
        float scaleDiff = startScale - minPulseScale;
        float t = 0;
        float halfPI = Mathf.PI / 2f;
        while (scale > minPulseScale)
        {
            scale -= scaleDiff * Time.deltaTime / shrinkTime;
            transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
        scaleDiff = maxPulseScale - minPulseScale;
        for (int i = 0; i < pulses; ++i)
        {
            t = 0;
            while (t < Mathf.PI)
            {
                t += halfPI * Time.deltaTime / pulseTime;
                scale = (maxPulseScale - minPulseScale) * Mathf.Sin(t) + minPulseScale;
                transform.localScale = new Vector3(scale, scale, scale);
                yield return null;
            }
        }
        Destroy(gameObject);
    }

    private void Update()
    {
        transform.LookAt(camera);
    }
}

