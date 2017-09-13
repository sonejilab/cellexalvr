using UnityEngine;

/// <summary>
/// This class holds the logic for burning and fading out a heatmap.
/// </summary>
public class HeatmapBurner : MonoBehaviour
{

    public GameObject firePrefab;
    public Material originalMaterial;
    public Material transparentMaterial;
    private GameObject fire;
    private float fadingSpeed = 0.5f;
    private Renderer rend;
    private Component[] childrenRenderers;
    private bool fadeHeatmap;
    private float fade = 0;

    // Use this for initialization
    void Start()
    {
        rend = GetComponent<Renderer>();
        childrenRenderers = GetComponentsInChildren<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (fadeHeatmap)
        {
            FadeHeatmap();
        }
    }

    /// <summary>
    /// Spawns a fire on top of the heatmap and fades the heatmap away until it is destroyed.
    /// </summary>
    public void BurnHeatmap()
    {
        fadeHeatmap = true;
        Vector3 heatmapScale = transform.localScale;
        Vector3 heatmapPosition = transform.position;
        fire = Instantiate(firePrefab, heatmapPosition + new Vector3(0, 5 * heatmapScale.z, 0), new Quaternion(0, 0, 0, 0));
        fire.transform.localScale = new Vector3(5 * heatmapScale.x, 0.1f, heatmapScale.z);
        fire.transform.Rotate(new Vector3(270.0f, transform.localEulerAngles.y, 0));
        this.GetComponents<AudioSource>()[0].PlayDelayed(10000);
    }

    void FadeHeatmap()
    {
        rend.material.Lerp(originalMaterial, transparentMaterial, fade);
        foreach (Renderer renderer in childrenRenderers)
        {
            rend.material.Lerp(originalMaterial, transparentMaterial, fade);
        }
        fade = fade + fadingSpeed * Time.deltaTime;
        if (fade >= 1)
        {
            fadeHeatmap = false;
            Destroy(this.gameObject);
            Destroy(fire);
            fade = 0;
        }
    }
}
