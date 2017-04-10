using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A classs for reading a data file and creating GraphPoints at the correct locations
public class InputReader  : MonoBehaviour{
	
	public string CellCoordinatesFileName;
	//public string GeneExpressionFileName;
	public GraphManager manager;

	void Start() {
		
		// put each line into an array
		string[] lines = System.IO.File.ReadAllLines(CellCoordinatesFileName);

		UpdateMinMax (lines);
		foreach (string line in lines) {
			// the coordinates are split with tab characters
			string[] words = line.Split('\t');
			float[] coords = new float[3];
			manager.addCell(words[0], float.Parse(words[1]), float.Parse(words[2]), float.Parse(words[3]));
		}
	}

	/** 
	 * Determines the maximum and the minimum values of the dataset. 
	 * Will be used for the scaling part onto the graphArea.
	 **/
	void UpdateMinMax(string[] lines){
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
		manager.setMinMaxCoords (minCoordValues, maxCoordValues);
	}
}
