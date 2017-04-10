using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A classs for reading a data file and creating GraphPoints at the correct locations
public class InputReader  : MonoBehaviour{
	
	public string fileName;
	public GraphManager manager;

	void Start() {
		
		// put each line into an array
		string[] lines = System.IO.File.ReadAllLines(fileName);
		foreach (string line in lines) {
			// the coordinates are split with tab characters
			string[] words = line.Split('\t');
			manager.addCell(words[0], float.Parse(words[1]), float.Parse(words[2]), float.Parse(words[3]));
		}
	}
}
