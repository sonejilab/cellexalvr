using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapBurner : MonoBehaviour {

	public GameObject SelectionTool;
	public GameObject firePrefab;
	public Material originalMaterial;
	public Material transparentMaterial;
	public float fadingTime = 0.1f;
	Renderer rend;
	bool fadeHeatmap;
	float t = 0;

	// Use this for initialization
	void Start () {
		rend = GetComponent<Renderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (fadeHeatmap) {
			FadeHeatmap ();
		}
	}
		
	void BurnHeatmap() {
		print ("heatmap grabbed & down pressed!");
		fadeHeatmap = true;
		Vector3 heatmapScale = transform.localScale;
		Vector3 heatmapPosition = transform.position;
		Vector3 firePosition = new Vector3(heatmapPosition.x, heatmapPosition.y + 2.5f, heatmapPosition.z);
		GameObject fire = Instantiate (firePrefab, heatmapPosition, transform.rotation);
		fire.transform.localScale = new Vector3(20 * heatmapScale.x, fire.transform.localScale.y, 10 * heatmapScale.z);
		fire.active = true;
		this.GetComponents<AudioSource> () [1].Play (10000);
		Destroy (this, 2.5f);
		Destroy (fire, 2.5f);
		//SelectionTool.GetComponent<SelectionToolHandler>().heatmapGrabbed = false;
		//SelectionTool.GetComponent<SelectionToolHandler>().UpdateButtonIcons ();
	}

	void FadeHeatmap() {
		print ("trying to fade out heatmap!");
		Material heatmapMaterial = rend.material;
		rend.material.Lerp(heatmapMaterial, transparentMaterial , t);
		t = t + fadingTime * Time.deltaTime;
		if (t >= 1) {
			fadeHeatmap = false;
			t = 0;
		}
		print (t);
	}

}
