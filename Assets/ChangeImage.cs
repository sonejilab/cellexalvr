using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ChangeImage : MonoBehaviour {
    public Texture texture;

	// Use this for initialization
	void Start () {
        
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void updateImage(string filepath)
    {
        byte[] fileData = File.ReadAllBytes(filepath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
    }
}
