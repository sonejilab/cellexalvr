using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// This class holds the logic for burning and fading out a heatmap.
/// </summary>
public class HeatmapBurner : MonoBehaviour
{

    public GameObject firePrefab;
    public Material transparentMaterial;

    private GameObject fire;
    private float fadingSpeed = 0.5f;
    private Renderer rend;
    private Component[] childrenRenderers;
    private bool fadeHeatmap;
    private float fade = 0;

    void Start()
    {
        rend = GetComponent<Renderer>();
        childrenRenderers = GetComponentsInChildren<Renderer>();
    }

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
        //rend.material = transparentMaterial;
        //rend.material.SetTexture("_MainTex", gameObject.GetComponent<Heatmap>().texture);
        rend.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        rend.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        rend.material.SetInt("_ZWrite", 0);
        rend.material.DisableKeyword("_ALPHATEST_ON");
        rend.material.EnableKeyword("_ALPHABLEND_ON");
        rend.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        rend.material.renderQueue = 3000;
        foreach (Renderer renderer in childrenRenderers)
        {
            renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            renderer.material.SetInt("_ZWrite", 0);
            renderer.material.DisableKeyword("_ALPHATEST_ON");
            renderer.material.EnableKeyword("_ALPHABLEND_ON");
            renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            renderer.material.renderQueue = 3000;
        }
        fadeHeatmap = true;
        fire = Instantiate(firePrefab, gameObject.transform);
    }

    /// <summary>
    /// Changes the heatmap's alpha.
    /// </summary>
    void FadeHeatmap()
    {
        Color c = rend.material.color;
        rend.material.color = new Color(c.r, c.g, c.b, 1 - fade);
        foreach (Renderer renderer in childrenRenderers)
        {
            renderer.material.color = new Color(c.r, c.g, c.b, 1 - fade);
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
