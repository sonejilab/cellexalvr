using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawableTexture : MonoBehaviour
{
    public Texture2D texture;


    private void Start()
    {
        texture = new Texture2D(1000, 1000, TextureFormat.ARGB32, false);
        Color[] colors = new Color[texture.width * texture.height];
        texture.SetPixels(colors);
        texture.Apply();
        GetComponent<MeshRenderer>().material.mainTexture = texture;
    }

    private void Update()
    {
        
    }
}
