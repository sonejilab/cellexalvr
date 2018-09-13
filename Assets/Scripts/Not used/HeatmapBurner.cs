using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// This class holds the logic for burning and fading out a heatmap.
/// </summary>
public class HeatmapBurner : MonoBehaviour
{

    public GameObject deletePrefab;
    public Material transparentMaterial;

    private GameObject deleteTool;
    private float fadingSpeed = 0.5f;
    private Renderer rend;
    private Component[] childrenRenderers;
    private bool fadeHeatmap;
    private float fade = 0;
    private Transform target;
    private float speed;
    public float targetScale;
    public float shrinkSpeed;

    void Start()
    {
        rend = GetComponent<Renderer>();
        childrenRenderers = GetComponentsInChildren<Renderer>();
        speed = 0.5f;
        shrinkSpeed = 3f;
        targetScale = 0.05f;
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
    public void BurnHeatmap(Transform device)
    {
        ////rend.material = transparentMaterial;
        ////rend.material.SetTexture("_MainTex", gameObject.GetComponent<Heatmap>().texture);
        //rend.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        //rend.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        //rend.material.SetInt("_ZWrite", 0);
        //rend.material.DisableKeyword("_ALPHATEST_ON");
        //rend.material.EnableKeyword("_ALPHABLEND_ON");
        //rend.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        //rend.material.renderQueue = 3000;
        //foreach (Renderer renderer in childrenRenderers)
        //{
        //    renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        //    renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        //    renderer.material.SetInt("_ZWrite", 0);
        //    renderer.material.DisableKeyword("_ALPHATEST_ON");
        //    renderer.material.EnableKeyword("_ALPHABLEND_ON");
        //    renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        //    renderer.material.renderQueue = 3000;
        //}
        //fire = Instantiate(firePrefab, gameObject.transform);
        target = device;
        fadeHeatmap = true;
        CellexalEvents.HeatmapBurned.Invoke();
    }

    /// <summary>
    /// Changes the heatmap's alpha.
    /// </summary>
    void FadeHeatmap()
    {
        //anim.enabled = true;
        //anim.Play("remove_heatmap");
        //anim.SetFloat()
        Color c = rend.material.color;
        rend.material.color = new Color(c.r, c.g, c.b, 1 - fade);
        foreach (Renderer renderer in childrenRenderers)
        {
            renderer.material.color = new Color(c.r, c.g, c.b, 1 - fade);
        }
        fade = fade + fadingSpeed * Time.deltaTime;
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
        transform.localScale -= Vector3.one * Time.deltaTime * shrinkSpeed;
        if (transform.localScale.x <= targetScale)
        {
            fadeHeatmap = false;
            Destroy(this.gameObject);
            //anim.Play("New State");
            //Destroy(fire);
            fade = 0;
        }
    }
}
