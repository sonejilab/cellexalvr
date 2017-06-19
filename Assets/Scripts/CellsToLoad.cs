using UnityEngine;

public class CellsToLoad : MonoBehaviour {

private string directory;
private bool graphsLoaded = false;
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
