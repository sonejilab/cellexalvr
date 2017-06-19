using UnityEngine;
using System.Collections;
using System.IO;

// A classs for reading a data file and creating GraphPoints at the correct locations
public class InputReader : MonoBehaviour {

public GraphManager graphManager;
public CellManager cellManager;
public LoaderController loaderController;
public void ReadFolder(string path) {
	string[] geneexprFiles = Directory.GetFiles(path, "*.expr");

	if (geneexprFiles.Length != 1) {
		throw new System.InvalidOperationException("There must be exactly one gene expression data file");
	}

	string[] mdsFiles = Directory.GetFiles(path, "*.mds");
	StartCoroutine(ReadMDSFiles(mdsFiles, geneexprFiles[0], 25));

	// clear the runtimeGroups
	string[] txtList = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Assets\\Data\\runtimeGroups", "*.txt");
	foreach (string f in txtList) {
		File.Delete(f);
	}

}

IEnumerator ReadMDSFiles(string[] mdsFiles, string geneexprFilename, int itemsPerFrame) {
	int fileIndex = 0;
	foreach (string file in mdsFiles) {
		graphManager.CreateGraph(fileIndex);
		graphManager.SetActiveGraph(fileIndex);

		// put each line into an array
		string[] lines = System.IO.File.ReadAllLines(file);
		string[] geneLines = System.IO.File.ReadAllLines(geneexprFilename);
		UpdateMinMax(lines);

		for (int i = 0; i < lines.Length; i += itemsPerFrame) {
			for (int j = i; j < i + itemsPerFrame && j < lines.Length; ++j) {
				string line = lines[j];
				string[] words = line.Split('\t');
				// print(words[0]);
				graphManager.AddCell(words[0], float.Parse(words[1]), float.Parse(words[2]), float.Parse(words[3]));
			}
			yield return new WaitForEndOfFrame();
		}

		string[] cellNames = geneLines[0].Split('\t');

		// process each gene and its expression values
		for (int i = 1; i < geneLines.Length; i++) {
			string[] words = geneLines[i].Split('\t');
			// the gene name is always the first word on the line
			string geneName = words[0].ToLower();
			float minExpr = 10000f;
			float maxExpr = -1f;
			// find the largest and smallest expression
			for (int j = 1; j < words.Length; j++) {
				float expr = float.Parse(words[j]);
				if (expr > maxExpr) {
					maxExpr = expr;
				}
				if (expr < minExpr) {
					minExpr = expr;
				}
			}
			// figure out how much gene expression each material represents
			float binSize = (maxExpr - minExpr) / 30;
			// for each cell, set its gene expression
			for (int k = 1; k < words.Length; k++) {
				int binIndex = 0;
				float expr = float.Parse(words[k]);
				binIndex = (int)((expr - minExpr) / binSize);
				if (binIndex == 30) {
					binIndex--;
				}
				cellManager.SetGeneExpression(cellNames[k - 1], geneName, binIndex);
			}
		}
		fileIndex++;
	}
	loaderController.MoveLoader(new Vector3(0f, -1f, 0f), 8);
}

/// <summary>
/// Determines the maximum and the minimum values of the dataset.
/// Will be used for the scaling part onto the graphArea.
///</summary>

void UpdateMinMax(string[] lines) {
	Vector3 maxCoordValues = new Vector3 ();
	maxCoordValues.x = maxCoordValues.y = maxCoordValues.z = -1000000.0f;
	Vector3 minCoordValues = new Vector3 ();
	minCoordValues.x = minCoordValues.y = minCoordValues.z = 1000000.0f;
	foreach (string line in lines) {
		// the coordinates are split with tab characters
		string[] words = line.Split('\t');
		float[] coords = new float[3];
		coords[0] = float.Parse(words[1]);
		coords[1] = float.Parse(words[2]);
		coords[2] = float.Parse(words[3]);
		if (coords [0] < minCoordValues.x)
			minCoordValues.x = coords [0];
		if (coords [0] > maxCoordValues.x)
			maxCoordValues.x = coords [0];
		if (coords [1] < minCoordValues.y)
			minCoordValues.y = coords [1];
		if (coords [1] > maxCoordValues.y)
			maxCoordValues.y = coords [1];
		if (coords [2] < minCoordValues.z)
			minCoordValues.z = coords [2];
		if (coords [2] > maxCoordValues.z)
			maxCoordValues.z = coords [2];

	}
	graphManager.SetMinMaxCoords (minCoordValues, maxCoordValues);
}
}
