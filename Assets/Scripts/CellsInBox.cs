using UnityEngine;

public class CellsInBox : MonoBehaviour {

private string directory;
private bool graphsLoaded = false;
// Use this for initialization
void Start () {

}

// Update is called once per frame
void Update () {

}

public bool GraphsLoaded() {
	return graphsLoaded;
}

public string GetDirectory() {
	graphsLoaded = true;
	return directory;
}

public void SetDirectory(string name) {
	directory = name;
}

}
