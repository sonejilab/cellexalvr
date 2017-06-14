using System.IO;
using UnityEngine;

public class ChangeImage : MonoBehaviour {
    public Texture texture;
	public GameObject heatBoard;

	// Use this for initialization
	void Awake () {
		heatBoard.SetActive (false);
    }

    // Update is called once per frame
    void Update () {

	}

    public void UpdateImage(string filepath)
    {
        byte[] fileData = File.ReadAllBytes(filepath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
    }

	public void ToggleHeatBoard(){
		heatBoard.SetActive (!heatBoard.activeSelf);
	}
}
